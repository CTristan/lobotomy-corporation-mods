// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using DebugPanel.Common.Enums.Diagnostics;
using DebugPanel.Common.Implementations;
using DebugPanel.Common.Models.Diagnostics;
using DebugPanel.Interfaces;

#endregion

namespace DebugPanel.Implementations
{
    public sealed class DependencyChecker : IInfoCollector<DependencyReport>
    {
        private const string Category = "Dependency";
        private const string SourceTab = "Mods";

        private readonly IFileSystemScanner _scanner;
        private readonly IList<DetectedModInfo> _detectedMods;
        private readonly IList<AssemblyInfo> _loadedAssemblies;

        public DependencyChecker(
            IFileSystemScanner scanner,
            IList<DetectedModInfo> detectedMods,
            IList<AssemblyInfo> loadedAssemblies)
        {
            ThrowHelper.ThrowIfNull(scanner);
            _scanner = scanner;
            ThrowHelper.ThrowIfNull(detectedMods);
            _detectedMods = detectedMods;
            ThrowHelper.ThrowIfNull(loadedAssemblies);
            _loadedAssemblies = loadedAssemblies;
        }

        public DependencyReport Collect()
        {
            var issues = new List<DiagnosticIssue>();
            var baseModsPath = _scanner.GetBaseModsPath();

            Check12HarmonyMissing(issues, baseModsPath);
            CheckBaseModListHealth(issues, baseModsPath);

            var baseModVersion = DetectBaseModVersion();
            var baseModListPath = baseModsPath + "/BaseModList_v2.xml";
            var baseModListExists = _scanner.FileExists(baseModListPath);

            return new DependencyReport(issues, baseModVersion, baseModListExists);
        }

        private void Check12HarmonyMissing(IList<DiagnosticIssue> issues, string baseModsPath)
        {
            var harmonyPath = baseModsPath + "/12Harmony.dll";
            if (_scanner.FileExists(harmonyPath))
            {
                return;
            }

            var isReferenced = false;
            foreach (var mod in _detectedMods)
            {
                if (mod != null &&
                    !string.IsNullOrEmpty(mod.AssemblyName) &&
                    mod.AssemblyName.Equals("12Harmony", StringComparison.OrdinalIgnoreCase))
                {
                    isReferenced = true;

                    break;
                }
            }

            if (!isReferenced)
            {
                foreach (var assembly in _loadedAssemblies)
                {
                    if (assembly != null &&
                        !string.IsNullOrEmpty(assembly.Name) &&
                        assembly.Name.Equals("12Harmony", StringComparison.OrdinalIgnoreCase))
                    {
                        isReferenced = true;

                        break;
                    }
                }
            }

            if (isReferenced)
            {
                issues.Add(new DiagnosticIssue(
                    FindingSeverity.Error,
                    Category,
                    "12Harmony.dll is referenced but not found in BaseMods. Mods depending on it will fail to load.",
                    SourceTab,
                    "Download and install 12Harmony.dll into the BaseMods folder."));
            }
        }

        private void CheckBaseModListHealth(IList<DiagnosticIssue> issues, string baseModsPath)
        {
            var baseModListPath = baseModsPath + "/BaseModList_v2.xml";
            if (!_scanner.FileExists(baseModListPath))
            {
                issues.Add(new DiagnosticIssue(
                    FindingSeverity.Warning,
                    Category,
                    "BaseModList_v2.xml is missing. The mod loader may not detect installed mods.",
                    SourceTab,
                    "Run the mod manager to regenerate the mod list file."));
            }
        }

        private string DetectBaseModVersion()
        {
            foreach (var assembly in _loadedAssemblies)
            {
                if (assembly != null &&
                    !string.IsNullOrEmpty(assembly.Name) &&
                    assembly.Name.Equals("LobotomyBaseModLib", StringComparison.OrdinalIgnoreCase))
                {
                    return assembly.Version ?? string.Empty;
                }
            }

            return string.Empty;
        }
    }
}
