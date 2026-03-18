// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using Hemocode.Common.Enums.Diagnostics;
using Hemocode.Common.Implementations;
using Hemocode.Common.Models.Diagnostics;
using Hemocode.DebugPanel.Interfaces;
using Hemocode.DebugPanel.JsonModels;

#endregion

namespace Hemocode.DebugPanel.Implementations
{
    public sealed class KnownIssuesChecker : IInfoCollector<KnownIssuesReport>
    {
        private readonly IKnownIssuesDatabase _database;
        private readonly IList<DetectedModInfo> _detectedMods;
        private readonly IList<AssemblyInfo> _loadedAssemblies;
        private readonly IFileSystemScanner _scanner;

        public KnownIssuesChecker(
            IKnownIssuesDatabase database,
            IList<DetectedModInfo> detectedMods,
            IList<AssemblyInfo> loadedAssemblies,
            IFileSystemScanner scanner)
        {
            ThrowHelper.ThrowIfNull(database);
            _database = database;
            ThrowHelper.ThrowIfNull(detectedMods);
            _detectedMods = detectedMods;
            ThrowHelper.ThrowIfNull(loadedAssemblies);
            _loadedAssemblies = loadedAssemblies;
            ThrowHelper.ThrowIfNull(scanner);
            _scanner = scanner;
        }

        public KnownIssuesReport Collect()
        {
            var matches = new List<KnownIssueMatch>();
            var knownIssues = _database.GetKnownIssues();
            var baseModsPath = _scanner.GetBaseModsPath();

            foreach (var issue in knownIssues)
            {
                if (issue == null)
                {
                    continue;
                }

                if (MatchByDllName(issue, baseModsPath, matches))
                {
                    continue;
                }

                MatchByAssemblyName(issue, matches);
            }

            CheckConflicts(knownIssues, matches);

            return new KnownIssuesReport(matches, _database.DatabaseVersion);
        }

        private bool MatchByDllName(KnownIssueItem issue, string baseModsPath, IList<KnownIssueMatch> matches)
        {
            if (string.IsNullOrEmpty(issue.DllName))
            {
                return false;
            }

            var dllPath = baseModsPath + "/" + issue.DllName;
            if (!_scanner.FileExists(dllPath))
            {
                return false;
            }

            matches.Add(CreateMatch(issue, "DLL file: " + issue.DllName));

            return true;
        }

        private void MatchByAssemblyName(KnownIssueItem issue, IList<KnownIssueMatch> matches)
        {
            if (string.IsNullOrEmpty(issue.AssemblyName))
            {
                return;
            }

            foreach (var assembly in _loadedAssemblies)
            {
                if (assembly != null &&
                    string.Equals(assembly.Name, issue.AssemblyName, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(CreateMatch(issue, "Loaded assembly: " + issue.AssemblyName));

                    return;
                }
            }
        }

        private void CheckConflicts(IList<KnownIssueItem> knownIssues, IList<KnownIssueMatch> matches)
        {
            foreach (var issue in knownIssues)
            {
                if (issue == null || issue.ConflictsWith == null || issue.ConflictsWith.Length == 0)
                {
                    continue;
                }

                if (!IsModPresent(issue))
                {
                    continue;
                }

                foreach (var conflictName in issue.ConflictsWith)
                {
                    if (string.IsNullOrEmpty(conflictName))
                    {
                        continue;
                    }

                    if (IsModPresentByName(conflictName))
                    {
                        var description = "Conflict: " + issue.ModName + " conflicts with " + conflictName;
                        var fixSuggestion = "Remove one of the conflicting mods: " + issue.ModName + " or " + conflictName;
                        matches.Add(new KnownIssueMatch(
                            issue.ModName ?? string.Empty,
                            FindingSeverity.Warning,
                            description,
                            fixSuggestion,
                            issue.WikiLink ?? string.Empty,
                            "Conflict pair"));
                    }
                }
            }
        }

        private bool IsModPresent(KnownIssueItem issue)
        {
            if (!string.IsNullOrEmpty(issue.DllName))
            {
                var dllPath = _scanner.GetBaseModsPath() + "/" + issue.DllName;
                if (_scanner.FileExists(dllPath))
                {
                    return true;
                }
            }

            if (!string.IsNullOrEmpty(issue.AssemblyName))
            {
                foreach (var assembly in _loadedAssemblies)
                {
                    if (assembly != null &&
                        string.Equals(assembly.Name, issue.AssemblyName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return IsModPresentByName(issue.ModName);
        }

        private bool IsModPresentByName(string modName)
        {
            if (string.IsNullOrEmpty(modName))
            {
                return false;
            }

            foreach (var mod in _detectedMods)
            {
                if (mod != null &&
                    string.Equals(mod.Name, modName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static KnownIssueMatch CreateMatch(KnownIssueItem issue, string matchedBy)
        {
            var severity = ConvertSeverity(issue.Severity);

            return new KnownIssueMatch(
                issue.ModName ?? string.Empty,
                severity,
                issue.Description ?? string.Empty,
                issue.FixSuggestion ?? string.Empty,
                issue.WikiLink ?? string.Empty,
                matchedBy);
        }

        private static FindingSeverity ConvertSeverity(int severity)
        {
            if (severity >= 2)
            {
                return FindingSeverity.Error;
            }

            if (severity == 1)
            {
                return FindingSeverity.Warning;
            }

            return FindingSeverity.Info;
        }
    }
}
