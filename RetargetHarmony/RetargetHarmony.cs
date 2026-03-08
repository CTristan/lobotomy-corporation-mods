// SPDX-License-Identifier: MIT

#pragma warning disable CA1724 // Type name conflicts with namespace name (required by BepInEx contract)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil;

namespace RetargetHarmony
{
    public static class RetargetHarmony
    {
        // Static log source managed by BepInEx framework (long-lived)
        private static readonly ManualLogSource Log = Logger.CreateLogSource("RetargetHarmony");

        // BaseMods directory path — hardcoded relative to game root
        // Uses fallback to BepInExRootPath when GameRootPath is not available (e.g., in tests)
        public static string BaseModsPath
        {
            get
            {
                string gameRoot;
                if (!string.IsNullOrEmpty(Paths.GameRootPath))
                {
                    gameRoot = Paths.GameRootPath;
                }
                else if (!string.IsNullOrEmpty(Paths.BepInExRootPath))
                {
                    gameRoot = Path.GetFullPath(Path.Combine(Paths.BepInExRootPath, ".."));
                }
                else
                {
                    // Fallback for test context where BepInEx paths are not initialized
                    // Use current directory as the game root
                    gameRoot = Environment.CurrentDirectory;
                }

                return Path.GetFullPath(Path.Combine(Path.Combine(gameRoot, "LobotomyCorp_Data"), "BaseMods"));
            }
        }

        // Flag to control whether assembly cache should be saved
        private static volatile bool s_shouldSaveCache = true;

        // Guard flag to prevent recursion in PostFindPluginTypes
        private static volatile bool s_isProcessing;

        // BepInEx 5 requires static property returning names of assemblies to patch
        public static IEnumerable<string> TargetDLLs
        {
            get
            {
                yield return "Assembly-CSharp.dll";
                yield return "LobotomyBaseModLib.dll";
            }
        }

        // BepInEx 5 requires static Patch method
        public static void Patch(AssemblyDefinition asm)
        {
            if (asm == null)
            {
                throw new System.ArgumentNullException(nameof(asm));
            }

            var refs = asm.MainModule.AssemblyReferences;
            var changed = false;

            // 1) Find all Harmony references (both old and potentially already retargeted)
            var harmonyRefs = refs.Where(r => r.Name == "0Harmony" || r.Name == "0Harmony109").ToList();

            // Defensive check: ToList() should never return null, but guard against unexpected behavior
            if (harmonyRefs != null && harmonyRefs.Count > 0)
            {
                // Ensure the first reference points to 0Harmony109
                if (harmonyRefs[0].Name != "0Harmony109")
                {
                    harmonyRefs[0].Name = "0Harmony109";
                    changed = true;
                }

                // 2) Defensive sweep: remove any duplicate Harmony metadata references
                // Safe to modify refs while iterating over harmonyRefs (a separate list copy)
                for (int i = 1; i < harmonyRefs.Count; i++)
                {
                    refs.Remove(harmonyRefs[i]);
                    changed = true;
                }
            }

            if (changed)
            {
                Log.LogInfo($"Rewrote reference 0Harmony -> 0Harmony109 in {asm.Name.Name}");
            }
            else
            {
                Log.LogInfo($"No 0Harmony reference found in {asm.Name.Name}; nothing changed.");
            }
        }

        // BepInEx preloader patcher lifecycle: Finish() runs after all Patch() calls complete,
        // but before the chainloader starts. Use this to register additional plugin directories.
        public static void Finish()
        {
            // Register assembly resolve handler for BaseMods dependencies
            // BepInEx's TypeLoader.AssemblyResolve uses Mono.Cecil's AssemblyResolveEventHandler
            TypeLoader.AssemblyResolve += OnAssemblyResolve;

            // Create Harmony instance to patch TypeLoader
            var harmony = new Harmony("com.lobotomycorp.retargetharmony.chainloaderhook");

            // Patch FindPluginTypes to include BaseMods directory
            var findPluginTypesMethod = AccessTools.Method(typeof(TypeLoader), nameof(TypeLoader.FindPluginTypes))
                .MakeGenericMethod(typeof(PluginInfo));
            harmony.Patch(
                findPluginTypesMethod,
                new HarmonyMethod(AccessTools.Method(typeof(RetargetHarmony), nameof(PreFindPluginTypes))),
                new HarmonyMethod(AccessTools.Method(typeof(RetargetHarmony), nameof(PostFindPluginTypes))));

            // Patch SaveAssemblyCache to prevent premature cache writes
            var saveAssemblyCacheMethod = AccessTools.Method(typeof(TypeLoader), nameof(TypeLoader.SaveAssemblyCache))
                .MakeGenericMethod(typeof(PluginInfo));
            harmony.Patch(
                saveAssemblyCacheMethod,
                new HarmonyMethod(AccessTools.Method(typeof(RetargetHarmony), nameof(OnSaveAssemblyCache))));

            Log.LogInfo($"Registered BaseMods plugin directory: {BaseModsPath}");
        }

        // Prefix for TypeLoader.FindPluginTypes — disables cache saving when scanning the main plugin directory
        private static void PreFindPluginTypes(string directory)
        {
            if (!string.Equals(directory, Paths.PluginPath, StringComparison.Ordinal))
            {
                return;
            }

            // Prevent saving cache to avoid overwriting it when loading all mods
            s_shouldSaveCache = false;
        }

        // Postfix for TypeLoader.FindPluginTypes — merges BaseMods plugins into results and saves merged cache
        private static void PostFindPluginTypes(
            Dictionary<string, List<PluginInfo>> __result,
            string directory,
            Func<TypeDefinition, PluginInfo> typeSelector,
            Func<AssemblyDefinition, bool> assemblyFilter,
            string cacheName)
        {
            // Prevent recursion — only handle the main plugin path
            if (!string.Equals(directory, Paths.PluginPath, StringComparison.Ordinal) || s_isProcessing)
            {
                return;
            }

            // Check if BaseMods directory exists
            if (!Directory.Exists(BaseModsPath))
            {
                Log.LogInfo($"BaseMods directory does not exist: {BaseModsPath}");
                s_shouldSaveCache = true;
                return;
            }

            Log.LogInfo($"Finding plugins from BaseMods: {BaseModsPath}");

            // Set guard flag to prevent recursion
            s_isProcessing = true;
            try
            {
                // Find plugins in BaseMods directory
                var baseModsResult = TypeLoader.FindPluginTypes(BaseModsPath, typeSelector, assemblyFilter, cacheName);

                // Merge results from BaseMods into the main results
                foreach (var kv in baseModsResult)
                {
                    __result[kv.Key] = kv.Value;
                }

                // Re-enable cache saving and save the merged cache
                s_shouldSaveCache = true;
                if (cacheName != null)
                {
                    TypeLoader.SaveAssemblyCache(cacheName, __result);
                }
            }
            finally
            {
                s_isProcessing = false;
            }
        }

        // Prefix for TypeLoader.SaveAssemblyCache — skips saving when flag is false
        private static bool OnSaveAssemblyCache()
        {
            return s_shouldSaveCache;
        }

        // Assembly resolve handler — tries to resolve assemblies from BaseMods directory
        // BepInEx's TypeLoader.AssemblyResolve uses Mono.Cecil's AssemblyResolveEventHandler
        // that takes AssemblyNameReference and returns AssemblyDefinition
        private static AssemblyDefinition OnAssemblyResolve(object sender, AssemblyNameReference reference)
        {
            try
            {
                // Try to resolve from BaseMods directory
                return TryResolveCecilAssembly(reference.Name, BaseModsPath);
            }
            catch (IOException)
            {
                // File access error - return null to let other handlers try
                return null;
            }
            catch (ArgumentException)
            {
                // Malformed assembly name - return null to let other handlers try
                return null;
            }
            catch (BadImageFormatException)
            {
                // Not a valid .NET assembly - return null to let other handlers try
                return null;
            }
        }

        // Helper method to resolve an assembly from a specific directory using Mono.Cecil
        private static AssemblyDefinition TryResolveCecilAssembly(string assemblyName, string directory)
        {
            if (!Directory.Exists(directory))
            {
                return null;
            }

            var dllPath = Path.Combine(directory, assemblyName + ".dll");
            if (!File.Exists(dllPath))
            {
                return null;
            }

            try
            {
                return AssemblyDefinition.ReadAssembly(dllPath);
            }
            catch (BadImageFormatException)
            {
                // Not a valid .NET assembly
                return null;
            }
            catch (IOException)
            {
                // File access error
                return null;
            }
        }
    }
}
