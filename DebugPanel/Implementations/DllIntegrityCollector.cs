// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.IO;
using DebugPanel.Common.Implementations;
using DebugPanel.Interfaces;
using DebugPanel.Common.Enums.Diagnostics;
using DebugPanel.Common.Models.Diagnostics;

#endregion

namespace DebugPanel.Implementations
{
    public sealed class DllIntegrityCollector : IInfoCollector<DllIntegrityReport>
    {
        private readonly IDllFileInspector _dllFileInspector;
        private readonly IShimArtifactSource _shimArtifactSource;
        private readonly ILoadedAssemblyReferenceSource _loadedAssemblySource;
        private readonly PeAssemblyRefReader _peReader;

        public DllIntegrityCollector(
            IDllFileInspector dllFileInspector,
            IShimArtifactSource shimArtifactSource,
            ILoadedAssemblyReferenceSource loadedAssemblySource)
        {
            ThrowHelper.ThrowIfNull(dllFileInspector);
            _dllFileInspector = dllFileInspector;
            ThrowHelper.ThrowIfNull(shimArtifactSource);
            _shimArtifactSource = shimArtifactSource;
            ThrowHelper.ThrowIfNull(loadedAssemblySource);
            _loadedAssemblySource = loadedAssemblySource;
            _peReader = new PeAssemblyRefReader();
        }

        public DllIntegrityReport Collect()
        {
            var shimBackupExists = _shimArtifactSource.BackupDirectoryExists;
            var shimBackupPath = _shimArtifactSource.BackupDirectoryPath;
            var interopCacheExists = _shimArtifactSource.InteropCacheExists;
            var interopCachePath = _shimArtifactSource.InteropCachePath;
            var interopCacheEntryCount = interopCacheExists ? _shimArtifactSource.GetInteropCacheEntryCount() : -1;

            var backupFileNames = shimBackupExists ? _shimArtifactSource.GetBackupFileNames() : new List<string>();

            var assemblies = _loadedAssemblySource.GetBaseModAssemblies();
            var findings = new List<DllIntegrityFinding>();
            var warnings = new List<string>();

            foreach (var assembly in assemblies)
            {
                if (assembly == null)
                {
                    continue;
                }

                var finding = InspectAssembly(assembly, backupFileNames, warnings);
                findings.Add(finding);
            }

            var totalRewritten = 0;
            foreach (var finding in findings)
            {
                if (finding.WasRewritten)
                {
                    totalRewritten++;
                }
            }

            var summary = findings.Count + " DLLs checked, " + totalRewritten + " rewritten";

            return new DllIntegrityReport(
                findings,
                shimBackupExists,
                shimBackupPath,
                interopCacheExists,
                interopCachePath,
                interopCacheEntryCount,
                _dllFileInspector.IsDeepInspectionAvailable,
                totalRewritten,
                warnings,
                summary);
        }

        private DllIntegrityFinding InspectAssembly(
            LoadedAssemblyInfo assembly,
            IList<string> backupFileNames,
            IList<string> warnings)
        {
            var dllName = Path.GetFileName(assembly.Location);
            var dllPath = assembly.Location;

            IList<string> onDiskReferences;
            try
            {
                onDiskReferences = _dllFileInspector.GetAssemblyReferences(dllPath);
            }
            catch (Exception ex)
            {
                warnings.Add("Unable to read " + dllName + ": " + ex.Message);

                return new DllIntegrityFinding(
                    dllPath,
                    dllName,
                    FindingSeverity.Warning,
                    new List<string>(),
                    new List<string>(),
                    false,
                    string.Empty,
                    false,
                    "Unable to read DLL: " + ex.Message);
            }

            var harmonyReferences = FilterHarmonyReferences(onDiskReferences);

            var hasBackup = ContainsFileName(backupFileNames, dllName);
            var backupPath = string.Empty;
            var originalReferences = new List<string>();

            if (hasBackup)
            {
                backupPath = _shimArtifactSource.BackupDirectoryPath + "/" + dllName;

                try
                {
                    var backupBytes = _shimArtifactSource.ReadBackupFileBytes(dllName);
                    originalReferences = new List<string>(FilterHarmonyReferences(_peReader.ReadAssemblyReferences(backupBytes)));
                }
                catch (Exception ex)
                {
                    warnings.Add("Unable to read backup for " + dllName + ": " + ex.Message);
                }
            }

            return ClassifyFinding(dllPath, dllName, harmonyReferences, originalReferences, hasBackup, backupPath);
        }

        private static DllIntegrityFinding ClassifyFinding(
            string dllPath,
            string dllName,
            IList<string> onDiskReferences,
            IList<string> originalReferences,
            bool hasBackup,
            string backupPath)
        {
            var containsShimmedReference = ContainsReference(onDiskReferences, "0Harmony109");
            var unexpectedReference = FindUnexpectedReference(onDiskReferences);

            if (containsShimmedReference)
            {
                var wasRewritten = true;
                if (hasBackup)
                {
                    return new DllIntegrityFinding(
                        dllPath, dllName, FindingSeverity.Warning,
                        onDiskReferences, originalReferences,
                        true, backupPath, wasRewritten,
                        "Rewritten by BepInEx shim (backup available)");
                }

                return new DllIntegrityFinding(
                    dllPath, dllName, FindingSeverity.Error,
                    onDiskReferences, originalReferences,
                    false, string.Empty, wasRewritten,
                    "Rewritten by BepInEx shim (no backup!)");
            }

            if (unexpectedReference != null)
            {
                return new DllIntegrityFinding(
                    dllPath, dllName, FindingSeverity.Error,
                    onDiskReferences, originalReferences,
                    hasBackup, backupPath, false,
                    "Unexpected Harmony reference: " + unexpectedReference);
            }

            return new DllIntegrityFinding(
                dllPath, dllName, FindingSeverity.Info,
                onDiskReferences, originalReferences,
                hasBackup, backupPath, false,
                "Not modified");
        }

        private static IList<string> FilterHarmonyReferences(IList<string> references)
        {
            var result = new List<string>();

            foreach (var reference in references)
            {
                if (reference != null && reference.IndexOf("Harmony", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result.Add(reference);
                }
            }

            return result;
        }

        private static bool ContainsReference(IList<string> references, string name)
        {
            foreach (var reference in references)
            {
                if (string.Equals(reference, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string FindUnexpectedReference(IList<string> harmonyReferences)
        {
            foreach (var reference in harmonyReferences)
            {
                if (!string.Equals(reference, "0Harmony", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(reference, "0Harmony109", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(reference, "0Harmony12", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(reference, "12Harmony", StringComparison.OrdinalIgnoreCase))
                {
                    return reference;
                }
            }

            return null;
        }

        private static bool ContainsFileName(IList<string> fileNames, string target)
        {
            foreach (var fileName in fileNames)
            {
                if (string.Equals(fileName, target, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
