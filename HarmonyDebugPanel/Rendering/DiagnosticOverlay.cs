// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyDebugPanel.Formatting;
using HarmonyDebugPanel.Models;
using UnityEngine;

namespace HarmonyDebugPanel.Rendering
{
    [ExcludeFromCodeCoverage(Justification = "Unity IMGUI rendering is covered by manual testing")]
    public sealed class DiagnosticOverlay
    {
        private Rect _windowRect = new Rect(20f, 20f, 600f, 450f);
        private Vector2 _scrollPosition = Vector2.zero;

        public void Draw(DiagnosticReport report, PluginConfiguration configuration, Action refreshAction)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (refreshAction == null)
            {
                throw new ArgumentNullException(nameof(refreshAction));
            }

            string title = "Harmony Debug Panel (" + configuration.OverlayToggleHotkey + " to hide)";
            _windowRect = GUI.Window(0xD1349, _windowRect, id => DrawWindowContents(id, report, configuration, refreshAction), title);
        }

        private void DrawWindowContents(int windowId, DiagnosticReport report, PluginConfiguration configuration, Action refreshAction)
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Collected: " + report.CollectedAt.ToString("u"));
            if (GUILayout.Button("Refresh", GUILayout.Width(100f)))
            {
                refreshAction();
            }

            GUILayout.EndHorizontal();

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            if (configuration.ShowBepInExPlugins)
            {
                DrawModSection(report, ModSource.BepInExPlugin, "BepInEx Plugins");
            }

            if (configuration.ShowLmmMods)
            {
                DrawModSection(report, ModSource.Lmm, "LMM/Basemod Mods");
            }

            GUILayout.Space(8f);
            GUILayout.Label("RetargetHarmony: " + report.RetargetHarmonyStatus.Message);

            if (configuration.ShowActivePatches)
            {
                DrawPatchSection(report);
            }

            if (configuration.ShowAssemblyInfo)
            {
                DrawAssemblySection(report);
            }

            if (report.MissingPatches.Count > 0)
            {
                DrawMissingPatchesSection(report);
            }

            if (report.Warnings.Count > 0)
            {
                GUILayout.Space(8f);
                GUILayout.Label("Warnings", GUI.skin.box);
                foreach (var warning in report.Warnings)
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
                var identifierSuffix = string.IsNullOrEmpty(mod.Identifier) ? string.Empty : " [" + mod.Identifier + "]";

                string patchStatus;
                if (mod.ExpectedPatchCount >= 0)
                {
                    var failedCount = mod.ActivePatchCount < mod.ExpectedPatchCount ? (mod.ExpectedPatchCount - mod.ActivePatchCount) : 0;
                    if (mod.ActivePatchCount == mod.ExpectedPatchCount)
                    {
                        patchStatus = $" ({mod.ActivePatchCount} loaded/{mod.ExpectedPatchCount} expected, {failedCount} failed)";
                    }
                    else if (mod.ActivePatchCount < mod.ExpectedPatchCount)
                    {
                        // Missing patches - show warning color
                        var warningColor = new Color(1f, 0.6f, 0.4f, 1f);
                        GUI.contentColor = warningColor;
                        patchStatus = $" ({mod.ActivePatchCount} loaded/{mod.ExpectedPatchCount} expected, {failedCount} failed)";
                    }
                    else
                    {
                        // More patches loaded than expected - shouldn't happen but handle gracefully
                        patchStatus = $" ({mod.ActivePatchCount} loaded/{mod.ExpectedPatchCount} expected, {failedCount} failed)";
                    }
                }
                else
                {
                    patchStatus = mod.HasActivePatches ? $" ({mod.ActivePatchCount} loaded)" : " (0 loaded)";
                }

                GUILayout.Label("- " + mod.Name + " v" + mod.Version + " (" + DiagnosticLogFormatter.ToHarmonyVersionLabel(mod.HarmonyVersion) + ")" + patchStatus + identifierSuffix);
                GUI.contentColor = originalColor;
            }

            if (!hadEntries)
            {
                GUILayout.Label("- None");
            }
        }

        private static Color GetModColor(HarmonyVersion version)
        {
            switch (version)
            {
                case HarmonyVersion.Harmony1:
                    return new Color(0.56f, 0.86f, 1f, 1f);
                case HarmonyVersion.Harmony2:
                    return new Color(0.66f, 1f, 0.66f, 1f);
                default:
                    return Color.white;
            }
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
            GUILayout.Label("Missing Harmony Patches (" + report.MissingPatches.Count + ")", GUI.skin.box);
            GUI.contentColor = originalColor;

            foreach (var missing in report.MissingPatches)
            {
                var prefix = missing.PatchType == PatchType.Prefix ? "Prefix" : missing.PatchType == PatchType.Postfix ? "Postfix" : "Transpiler";
                GUILayout.Label("- [" + missing.PatchAssembly + "] " + prefix + " for " + missing.TargetMethod + " in " + missing.TargetType);
            }

            if (report.MissingPatches.Count == 0)
            {
                GUILayout.Label("- None");
            }
        }
    }
}
