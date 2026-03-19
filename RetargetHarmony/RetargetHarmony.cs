// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Globalization;
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
        private const string AuditLogFileName = "patched_mods.log";

        // Static log source managed by BepInEx framework (long-lived)
        private static readonly ManualLogSource Log = Logger.CreateLogSource("RetargetHarmony");

        // Testability: override BaseMods directory path
        public static string BaseModsPathOverride { get; set; }

        // BaseMods directory path — hardcoded relative to game root
        public static string BaseModsPath
        {
            get
            {
                if (!string.IsNullOrEmpty(BaseModsPathOverride))
                {
                    return BaseModsPathOverride;
                }

                return Path.GetFullPath(Path.Combine(Path.Combine(GameRootPath, "LobotomyCorp_Data"), "BaseMods"));
            }
        }

        // Flag to control whether assembly cache should be saved
        private static volatile bool s_shouldSaveCache = true;

        // Guard flag to prevent recursion in PostFindPluginTypes
        private static volatile bool s_isProcessing;

        // Configuration: enable patching of DLLs in BaseMods directory
        // Set via BepInEx config file (RetargetHarmony.cfg)
        private static bool s_patchBaseMods;
        private static bool s_configInitialized;

        // Public override for testing - bypasses config file
        public static bool PatchBaseModsOverride { get; set; }

        // BepInEx 5 requires static property returning names of assemblies to patch
        public static IEnumerable<string> TargetDLLs
        {
            get
            {
                yield return "Assembly-CSharp.dll";
                yield return "LobotomyBaseModLib.dll";
            }
        }

        /// <summary>
        /// Publicly accessible config value for PatchBaseMods setting.
        /// Returns override value if set for testing, otherwise returns config value.
        /// </summary>
        public static bool PatchBaseModsEnabled => PatchBaseModsOverride || s_patchBaseMods;

        /// <summary>
        /// Clears the PatchBaseMods configuration to force re-initialization.
        /// Used for testing purposes.
        /// </summary>
        public static void ClearPatchBaseModsConfig()
        {
            s_configInitialized = false;
            s_patchBaseMods = false;
        }

        // Testability: override audit log directory path
        public static string AuditLogDirectoryOverride { get; set; }

        /// <summary>
        /// Gets the game root directory path.
        /// </summary>
        public static string GameRootPath
        {
            get
            {
                if (!string.IsNullOrEmpty(Paths.GameRootPath))
                {
                    return Paths.GameRootPath;
                }

                if (!string.IsNullOrEmpty(Paths.BepInExRootPath))
                {
                    return Path.GetFullPath(Path.Combine(Paths.BepInExRootPath, ".."));
                }

                return Environment.CurrentDirectory;
            }
        }

        /// <summary>
        /// Writes a patched mod's relative path to the audit log, deduplicating entries.
        /// </summary>
        private static void WriteAuditLog(string assemblyName)
        {
            try
            {
                // Compute relative path from game root
                var relativePath = Path.Combine(Path.Combine("LobotomyCorp_Data", "BaseMods"), assemblyName + ".dll");

                // Determine audit log directory
                string logDir;
                if (AuditLogDirectoryOverride != null)
                {
                    logDir = AuditLogDirectoryOverride;
                }
                else
                {
                    var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
                    logDir = Path.Combine(assemblyDir, "logs");
                }

                if (!Directory.Exists(logDir))
                {
                    _ = Directory.CreateDirectory(logDir);
                }

                var logPath = Path.Combine(logDir, AuditLogFileName);

                // Read existing entries to deduplicate
                var existingEntries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (File.Exists(logPath))
                {
                    foreach (var line in File.ReadAllLines(logPath))
                    {
                        var trimmed = line.Trim();
                        if (!string.IsNullOrEmpty(trimmed))
                        {
                            _ = existingEntries.Add(trimmed);
                        }
                    }
                }

                if (existingEntries.Contains(relativePath))
                {
                    SafeTrace(string.Format(CultureInfo.InvariantCulture, "Audit log already contains: {0}", relativePath));
                    return;
                }

                File.AppendAllText(logPath, relativePath + Environment.NewLine);
                SafeInfo(string.Format(CultureInfo.InvariantCulture, "Audit log entry added: {0}", relativePath));
            }
            catch (Exception ex)
            {
                SafeWarn(string.Format(CultureInfo.InvariantCulture, "Failed to write audit log: {0}", ex.Message));
            }
        }

        // Safe logging wrapper - catches all exceptions to prevent crashing the patcher
        private static void SafeTrace(string message)
        {
            try { DebugLogger.Trace(message); }
            catch (Exception ex) { Log?.LogWarning($"SafeTrace failed: {ex.Message}"); }
        }

        private static void SafeDebug(string message)
        {
            try { DebugLogger.Debug(message); }
            catch (Exception ex) { Log?.LogWarning($"SafeDebug failed: {ex.Message}"); }
        }

        private static void SafeInfo(string message)
        {
            try { DebugLogger.Info(message); }
            catch (Exception ex) { Log?.LogWarning($"SafeInfo failed: {ex.Message}"); }
        }

        private static void SafeWarn(string message)
        {
            try { DebugLogger.Warn(message); }
            catch (Exception ex) { Log?.LogWarning($"SafeWarn failed: {ex.Message}"); }
        }

        private static void SafeError(string message)
        {
            try { DebugLogger.Error(message); }
            catch (Exception ex) { Log?.LogWarning($"SafeError failed: {ex.Message}"); }
        }

        // Lazy initialization for configuration - must be called from early entry point
        private static void InitializeConfig()
        {
            if (s_configInitialized)
            {
                return; // Already initialized
            }

            try
            {
                // Determine config path - check assembly directory first (patcher's own folder),
                // then fall back to BepInEx config path
                var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
                var assemblyConfigPath = Path.Combine(assemblyDir, "RetargetHarmony.cfg");

                string configPath;

                if (File.Exists(assemblyConfigPath))
                {
                    // Config found in assembly directory
                    configPath = assemblyConfigPath;
                }
                else if (!string.IsNullOrEmpty(Paths.ConfigPath))
                {
                    configPath = Path.Combine(Paths.ConfigPath, "RetargetHarmony.cfg");
                }
                else
                {
                    // Fall back to assembly directory
                    configPath = assemblyConfigPath;
                }

                // Generate default config file if it doesn't exist
                if (!File.Exists(configPath))
                {
                    var defaultConfig = @"# RetargetHarmony Configuration File
# Generated on first run

# Logging level: None, Trace, Debug, Info, Warn, Error
LogLevel = Warn

# Enable patching of DLLs in the BaseMods directory
PatchBaseMods = false
";
                    File.WriteAllText(configPath, defaultConfig);
                    SafeInfo(string.Format(CultureInfo.InvariantCulture, "Generated default config file: {0}", configPath));
                }

                // Parse the config file manually
                ParseConfigFile(configPath);

                SafeDebug(string.Format(CultureInfo.InvariantCulture,
                    "Configuration initialized: PatchBaseMods={0}",
                    s_patchBaseMods));
            }
            catch (Exception ex)
            {
                SafeWarn(string.Format(CultureInfo.InvariantCulture,
                    "Failed to initialize configuration, using defaults: {0}",
                    ex.Message));
            }
            finally
            {
                s_configInitialized = true;
            }
        }

        // Parse the config file and set config values
        private static void ParseConfigFile(string configPath)
        {
            if (!File.Exists(configPath))
            {
                return;
            }

            try
            {
                var lines = File.ReadAllLines(configPath);
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    // Skip comments and empty lines
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var separator = new[] { '=' };
                    var parts = trimmed.Split(separator, 2);
                    if (parts.Length != 2)
                    {
                        continue;
                    }

                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    switch (key)
                    {
                        case "PatchBaseMods":
                            if (bool.TryParse(value, out var patchBaseMods))
                            {
                                s_patchBaseMods = patchBaseMods;
                            }
                            break;

                        case "LogLevel":
                            // Pass to DebugLogger
                            DebugLogger.SetLogLevelFromConfig(value);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                SafeWarn(string.Format(CultureInfo.InvariantCulture, "Error parsing config file: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Core retargeting logic: rewrites Harmony references to 0Harmony109 and removes duplicates.
        /// Returns true if any changes were made.
        /// </summary>
        public static bool RetargetHarmonyReferences(AssemblyDefinition asm)
        {
            if (asm == null)
            {
                throw new ArgumentNullException(nameof(asm));
            }

            var refs = asm.MainModule.AssemblyReferences;

            // List all assembly references for debugging
            var allRefNames = refs.Select(r => r.Name).ToArray();
            SafeTrace(string.Format(CultureInfo.InvariantCulture, "Assembly references: [{0}]", string.Join(", ", allRefNames)));

            var changed = false;

            // 1) Find all Harmony references (both old and potentially already retargeted)
            // Skip 0Harmony references with Version.Major >= 2 — those are HarmonyX (Harmony 2.x)
            // and should NOT be retargeted to the Harmony 1.x shim (0Harmony109).
            List<AssemblyNameReference> harmonyRefs = refs.Where(r =>
                (r.Name == "0Harmony" && r.Version.Major < 2) ||
                r.Name == "0Harmony109" ||
                r.Name == "12Harmony" ||
                r.Name == "0Harmony12").ToList();

            SafeDebug(string.Format(CultureInfo.InvariantCulture, "Harmony references found: {0} - {1}", harmonyRefs.Count, string.Join(", ", harmonyRefs.Select(r => r.Name).ToArray())));

            // Defensive check: ToList() should never return null, but guard against unexpected behavior
            if (harmonyRefs != null && harmonyRefs.Count > 0)
            {
                // Ensure the first reference points to 0Harmony109
                if (harmonyRefs[0].Name != "0Harmony109")
                {
                    SafeDebug(string.Format(CultureInfo.InvariantCulture, "Rewriting reference {0} -> 0Harmony109", harmonyRefs[0].Name));
                    harmonyRefs[0].Name = "0Harmony109";
                    changed = true;
                }

                // 2) Defensive sweep: remove any duplicate Harmony metadata references
                // Safe to modify refs while iterating over harmonyRefs (a separate list copy)
                for (var i = 1; i < harmonyRefs.Count; i++)
                {
                    SafeTrace(string.Format(CultureInfo.InvariantCulture, "Removing duplicate Harmony reference: {0}", harmonyRefs[i].Name));
                    _ = refs.Remove(harmonyRefs[i]);
                    changed = true;
                }
            }

            return changed;
        }

        // BepInEx 5 requires static Patch method
        public static void Patch(AssemblyDefinition asm)
        {
            try
            {
                // Lazy initialize debug logger
                DebugLogger.Initialize(Log);

                // Lazy initialize configuration
                InitializeConfig();

                if (asm == null)
                {
                    throw new ArgumentNullException(nameof(asm));
                }

                SafeTrace(string.Format(CultureInfo.InvariantCulture, "Patch() entry with assembly: {0}", asm.Name.Name));

                var changed = RetargetHarmonyReferences(asm);

                if (changed)
                {
                    SafeInfo(string.Format(CultureInfo.InvariantCulture, "Rewrote Harmony reference(s) -> 0Harmony109 in {0}", asm.Name.Name));

                    // Write audit log for BaseMods DLLs (not the core game assemblies)
                    if (asm.Name.Name != "Assembly-CSharp" && asm.Name.Name != "LobotomyBaseModLib")
                    {
                        WriteAuditLog(asm.Name.Name);
                    }
                }
                else
                {
                    SafeInfo(string.Format(CultureInfo.InvariantCulture, "No Harmony reference found in {0}; nothing changed.", asm.Name.Name));
                }
            }
            catch (Exception ex)
            {
                SafeError(string.Format(CultureInfo.InvariantCulture, "Exception in Patch(): {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Harmony prefix for Assembly.LoadFile(string). Intercepts BaseMods DLL loads
        /// to retarget Harmony references in memory without writing to disk.
        /// </summary>
        /// <remarks>
        /// Uses positional parameter injection (__0) instead of named parameter to avoid
        /// HarmonyX "parameter not found" errors from name mismatches across runtimes.
        /// </remarks>
        // ReSharper disable InconsistentNaming
        public static bool OnAssemblyLoadFile(ref Assembly __result, string __0)
        // ReSharper restore InconsistentNaming
        {
            try
            {
                var assemblyFile = __0;

                // Only intercept loads from the BaseMods directory
                if (string.IsNullOrEmpty(assemblyFile) || !assemblyFile.StartsWith(BaseModsPath, StringComparison.OrdinalIgnoreCase))
                {
                    return true; // Let original proceed
                }

                SafeTrace(string.Format(CultureInfo.InvariantCulture, "OnAssemblyLoadFile() intercepting: {0}", assemblyFile));

                // Read file bytes (never write back)
                var originalBytes = File.ReadAllBytes(assemblyFile);

                // Load into Mono.Cecil for inspection
                using (var ms = new MemoryStream(originalBytes))
                {
                    using (var asm = AssemblyDefinition.ReadAssembly(ms))
                    {
                        var changed = RetargetHarmonyReferences(asm);
                        if (!changed)
                        {
                            SafeTrace(string.Format(CultureInfo.InvariantCulture, "No retargeting needed for: {0}", assemblyFile));
                            return true; // Let original proceed — no changes needed
                        }

                        // Write retargeted assembly to memory
                        using (var outputMs = new MemoryStream())
                        {
                            asm.Write(outputMs);
                            var patchedBytes = outputMs.ToArray();

                            SafeInfo(string.Format(CultureInfo.InvariantCulture, "Retargeted Harmony references in memory for: {0}", Path.GetFileName(assemblyFile)));

                            // Load from bytes — no disk write
                            __result = Assembly.Load(patchedBytes);

                            // Write audit log
                            var assemblyName = Path.GetFileNameWithoutExtension(assemblyFile);
                            WriteAuditLog(assemblyName);

                            return false; // Skip original Assembly.LoadFile
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // On any failure, fall through to original Assembly.LoadFile so the game still works
                SafeError(string.Format(CultureInfo.InvariantCulture, "OnAssemblyLoadFile() failed, falling through: {0}", ex.Message));
                return true;
            }
        }

        // BepInEx preloader patcher lifecycle: Finish() runs after all Patch() calls complete,
        // but before the chainloader starts. Use this to register additional plugin directories.
        public static void Finish()
        {
            try
            {
                // Lazy initialize debug logger (may not have been initialized if Patch() wasn't called)
                DebugLogger.Initialize(Log);

                SafeTrace("Finish() entry");

                // Create Harmony instance — needed for both neutralization and TypeLoader patching
                Harmony harmony = new Harmony("com.lobotomycorp.retargetharmony.chainloaderhook");

                SafeDebug("Created Harmony instance");

                // Neutralize BepInEx's HarmonyInteropFix FIRST — this is the most critical operation.
                // Must run before TypeLoader patching which can throw and prevent execution.
                // NeutralizeHarmonyInteropFix has its own try/catch that does NOT rethrow,
                // so a failure here won't prevent TypeLoader patching from proceeding.
                NeutralizeHarmonyInteropFix(harmony);

                // Register CLR-level assembly resolve handler for in-memory loaded assemblies.
                // When OnAssemblyLoadFile returns Assembly.Load(bytes), the loaded assembly has no
                // file location, so the CLR can't probe the original directory for dependencies.
                // This handler resolves missing assemblies from BaseMods subdirectories.
                AppDomain.CurrentDomain.AssemblyResolve += OnClrAssemblyResolve;

                SafeDebug("Registered AppDomain.AssemblyResolve handler for BaseMods dependencies");

                // Register assembly resolve handler for BaseMods dependencies
                // NOTE: This is NOT AppDomain.AssemblyResolve (System.Reflection).
                // BepInEx's TypeLoader.AssemblyResolve is a custom event using Mono.Cecil types:
                //   delegate: AssemblyDefinition handler(object sender, AssemblyNameReference reference)
                TypeLoader.AssemblyResolve += OnAssemblyResolve;

                SafeDebug("Registered AssemblyResolve handler");

                // Patch FindPluginTypes to include BaseMods directory
                var findPluginTypesMethod = AccessTools.Method(typeof(TypeLoader), nameof(TypeLoader.FindPluginTypes))
                    .MakeGenericMethod(typeof(PluginInfo));
                SafeTrace("Patching TypeLoader.FindPluginTypes (prefix)");
                _ = harmony.Patch(
                    findPluginTypesMethod,
                    new HarmonyMethod(AccessTools.Method(typeof(RetargetHarmony), nameof(PreFindPluginTypes))),
                    new HarmonyMethod(AccessTools.Method(typeof(RetargetHarmony), nameof(PostFindPluginTypes))));
                SafeDebug("Patched TypeLoader.FindPluginTypes successfully");

                // Patch SaveAssemblyCache to prevent premature cache writes
                var saveAssemblyCacheMethod = AccessTools.Method(typeof(TypeLoader), nameof(TypeLoader.SaveAssemblyCache))
                    .MakeGenericMethod(typeof(PluginInfo));
                SafeTrace("Patching TypeLoader.SaveAssemblyCache (prefix)");
                _ = harmony.Patch(
                    saveAssemblyCacheMethod,
                    new HarmonyMethod(AccessTools.Method(typeof(RetargetHarmony), nameof(OnSaveAssemblyCache))));
                SafeDebug("Patched TypeLoader.SaveAssemblyCache successfully");

                SafeInfo(string.Format(CultureInfo.InvariantCulture, "Registered BaseMods plugin directory: {0}", BaseModsPath));
            }
            catch (Exception ex)
            {
                SafeError(string.Format(CultureInfo.InvariantCulture, "Exception in Finish(): {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Unpatches BepInEx's HarmonyInteropFix from Assembly.LoadFile/LoadFrom and installs
        /// our own non-destructive prefix that retargets Harmony references in memory only.
        /// Falls back to patching HarmonyInterop.TryShim if unpatch fails.
        /// </summary>
        private static void NeutralizeHarmonyInteropFix(Harmony harmony)
        {
            try
            {
                SafeTrace("NeutralizeHarmonyInteropFix() entry");

                var loadFileMethod = typeof(Assembly).GetMethod("LoadFile", new Type[] { typeof(string) });
                var loadFromMethod = typeof(Assembly).GetMethod("LoadFrom", new Type[] { typeof(string) });

                if (loadFileMethod == null || loadFromMethod == null)
                {
                    SafeWarn("Could not find Assembly.LoadFile or Assembly.LoadFrom methods via reflection");
                    return;
                }

                // Unpatch BepInEx's HarmonyInteropFix from both methods
                // Note: The owner ID has a typo in BepInEx source — "bepinex" not "bepinEx"
                var harmonyInteropOwner = "org.bepinex.fixes.harmonyinterop";

                LogPatchInfo(loadFileMethod, "Assembly.LoadFile BEFORE unpatch");

                harmony.Unpatch(loadFileMethod, HarmonyPatchType.All, harmonyInteropOwner);
                SafeDebug("Unpatched HarmonyInteropFix from Assembly.LoadFile");

                harmony.Unpatch(loadFromMethod, HarmonyPatchType.All, harmonyInteropOwner);
                SafeDebug("Unpatched HarmonyInteropFix from Assembly.LoadFrom");

                LogPatchInfo(loadFileMethod, "Assembly.LoadFile AFTER unpatch");

                // Install our non-destructive prefix on Assembly.LoadFile only
                // (game's Add_On.cs only calls Assembly.LoadFile, not LoadFrom)
                var ourPrefix = AccessTools.Method(typeof(RetargetHarmony), nameof(OnAssemblyLoadFile));
                _ = harmony.Patch(loadFileMethod, new HarmonyMethod(ourPrefix));
                SafeInfo("Installed non-destructive Assembly.LoadFile prefix for BaseMods retargeting");

                LogPatchInfo(loadFileMethod, "Assembly.LoadFile FINAL state");

                // Verify HarmonyInteropFix was actually removed — if not, escalate to TryShim fallback
                if (HasPatchesFromOwner(loadFileMethod, harmonyInteropOwner))
                {
                    SafeWarn("Unpatch did not remove HarmonyInteropFix — escalating to TryShim fallback");
                    PatchTryShimFallback(harmony);
                }
            }
            catch (Exception ex)
            {
                // On failure, the original HarmonyInteropFix stays active (degraded but functional)
                SafeError(string.Format(CultureInfo.InvariantCulture, "Failed to neutralize HarmonyInteropFix: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Logs Harmony patch info for a method to aid diagnostics.
        /// </summary>
        private static void LogPatchInfo(MethodBase method, string label)
        {
            try
            {
                var patchInfo = Harmony.GetPatchInfo(method);
                if (patchInfo == null)
                {
                    SafeDebug(string.Format(CultureInfo.InvariantCulture, "[{0}] No patch info found", label));
                    return;
                }

                if (patchInfo.Prefixes != null)
                {
                    foreach (var prefix in patchInfo.Prefixes)
                    {
                        SafeDebug(string.Format(CultureInfo.InvariantCulture,
                            "[{0}] Prefix: owner={1}, method={2}, priority={3}",
                            label, prefix.owner, prefix.PatchMethod.Name, prefix.priority));
                    }
                }

                if (patchInfo.Prefixes == null || patchInfo.Prefixes.Count == 0)
                {
                    SafeDebug(string.Format(CultureInfo.InvariantCulture, "[{0}] No prefixes", label));
                }
            }
            catch (Exception ex)
            {
                SafeTrace(string.Format(CultureInfo.InvariantCulture, "[{0}] Failed to get patch info: {1}", label, ex.Message));
            }
        }

        /// <summary>
        /// Checks whether a method has any Harmony patches from a specific owner.
        /// </summary>
        private static bool HasPatchesFromOwner(MethodBase method, string owner)
        {
            try
            {
                var patchInfo = Harmony.GetPatchInfo(method);
                if (patchInfo == null)
                {
                    return false;
                }

                if (patchInfo.Prefixes != null)
                {
                    foreach (var prefix in patchInfo.Prefixes)
                    {
                        if (prefix.owner == owner)
                        {
                            return true;
                        }
                    }
                }

                if (patchInfo.Postfixes != null)
                {
                    foreach (var postfix in patchInfo.Postfixes)
                    {
                        if (postfix.owner == owner)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Fallback: patches HarmonyInterop.TryShim directly to prevent disk writes
        /// when Harmony.Unpatch fails to remove HarmonyInteropFix.
        /// </summary>
        private static void PatchTryShimFallback(Harmony harmony)
        {
            try
            {
                // HarmonyInterop is in HarmonyXInterop.dll
                var harmonyInteropType = Type.GetType("HarmonyInterop, HarmonyXInterop");
                if (harmonyInteropType == null)
                {
                    SafeWarn("Could not find HarmonyInterop type for TryShim fallback");
                    return;
                }

                var tryShimMethod = AccessTools.Method(harmonyInteropType, "TryShim");
                if (tryShimMethod == null)
                {
                    SafeWarn("Could not find HarmonyInterop.TryShim method for fallback");
                    return;
                }

                var blockPrefix = AccessTools.Method(typeof(RetargetHarmony), nameof(BlockTryShim));
                _ = harmony.Patch(tryShimMethod, new HarmonyMethod(blockPrefix));
                SafeInfo("Installed TryShim blocking prefix as fallback");
            }
            catch (Exception ex)
            {
                SafeError(string.Format(CultureInfo.InvariantCulture, "Failed to install TryShim fallback: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Fallback prefix that blocks HarmonyInterop.TryShim for BaseMods paths,
        /// preventing disk writes when Unpatch fails.
        /// </summary>
        /// <remarks>
        /// Uses positional parameter injection (__0) to avoid HarmonyX name mismatches.
        /// </remarks>
        // ReSharper disable InconsistentNaming
        public static bool BlockTryShim(string __0)
        // ReSharper restore InconsistentNaming
        {
            try
            {
                if (!string.IsNullOrEmpty(__0) &&
                    __0.StartsWith(BaseModsPath, StringComparison.OrdinalIgnoreCase))
                {
                    SafeTrace(string.Format(CultureInfo.InvariantCulture,
                        "Blocked TryShim for BaseMods path: {0}", Path.GetFileName(__0)));
                    return false;
                }

                return true;
            }
            catch
            {
                return true;
            }
        }

        // Prefix for TypeLoader.FindPluginTypes — disables cache saving when scanning the main plugin directory
        private static void PreFindPluginTypes(string directory)
        {
            try
            {
                SafeTrace(string.Format(CultureInfo.InvariantCulture, "PreFindPluginTypes() entry with directory: {0}", directory));

                if (!string.Equals(directory, Paths.PluginPath, StringComparison.Ordinal))
                {
                    return;
                }

                // Prevent saving cache to avoid overwriting it when loading all mods
                s_shouldSaveCache = false;
                SafeDebug("Disabling cache save for main plugin directory");
            }
            catch (Exception ex)
            {
                SafeError(string.Format(CultureInfo.InvariantCulture, "Exception in PreFindPluginTypes(): {0}", ex.Message));
            }
        }

        // Postfix for TypeLoader.FindPluginTypes — merges BaseMods plugins into results and saves merged cache
        private static void PostFindPluginTypes(
            Dictionary<string, List<PluginInfo>> __result,
            string directory,
            Func<TypeDefinition, PluginInfo> typeSelector,
            Func<AssemblyDefinition, bool> assemblyFilter,
            string cacheName)
        {
            try
            {
                SafeTrace(string.Format(CultureInfo.InvariantCulture, "PostFindPluginTypes() entry with directory: {0}", directory));

                // Prevent recursion — only handle the main plugin path
                if (!string.Equals(directory, Paths.PluginPath, StringComparison.Ordinal) || s_isProcessing)
                {
                    SafeDebug("Skipping - wrong directory or recursion guard active");
                    return;
                }

                // Check if BaseMods directory exists
                if (!Directory.Exists(BaseModsPath))
                {
                    SafeInfo(string.Format(CultureInfo.InvariantCulture, "BaseMods directory does not exist: {0}", BaseModsPath));
                    s_shouldSaveCache = true;
                    return;
                }

                SafeInfo(string.Format(CultureInfo.InvariantCulture, "Finding plugins from BaseMods: {0}", BaseModsPath));

                // Set guard flag to prevent recursion
                s_isProcessing = true;
                try
                {
                    // Find plugins in BaseMods directory
                    var baseModsResult = TypeLoader.FindPluginTypes(BaseModsPath, typeSelector, assemblyFilter, cacheName);

                    SafeDebug(string.Format(CultureInfo.InvariantCulture, "Merged count after loop: {0}", baseModsResult.Count));
                    foreach (var kv in baseModsResult)
                    {
                        SafeTrace(string.Format(CultureInfo.InvariantCulture, "Merged entry key: {0}", kv.Key));
                    }

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
            catch (Exception ex)
            {
                SafeError(string.Format(CultureInfo.InvariantCulture, "Exception in PostFindPluginTypes(): {0}", ex.Message));
            }
        }

        // Prefix for TypeLoader.SaveAssemblyCache — skips saving when flag is false
        private static bool OnSaveAssemblyCache()
        {
            try
            {
                SafeTrace(string.Format(CultureInfo.InvariantCulture, "OnSaveAssemblyCache() current flag value: {0}", s_shouldSaveCache));
            }
            catch
            {
                // Ignore logging errors
            }

            return s_shouldSaveCache;
        }

        /// <summary>
        /// CLR-level assembly resolve handler. When OnAssemblyLoadFile loads assemblies via
        /// Assembly.Load(byte[]), the loaded assembly has no file location and the CLR can't
        /// probe the original directory for dependencies. This handler searches BaseMods
        /// subdirectories for the requested assembly.
        /// </summary>
        private static Assembly OnClrAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                if (args == null || string.IsNullOrEmpty(args.Name))
                {
                    return null;
                }

                // Extract simple assembly name from full name (e.g., "MyAssembly, Version=1.0.0.0, ...")
                var assemblyName = args.Name;
                var commaIndex = assemblyName.IndexOf(',');
                if (commaIndex > 0)
                {
                    assemblyName = assemblyName.Substring(0, commaIndex);
                }

                SafeTrace(string.Format(CultureInfo.InvariantCulture, "OnClrAssemblyResolve() resolving: {0}", assemblyName));

                // Search BaseMods subdirectories for matching DLL
                if (!Directory.Exists(BaseModsPath))
                {
                    return null;
                }

                foreach (var subDir in Directory.GetDirectories(BaseModsPath))
                {
                    // Try exact name match
                    var dllPath = Path.Combine(subDir, assemblyName + ".dll");
                    if (File.Exists(dllPath))
                    {
                        SafeDebug(string.Format(CultureInfo.InvariantCulture, "OnClrAssemblyResolve() found: {0}", dllPath));

                        // Use Assembly.LoadFile so our prefix can handle retargeting if needed.
                        // For DLLs that don't need retargeting, the prefix passes through to original LoadFile.
                        return Assembly.LoadFile(dllPath);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                SafeTrace(string.Format(CultureInfo.InvariantCulture, "OnClrAssemblyResolve() failed: {0}", ex.Message));
                return null;
            }
        }

        // Assembly resolve handler — tries to resolve assemblies from BaseMods directory
        // NOTE: This is NOT AppDomain.AssemblyResolve (System.Reflection).
        // BepInEx's TypeLoader.AssemblyResolve is a custom event using Mono.Cecil types:
        //   delegate: AssemblyDefinition handler(object sender, AssemblyNameReference reference)
        // The parameter and return types below are CORRECT — do not change to System.Reflection types.
        private static AssemblyDefinition OnAssemblyResolve(object sender, AssemblyNameReference reference)
        {
            try
            {
                SafeTrace(string.Format(CultureInfo.InvariantCulture, "OnAssemblyResolve() entry with reference: {0}", reference.Name));

                try
                {
                    // Try to resolve from BaseMods directory
                    var result = TryResolveCecilAssembly(reference.Name, BaseModsPath);
                    if (result != null)
                    {
                        SafeDebug(string.Format(CultureInfo.InvariantCulture, "Successfully resolved assembly: {0}", reference.Name));
                    }

                    return result;
                }
                catch (IOException ex)
                {
                    SafeTrace(string.Format(CultureInfo.InvariantCulture, "Caught IOException: {0}", ex.Message));
                    // File access error - return null to let other handlers try
                    return null;
                }
                catch (ArgumentException ex)
                {
                    SafeTrace(string.Format(CultureInfo.InvariantCulture, "Caught ArgumentException: {0}", ex.Message));
                    // Malformed assembly name - return null to let other handlers try
                    return null;
                }
                catch (BadImageFormatException ex)
                {
                    SafeTrace(string.Format(CultureInfo.InvariantCulture, "Caught BadImageFormatException: {0}", ex.Message));
                    // Not a valid .NET assembly - return null to let other handlers try
                    return null;
                }
            }
            catch (Exception ex)
            {
                SafeError(string.Format(CultureInfo.InvariantCulture, "Exception in OnAssemblyResolve(): {0}", ex.Message));
                return null;
            }
        }

        // Helper method to resolve an assembly from a specific directory using Mono.Cecil
        private static AssemblyDefinition TryResolveCecilAssembly(string assemblyName, string directory)
        {
            try
            {
                SafeTrace(string.Format(CultureInfo.InvariantCulture, "TryResolveCecilAssembly() entry with assembly: {0}, directory: {1}", assemblyName, directory));

                if (!Directory.Exists(directory))
                {
                    SafeTrace(string.Format(CultureInfo.InvariantCulture, "Directory does not exist: {0}", directory));
                    return null;
                }

                var dllPath = Path.Combine(directory, assemblyName + ".dll");
                if (!File.Exists(dllPath))
                {
                    SafeTrace(string.Format(CultureInfo.InvariantCulture, "DLL not found: {0}", dllPath));
                    return null;
                }

                try
                {
                    AssemblyDefinition result = AssemblyDefinition.ReadAssembly(dllPath);
                    SafeDebug(string.Format(CultureInfo.InvariantCulture, "Loaded assembly successfully: {0}", assemblyName));
                    return result;
                }
                catch (BadImageFormatException ex)
                {
                    SafeTrace(string.Format(CultureInfo.InvariantCulture, "BadImageFormatException: {0}", ex.Message));
                    // Not a valid .NET assembly
                    return null;
                }
                catch (IOException ex)
                {
                    SafeTrace(string.Format(CultureInfo.InvariantCulture, "IOException: {0}", ex.Message));
                    // File access error
                    return null;
                }
            }
            catch (Exception ex)
            {
                SafeError(string.Format(CultureInfo.InvariantCulture, "Exception in TryResolveCecilAssembly(): {0}", ex.Message));
                return null;
            }
        }
    }
}
