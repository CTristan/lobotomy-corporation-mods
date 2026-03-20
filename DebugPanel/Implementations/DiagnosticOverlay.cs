// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DebugPanel.Common.Attributes;
using DebugPanel.Common.Constants;
using DebugPanel.Common.Implementations;
using DebugPanel.Interfaces;
using DebugPanel.JsonModels;
using DebugPanel.Common.Enums.Diagnostics;
using DebugPanel.Common.Models.Diagnostics;
using UnityEngine;

#endregion

namespace DebugPanel.Implementations
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class DiagnosticOverlay : IOverlayRenderer
    {
        private const int WindowId = 0xD1349;

        private static readonly string[] s_tabLabels = { "Issues", "Harmony", "Files", "Mods", "Environment" };

        private Rect _windowRect = new Rect(20f, 20f, 650f, 500f);
        private Vector2 _scrollPosition = Vector2.zero;
        private int _selectedTab;

        private DiagnosticReport _currentReport;
        private DebugPanelConfig _currentConfig;
        private Action _currentRefreshAction;
        private Action _currentGenerateLogAction;

        public void Draw(DiagnosticReport report, DebugPanelConfig config, Action refreshAction, Action generateLogAction)
        {
            ThrowHelper.ThrowIfNull(report);
            _currentReport = report;
            ThrowHelper.ThrowIfNull(config);
            _currentConfig = config;
            ThrowHelper.ThrowIfNull(refreshAction);
            _currentRefreshAction = refreshAction;
            _currentGenerateLogAction = generateLogAction;

            var title = "Debug Panel (" + config.OverlayToggleKey + " to hide)";
            _windowRect = GUI.Window(WindowId, _windowRect, DrawWindowContents, title);
        }

        private void DrawWindowContents(int windowId)
        {
            GUILayout.BeginVertical();

            DrawHeaderBar();

            _selectedTab = GUILayout.Toolbar(_selectedTab, s_tabLabels);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            switch (_selectedTab)
            {
                case 0:
                    DrawIssuesTab();
                    break;
                case 1:
                    DrawHarmonyTab();
                    break;
                case 2:
                    DrawFilesTab();
                    break;
                case 3:
                    DrawModsTab();
                    break;
                default:
                    DrawEnvironmentTab();
                    break;
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0f, 0f, 10000f, 20f));
        }

        private void DrawHeaderBar()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Collected: " + _currentReport.CollectedAt.ToString("u"));
            if (GUILayout.Button("Refresh", GUILayout.Width(100f)))
            {
                _currentRefreshAction();
            }

            if (_currentGenerateLogAction != null && GUILayout.Button("Generate Log", GUILayout.Width(120f)))
            {
                _currentGenerateLogAction();
            }

            GUILayout.EndHorizontal();
        }

        private void DrawIssuesTab()
        {
            var issues = _currentReport.AggregatedIssues;
            if (issues.Count == 0)
            {
                GUILayout.Label("No issues detected.");

                return;
            }

            GUILayout.Label("Issues (" + issues.Count + ")", GUI.skin.box);
            foreach (var issue in issues)
            {
                var originalColor = GUI.contentColor;
                GUI.contentColor = GetSeverityColor(issue.Severity);
                GUILayout.Label("[" + GetSeverityLabel(issue.Severity) + "] " + issue.Description);
                if (!string.IsNullOrEmpty(issue.FixSuggestion))
                {
                    GUILayout.Label("  Fix: " + issue.FixSuggestion);
                }

                GUI.contentColor = originalColor;
            }
        }

        private void DrawHarmonyTab()
        {
            if (_currentConfig.ShowActivePatches)
            {
                DrawPatchSection(_currentReport);
            }

            if (_currentReport.PatchComparison.HasMissingPatches)
            {
                DrawMissingPatchesSection(_currentReport);
            }

            if (_currentConfig.ShowExpectedPatches && _currentReport.PatchComparison.TotalExpected > 0)
            {
                GUILayout.Space(8f);
                GUILayout.Label("Expected Patches: " + _currentReport.PatchComparison.TotalExpected + " total, " + _currentReport.PatchComparison.TotalMatched + " matched");
            }
        }

        private void DrawFilesTab()
        {
            if (_currentConfig.ShowFilesystemValidation)
            {
                DrawFilesystemValidationSection(_currentReport);
            }

            if (_currentConfig.ShowDllIntegrity)
            {
                DrawDllIntegritySection(_currentReport);
            }

            if (_currentConfig.ShowErrorLogs)
            {
                DrawErrorLogsSection(_currentReport);
            }
        }

        private void DrawModsTab()
        {
            if (_currentConfig.ShowBepInExPlugins && _currentReport.EnvironmentInfo.IsBepInExAvailable)
            {
                DrawModSection(_currentReport, ModSource.BepInExPlugin, "BepInEx Plugins");
            }

            if (_currentConfig.ShowLmmMods)
            {
                DrawModSection(_currentReport, ModSource.Lmm, "LMM/Basemod Mods");
            }

            if (_currentConfig.ShowKnownIssues)
            {
                DrawKnownIssuesSection(_currentReport);
            }

            if (_currentConfig.ShowDependencies)
            {
                DrawDependenciesSection(_currentReport);
            }
        }

        private void DrawEnvironmentTab()
        {
            if (_currentConfig.ShowAssemblyInfo)
            {
                DrawAssemblySection(_currentReport);
            }

            if (_currentReport.EnvironmentInfo.IsBepInExAvailable)
            {
                GUILayout.Space(8f);
                GUILayout.Label("RetargetHarmony: " + _currentReport.RetargetHarmonyStatus.Message);
            }

            GUILayout.Space(8f);
            GUILayout.Label("Environment", GUI.skin.box);
            GUILayout.Label("  Harmony 2: " + (_currentReport.EnvironmentInfo.IsHarmony2Available ? "Available" : "Not detected"));
            GUILayout.Label("  BepInEx: " + (_currentReport.EnvironmentInfo.IsBepInExAvailable ? "Available" : "Not detected"));
            GUILayout.Label("  Mono.Cecil: " + (_currentReport.EnvironmentInfo.IsMonoCecilAvailable ? "Available" : "Not detected"));

            if (_currentReport.Warnings.Count > 0)
            {
                GUILayout.Space(8f);
                GUILayout.Label("Warnings", GUI.skin.box);
                foreach (var warning in _currentReport.Warnings)
                {
                    GUILayout.Label("- " + warning);
                }
            }
        }

        private static void DrawModSection(DiagnosticReport report, ModSource source, string title)
        {
            GUILayout.Space(8f);
            GUILayout.Label(title, GUI.skin.box);
            var hadEntries = false;

            foreach (var mod in report.Mods)
            {
                if (mod.Source != source)
                {
                    continue;
                }

                hadEntries = true;
                var originalColor = GUI.contentColor;
                GUI.contentColor = GetModColor(mod.HarmonyVersion);
                var identifierSuffix = string.IsNullOrEmpty(mod.Identifier)
                    ? string.Empty
                    : " [" + mod.Identifier + "]";

                string patchStatus;
                if (mod.ExpectedPatchCount >= 0)
                {
                    var failedCount = mod.ActivePatchCount < mod.ExpectedPatchCount
                        ? mod.ExpectedPatchCount - mod.ActivePatchCount
                        : 0;

                    if (mod.ActivePatchCount < mod.ExpectedPatchCount)
                    {
                        GUI.contentColor = new Color(1f, 0.6f, 0.4f, 1f);
                    }

                    patchStatus = " (" + mod.ActivePatchCount + " loaded/" + mod.ExpectedPatchCount + " expected, " + failedCount + " failed)";
                }
                else
                {
                    patchStatus = mod.HasActivePatches
                        ? " (" + mod.ActivePatchCount + " loaded)"
                        : " (0 loaded)";
                }

                GUILayout.Label("- " + mod.Name + " v" + mod.Version + " (" + ReportFormatter.ToHarmonyVersionLabel(mod.HarmonyVersion) + ")" + patchStatus + identifierSuffix);
                GUI.contentColor = originalColor;
            }

            if (!hadEntries)
            {
                GUILayout.Label("- None");
            }
        }

        private static Color GetModColor(HarmonyVersion version)
        {
            if (version == HarmonyVersion.Harmony1)
            {
                return new Color(0.56f, 0.86f, 1f, 1f);
            }

            if (version == HarmonyVersion.Harmony2)
            {
                return new Color(0.66f, 1f, 0.66f, 1f);
            }

            return Color.white;
        }

        private static void DrawPatchSection(DiagnosticReport report)
        {
            GUILayout.Space(8f);
            GUILayout.Label("Active Harmony Patches (" + report.Patches.Count + ")", GUI.skin.box);

            foreach (var patch in report.Patches)
            {
                GUILayout.Label("- " + patch.TargetType + "." + patch.TargetMethod + " [" + patch.PatchType + "] by " + patch.Owner + " -> " + patch.PatchMethod);
            }

            if (report.Patches.Count == 0)
            {
                GUILayout.Label("- None");
            }
        }

        private static void DrawAssemblySection(DiagnosticReport report)
        {
            GUILayout.Space(8f);
            GUILayout.Label("Loaded Assemblies (" + report.Assemblies.Count + ")", GUI.skin.box);

            foreach (var assembly in report.Assemblies)
            {
                var originalColor = GUI.contentColor;
                if (assembly.IsHarmonyRelated)
                {
                    GUI.contentColor = new Color(1f, 0.88f, 0.45f, 1f);
                }

                GUILayout.Label("- " + assembly.Name + " v" + assembly.Version + " :: " + assembly.Location);
                GUI.contentColor = originalColor;
            }

            if (report.Assemblies.Count == 0)
            {
                GUILayout.Label("- None");
            }
        }

        private static void DrawMissingPatchesSection(DiagnosticReport report)
        {
            GUILayout.Space(8f);
            var warningColor = new Color(1f, 0.6f, 0.4f, 1f);
            var originalColor = GUI.contentColor;
            GUI.contentColor = warningColor;
            GUILayout.Label("Missing Harmony Patches (" + report.PatchComparison.MissingPatches.Count + ")", GUI.skin.box);
            GUI.contentColor = originalColor;

            foreach (var missing in report.PatchComparison.MissingPatches)
            {
                string patchTypeLabel;
                if (missing.PatchType == PatchType.Prefix)
                {
                    patchTypeLabel = "Prefix";
                }
                else if (missing.PatchType == PatchType.Postfix)
                {
                    patchTypeLabel = "Postfix";
                }
                else if (missing.PatchType == PatchType.Transpiler)
                {
                    patchTypeLabel = "Transpiler";
                }
                else
                {
                    patchTypeLabel = "Finalizer";
                }

                GUILayout.Label("- [" + missing.PatchAssembly + "] " + patchTypeLabel + " for " + missing.TargetMethod + " in " + missing.TargetType);
            }

            if (report.PatchComparison.MissingPatches.Count == 0)
            {
                GUILayout.Label("- None");
            }
        }

        private static void DrawDllIntegritySection(DiagnosticReport report)
        {
            GUILayout.Space(8f);
            GUILayout.Label("DLL Integrity (" + report.DllIntegrity.Findings.Count + " checked, " + report.DllIntegrity.TotalRewrittenCount + " rewritten)", GUI.skin.box);

            if (report.DllIntegrity.Findings.Count == 0)
            {
                GUILayout.Label("  - None");
            }
            else
            {
                foreach (var finding in report.DllIntegrity.Findings)
                {
                    var originalColor = GUI.contentColor;
                    GUI.contentColor = GetSeverityColor(finding.Severity);
                    GUILayout.Label("  " + finding.DllName + " \u2014 " + finding.Summary);
                    if (finding.WasRewritten && (finding.OnDiskHarmonyReferences.Count > 0 || finding.OriginalHarmonyReferences.Count > 0))
                    {
                        var onDiskRefs = JoinReferences(finding.OnDiskHarmonyReferences);
                        var originalRefs = JoinReferences(finding.OriginalHarmonyReferences);
                        GUILayout.Label("           On-disk: " + onDiskRefs + " | Original: " + originalRefs);
                    }

                    GUI.contentColor = originalColor;
                }
            }

            if (report.DllIntegrity.ShimBackupDirectoryExists)
            {
                GUILayout.Label("  Shim Backup: " + report.DllIntegrity.ShimBackupDirectoryPath);
            }
            else
            {
                GUILayout.Label("  Shim Backup: Not found");
            }

            if (report.DllIntegrity.InteropCacheExists)
            {
                GUILayout.Label("  Interop Cache: " + report.DllIntegrity.InteropCachePath + " (" + report.DllIntegrity.InteropCacheEntryCount + " entries)");
            }
            else
            {
                GUILayout.Label("  Interop Cache: Not found");
            }

            GUILayout.Label("  Inspection Mode: " + (report.DllIntegrity.MonoCecilAvailable ? "Deep (Mono.Cecil)" : "Basic (byte scan)"));
        }

        private static void DrawFilesystemValidationSection(DiagnosticReport report)
        {
            GUILayout.Space(8f);
            var issues = report.FilesystemValidation.Issues;
            GUILayout.Label("Filesystem Validation (" + issues.Count + " issues)", GUI.skin.box);

            if (issues.Count == 0)
            {
                GUILayout.Label("  - No issues found");
            }
            else
            {
                foreach (var issue in issues)
                {
                    var originalColor = GUI.contentColor;
                    GUI.contentColor = GetSeverityColor(issue.Severity);
                    GUILayout.Label("  [" + GetSeverityLabel(issue.Severity) + "] " + issue.Description);
                    if (!string.IsNullOrEmpty(issue.FixSuggestion))
                    {
                        GUILayout.Label("    Fix: " + issue.FixSuggestion);
                    }

                    GUI.contentColor = originalColor;
                }
            }
        }

        private static void DrawErrorLogsSection(DiagnosticReport report)
        {
            GUILayout.Space(8f);
            var entries = report.ErrorLogReport.Entries;
            GUILayout.Label("Error Logs (" + entries.Count + " found)", GUI.skin.box);

            if (entries.Count == 0)
            {
                GUILayout.Label("  - No error logs found");
            }
            else
            {
                foreach (var entry in entries)
                {
                    var originalColor = GUI.contentColor;
                    GUI.contentColor = new Color(1f, 0.6f, 0.4f, 1f);
                    GUILayout.Label("  - " + entry.FileName);
                    GUI.contentColor = originalColor;
                }
            }
        }

        private static void DrawKnownIssuesSection(DiagnosticReport report)
        {
            GUILayout.Space(8f);
            var matches = report.KnownIssuesReport.Matches;
            GUILayout.Label("Known Issues (" + matches.Count + ")", GUI.skin.box);

            if (!string.IsNullOrEmpty(report.KnownIssuesReport.DatabaseVersion))
            {
                GUILayout.Label("  Database v" + report.KnownIssuesReport.DatabaseVersion);
            }

            if (matches.Count == 0)
            {
                GUILayout.Label("  - No known issues detected");
            }
            else
            {
                foreach (var match in matches)
                {
                    var originalColor = GUI.contentColor;
                    GUI.contentColor = GetSeverityColor(match.Severity);
                    GUILayout.Label("  [" + GetSeverityLabel(match.Severity) + "] " + match.ModName + ": " + match.Description);
                    if (!string.IsNullOrEmpty(match.FixSuggestion))
                    {
                        GUILayout.Label("    Fix: " + match.FixSuggestion);
                    }

                    GUI.contentColor = originalColor;
                }
            }
        }

        private static void DrawDependenciesSection(DiagnosticReport report)
        {
            GUILayout.Space(8f);
            var issues = report.DependencyReport.Issues;
            GUILayout.Label("Dependencies (" + issues.Count + " issues)", GUI.skin.box);

            if (!string.IsNullOrEmpty(report.DependencyReport.BaseModVersion))
            {
                GUILayout.Label("  BaseMod Version: " + report.DependencyReport.BaseModVersion);
            }

            GUILayout.Label("  BaseModList_v2.xml: " + (report.DependencyReport.BaseModListExists ? "Found" : "Missing"));

            if (issues.Count == 0)
            {
                GUILayout.Label("  - No dependency issues");
            }
            else
            {
                foreach (var issue in issues)
                {
                    var originalColor = GUI.contentColor;
                    GUI.contentColor = GetSeverityColor(issue.Severity);
                    GUILayout.Label("  [" + GetSeverityLabel(issue.Severity) + "] " + issue.Description);
                    if (!string.IsNullOrEmpty(issue.FixSuggestion))
                    {
                        GUILayout.Label("    Fix: " + issue.FixSuggestion);
                    }

                    GUI.contentColor = originalColor;
                }
            }
        }

        private static Color GetSeverityColor(FindingSeverity severity)
        {
            if (severity == FindingSeverity.Info)
            {
                return new Color(0.66f, 1f, 0.66f, 1f);
            }

            if (severity == FindingSeverity.Warning)
            {
                return new Color(1f, 0.6f, 0.4f, 1f);
            }

            return new Color(1f, 0.4f, 0.4f, 1f);
        }

        private static string GetSeverityLabel(FindingSeverity severity)
        {
            if (severity == FindingSeverity.Info)
            {
                return "Info";
            }

            if (severity == FindingSeverity.Warning)
            {
                return "Warning";
            }

            return "Error";
        }

        private static string JoinReferences(IList<string> items)
        {
            if (items.Count == 0)
            {
                return "(none)";
            }

            var result = items[0];
            for (var i = 1; i < items.Count; i++)
            {
                result += ", " + items[i];
            }

            return result;
        }
    }
}
