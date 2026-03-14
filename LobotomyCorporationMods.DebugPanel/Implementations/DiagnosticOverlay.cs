// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.DebugPanel.JsonModels;
using LobotomyCorporationMods.DebugPanel.Models;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Implementations
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class DiagnosticOverlay : IOverlayRenderer
    {
        private const int WindowId = 0xD1349;

        private Rect _windowRect = new Rect(20f, 20f, 600f, 450f);
        private Vector2 _scrollPosition = Vector2.zero;

        private DiagnosticReport _currentReport;
        private DebugPanelConfig _currentConfig;
        private Action _currentRefreshAction;
        private Action _currentGenerateLogAction;

        public void Draw(DiagnosticReport report, DebugPanelConfig config, Action refreshAction, Action generateLogAction)
        {
            _currentReport = Guard.Against.Null(report, nameof(report));
            _currentConfig = Guard.Against.Null(config, nameof(config));
            _currentRefreshAction = Guard.Against.Null(refreshAction, nameof(refreshAction));
            _currentGenerateLogAction = generateLogAction;

            var title = "Debug Panel (" + config.OverlayToggleKey + " to hide)";
            _windowRect = GUI.Window(WindowId, _windowRect, DrawWindowContents, title);
        }

        private void DrawWindowContents(int windowId)
        {
            GUILayout.BeginVertical();

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

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            if (_currentConfig.ShowBepInExPlugins)
            {
                DrawModSection(_currentReport, ModSource.BepInExPlugin, "BepInEx Plugins");
            }

            if (_currentConfig.ShowLmmMods)
            {
                DrawModSection(_currentReport, ModSource.Lmm, "LMM/Basemod Mods");
            }

            GUILayout.Space(8f);
            GUILayout.Label("RetargetHarmony: " + _currentReport.RetargetHarmonyStatus.Message);

            if (_currentConfig.ShowActivePatches)
            {
                DrawPatchSection(_currentReport);
            }

            if (_currentConfig.ShowAssemblyInfo)
            {
                DrawAssemblySection(_currentReport);
            }

            if (_currentReport.PatchComparison.HasMissingPatches)
            {
                DrawMissingPatchesSection(_currentReport);
            }

            if (_currentReport.Warnings.Count > 0)
            {
                GUILayout.Space(8f);
                GUILayout.Label("Warnings", GUI.skin.box);
                foreach (var warning in _currentReport.Warnings)
                {
                    GUILayout.Label("- " + warning);
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0f, 0f, 10000f, 20f));
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
    }
}
