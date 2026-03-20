// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using DebugPanel.Common.Enums.Diagnostics;
using DebugPanel.Common.Implementations;
using DebugPanel.Common.Models.Diagnostics;
using DebugPanel.Interfaces;

#endregion

namespace DebugPanel.Implementations
{
    public sealed class IssuesAggregator : IIssuesAggregator
    {
        public IList<DiagnosticIssue> AggregateIssues(DiagnosticReport report)
        {
            ThrowHelper.ThrowIfNull(report);

            var issues = new List<DiagnosticIssue>();

            AddFilesystemIssues(issues, report.FilesystemValidation);
            AddDependencyIssues(issues, report.DependencyReport);
            AddKnownIssueMatches(issues, report.KnownIssuesReport);
            AddGameplayLogErrors(issues, report.GameplayLogErrorReport);
            AddErrorLogEntries(issues, report.ErrorLogReport);
            AddMissingPatches(issues, report.PatchComparison);
            AddDllIntegrityIssues(issues, report.DllIntegrity);
            AddWarnings(issues, report.Warnings);

            SortBySeverityDescending(issues);

            return issues;
        }

        private static void AddFilesystemIssues(List<DiagnosticIssue> issues, FilesystemValidationReport filesystemValidation)
        {
            issues.AddRange(filesystemValidation.Issues);
        }

        private static void AddDependencyIssues(List<DiagnosticIssue> issues, DependencyReport dependencyReport)
        {
            issues.AddRange(dependencyReport.Issues);
        }

        private static void AddKnownIssueMatches(List<DiagnosticIssue> issues, KnownIssuesReport knownIssuesReport)
        {
            foreach (var match in knownIssuesReport.Matches)
            {
                var description = match.ModName + ": " + match.Description;
                issues.Add(new DiagnosticIssue(
                    match.Severity,
                    "Known Issue",
                    description,
                    "Mods",
                    match.FixSuggestion));
            }
        }

        private static void AddGameplayLogErrors(List<DiagnosticIssue> issues, GameplayLogErrorReport gameplayLogErrorReport)
        {
            foreach (var entry in gameplayLogErrorReport.Entries)
            {
                var description = string.IsNullOrEmpty(entry.ModName)
                    ? "Mod load error: " + entry.RawLine
                    : "Mod load error: " + entry.ModName;

                if (!string.IsNullOrEmpty(entry.ErrorMessage))
                {
                    description += " — " + entry.ErrorMessage;
                }

                issues.Add(new DiagnosticIssue(
                    FindingSeverity.Error,
                    "Mod Load Error",
                    description,
                    "Mods",
                    "Check the gameplay log for full stack trace. The mod may be incompatible or incorrectly installed."));
            }
        }

        private static void AddErrorLogEntries(List<DiagnosticIssue> issues, ErrorLogReport errorLogReport)
        {
            foreach (var entry in errorLogReport.Entries)
            {
                var firstLine = GetFirstLine(entry.Content);
                var description = string.IsNullOrEmpty(firstLine)
                    ? "Error log found: " + entry.FileName
                    : "Error log " + entry.FileName + ": " + firstLine;

                issues.Add(new DiagnosticIssue(
                    FindingSeverity.Error,
                    "Error Log",
                    description,
                    "Files",
                    GetErrorLogFixSuggestion(entry.FileName, entry.FilePath)));
            }
        }

        private static string GetFirstLine(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }

            var newlineIndex = content.IndexOf('\n');
            if (newlineIndex < 0)
            {
                return content;
            }

            var line = content.Substring(0, newlineIndex);
            if (line.EndsWith("\r", System.StringComparison.Ordinal))
            {
                line = line.Substring(0, line.Length - 1);
            }

            return line;
        }

        private static string GetErrorLogFixSuggestion(string fileName, string filePath)
        {
            if (fileName == "LMMerror.txt")
            {
                return "Map/graph loading error. Check " + filePath + " for details.";
            }

            if (fileName == "LTDerror.txt")
            {
                return "Localization data loading error. Check " + filePath + " for details.";
            }

            if (fileName == "Glerror.txt")
            {
                return "Global game manager error. Check " + filePath + " for details.";
            }

            if (fileName == "DPerror.txt")
            {
                return "Deploy UI error. Check " + filePath + " for details.";
            }

            return "Check " + filePath + " for details.";
        }

        private static void AddMissingPatches(List<DiagnosticIssue> issues, PatchComparisonResult patchComparison)
        {
            foreach (var missing in patchComparison.MissingPatches)
            {
                issues.Add(new DiagnosticIssue(
                    FindingSeverity.Warning,
                    "Missing Patch",
                    missing.PatchAssembly + ": " + missing.PatchMethod + " for " + missing.TargetType + "." + missing.TargetMethod + " not loaded",
                    "Harmony",
                    "Verify the mod is installed correctly and compatible with the current game version."));
            }
        }

        private static void AddDllIntegrityIssues(List<DiagnosticIssue> issues, DllIntegrityReport dllIntegrity)
        {
            foreach (var finding in dllIntegrity.Findings)
            {
                if (finding.Severity == FindingSeverity.Info)
                {
                    continue;
                }

                issues.Add(new DiagnosticIssue(
                    finding.Severity,
                    "DLL Integrity",
                    finding.DllName + ": " + finding.Summary,
                    "Files",
                    string.Empty));
            }
        }

        private static void AddWarnings(List<DiagnosticIssue> issues, IList<string> warnings)
        {
            foreach (var warning in warnings)
            {
                issues.Add(new DiagnosticIssue(
                    FindingSeverity.Info,
                    "Internal",
                    warning,
                    "Environment",
                    string.Empty));
            }
        }

        private static void SortBySeverityDescending(List<DiagnosticIssue> issues)
        {
            for (var i = 0; i < issues.Count - 1; i++)
            {
                for (var j = i + 1; j < issues.Count; j++)
                {
                    if (issues[i].Severity < issues[j].Severity)
                    {
#pragma warning disable IDE0180
                        var temp = issues[i];
                        issues[i] = issues[j];
                        issues[j] = temp;
#pragma warning restore IDE0180
                    }
                }
            }
        }
    }
}
