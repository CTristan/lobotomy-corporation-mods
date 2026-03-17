// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Enums.Diagnostics;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Models.Diagnostics;
using LobotomyCorporationMods.DebugPanel.Interfaces;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Implementations
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

        private static void AddErrorLogEntries(List<DiagnosticIssue> issues, ErrorLogReport errorLogReport)
        {
            foreach (var entry in errorLogReport.Entries)
            {
                issues.Add(new DiagnosticIssue(
                    FindingSeverity.Warning,
                    "Error Log",
                    "Error log found: " + entry.FileName,
                    "Files",
                    "Check " + entry.FilePath + " for details."));
            }
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
