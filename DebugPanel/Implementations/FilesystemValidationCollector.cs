// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.IO;
using DebugPanel.Common.Enums.Diagnostics;
using DebugPanel.Common.Implementations;
using DebugPanel.Common.Models.Diagnostics;
using DebugPanel.Interfaces;

#endregion

namespace DebugPanel.Implementations
{
    public sealed class FilesystemValidationCollector : IInfoCollector<FilesystemValidationReport>
    {
        private const string Category = "Filesystem";
        private const string SourceTab = "Files";

        private readonly IFileSystemScanner _scanner;

        public FilesystemValidationCollector(IFileSystemScanner scanner)
        {
            ThrowHelper.ThrowIfNull(scanner);
            _scanner = scanner;
        }

        public FilesystemValidationReport Collect()
        {
            var issues = new List<DiagnosticIssue>();
            CheckAssemblyCSharpInBaseMods(issues);
            CheckLmmExecutablesInBaseMods(issues);
            CheckDoubleFolderNesting(issues);
            CheckSaveDataPathLength(issues);
            CheckBaseModListExists(issues);

            var summary = issues.Count + " filesystem issue(s) found";

            return new FilesystemValidationReport(issues, summary);
        }

        private void CheckAssemblyCSharpInBaseMods(IList<DiagnosticIssue> issues)
        {
            var baseModsPath = _scanner.GetBaseModsPath();
            var dllPath = baseModsPath + "/Assembly-CSharp.dll";
            if (_scanner.FileExists(dllPath))
            {
                issues.Add(new DiagnosticIssue(
                    FindingSeverity.Error,
                    Category,
                    "Assembly-CSharp.dll found in BaseMods folder. This is an assembly replacement, not a mod, and will cause conflicts.",
                    SourceTab,
                    "Remove Assembly-CSharp.dll from BaseMods. Use 'Reticle Memory Leak Fix' mod instead if this was from Memory Leak Fix."));
            }
        }

        private void CheckLmmExecutablesInBaseMods(IList<DiagnosticIssue> issues)
        {
            var baseModsPath = _scanner.GetBaseModsPath();
            if (!_scanner.DirectoryExists(baseModsPath))
            {
                return;
            }

            var filesToCheck = new[]
            {
                "LobotomyModManager.exe",
                "LobotomyBaseModLib.dll",
            };

            foreach (var fileName in filesToCheck)
            {
                var filePath = baseModsPath + "/" + fileName;
                if (_scanner.FileExists(filePath))
                {
                    issues.Add(new DiagnosticIssue(
                        FindingSeverity.Error,
                        Category,
                        fileName + " found in BaseMods folder. This file should not be in BaseMods and may cause Infohazard errors.",
                        SourceTab,
                        "Remove " + fileName + " from the BaseMods folder."));
                }
            }
        }

        private void CheckDoubleFolderNesting(IList<DiagnosticIssue> issues)
        {
            var baseModsPath = _scanner.GetBaseModsPath();
            if (!_scanner.DirectoryExists(baseModsPath))
            {
                return;
            }

            var modDirectories = _scanner.GetDirectories(baseModsPath);
            foreach (var modDir in modDirectories)
            {
                var subdirectories = _scanner.GetDirectories(modDir);
                if (subdirectories.Count != 1)
                {
                    continue;
                }

                var innerDir = subdirectories[0];
                var innerFiles = _scanner.GetFiles(innerDir, "*.dll");
                var outerFiles = _scanner.GetFiles(modDir, "*.dll");
                if (innerFiles.Count > 0 && outerFiles.Count == 0)
                {
                    var modDirName = Path.GetFileName(modDir);
                    issues.Add(new DiagnosticIssue(
                        FindingSeverity.Warning,
                        Category,
                        "Double-folder nesting detected in '" + modDirName + "'. Mod DLLs are nested one level too deep.",
                        SourceTab,
                        "Move the contents of the inner folder up one level, or re-extract the mod archive."));
                }
            }
        }

        private void CheckSaveDataPathLength(IList<DiagnosticIssue> issues)
        {
            try
            {
                var saveDataPath = _scanner.GetSaveDataPath();
                if (saveDataPath.Length > 260)
                {
                    issues.Add(new DiagnosticIssue(
                        FindingSeverity.Warning,
                        Category,
                        "Save data path exceeds 260 characters (" + saveDataPath.Length + " chars). This may cause issues on Windows due to MAX_PATH limitations.",
                        SourceTab,
                        "Move the game installation to a shorter path."));
                }
            }
            catch (Exception)
            {
                // Save data path may not be accessible; skip this check
            }
        }

        private void CheckBaseModListExists(IList<DiagnosticIssue> issues)
        {
            var baseModsPath = _scanner.GetBaseModsPath();
            var baseModListPath = baseModsPath + "/BaseModList_v2.xml";

            if (!_scanner.FileExists(baseModListPath))
            {
                issues.Add(new DiagnosticIssue(
                    FindingSeverity.Warning,
                    Category,
                    "BaseModList_v2.xml not found in BaseMods folder. Mods may not load correctly.",
                    SourceTab,
                    "Run the mod manager to regenerate the mod list, or verify BaseMods folder structure."));

                return;
            }

            try
            {
                var fileSize = _scanner.GetFileSize(baseModListPath);
                if (fileSize == 0)
                {
                    issues.Add(new DiagnosticIssue(
                        FindingSeverity.Warning,
                        Category,
                        "BaseModList_v2.xml is empty. Mods will not load.",
                        SourceTab,
                        "Run the mod manager to regenerate the mod list."));
                }
            }
            catch (Exception)
            {
                // File size check failed; skip
            }
        }
    }
}
