// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using RetargetHarmony.Installer.Interfaces;

namespace RetargetHarmony.Installer.Services
{
    /// <summary>
    /// Analyzes DLLs in the BaseMods directory for BepInEx or Harmony 2+ dependencies.
    /// </summary>
    public sealed class BaseModsAnalyzer : IBaseModsAnalyzer
    {
        private const string BaseModsFolder = "BaseMods";
        private const string HarmonyAssemblyName = "0Harmony";
        private const string BepInExPrefix = "BepInEx";
        private static readonly Version Harmony2Version = new(2, 0, 0, 0);

        /// <inheritdoc />
        public IReadOnlyList<FlaggedMod> Analyze(string gamePath)
        {
            var flaggedMods = new List<FlaggedMod>();
            var baseModsPath = Path.Combine(gamePath, BaseModsFolder);

            if (!Directory.Exists(baseModsPath))
            {
                return flaggedMods;
            }

            foreach (var dllPath in Directory.GetFiles(baseModsPath, "*.dll", SearchOption.AllDirectories))
            {
                AnalyzeDll(dllPath, flaggedMods);
            }

            return flaggedMods;
        }

        private static void AnalyzeDll(string dllPath, List<FlaggedMod> flaggedMods)
        {
            try
            {
                using var assembly = AssemblyDefinition.ReadAssembly(dllPath, new ReaderParameters { ReadWrite = false });
                var fileName = Path.GetFileName(dllPath);

                foreach (var reference in assembly.MainModule.AssemblyReferences)
                {
                    if (reference.Name.Equals(HarmonyAssemblyName, StringComparison.Ordinal)
                        && reference.Version >= Harmony2Version)
                    {
                        flaggedMods.Add(new FlaggedMod(
                            dllPath,
                            fileName,
                            FlagReason.Harmony2Reference,
                            $"{reference.Name} v{reference.Version}"));
                    }
                    else if (reference.Name.StartsWith(BepInExPrefix, StringComparison.Ordinal))
                    {
                        flaggedMods.Add(new FlaggedMod(
                            dllPath,
                            fileName,
                            FlagReason.BepInExReference,
                            reference.Name));
                    }
                }
            }
            catch (Exception ex) when (ex is BadImageFormatException or IOException or InvalidOperationException)
            {
                // Skip files that are not valid .NET assemblies or cannot be read
            }
        }
    }
}
