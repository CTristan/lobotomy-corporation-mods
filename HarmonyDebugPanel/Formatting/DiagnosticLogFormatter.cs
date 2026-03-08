// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using HarmonyDebugPanel.Models;

namespace HarmonyDebugPanel.Formatting
{
    public static class DiagnosticLogFormatter
    {
        public static IList<string> Format(DiagnosticReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            var lines = new List<string>
            {
                "=== Harmony Diagnostic Report ===",
                "Collected At: " + report.CollectedAt.ToString("u"),
            };

            var bepInExPlugins = FilterBySource(report.Mods, ModSource.BepInExPlugin);
            var lmmMods = FilterBySource(report.Mods, ModSource.Lmm);

            lines.Add("BepInEx Plugins (" + bepInExPlugins.Count + "):");
            AddMods(lines, bepInExPlugins);

            lines.Add("LMM/Basemod Mods (" + lmmMods.Count + "):");
            AddMods(lines, lmmMods);

            lines.Add("RetargetHarmony: " + report.RetargetHarmonyStatus.Message);
            lines.Add("Active Harmony Patches: " + report.Patches.Count + " patches");

            if (report.MissingPatches.Count > 0)
            {
                lines.Add("Missing Harmony Patches (" + report.MissingPatches.Count + "):");
                AddMissingPatches(lines, report.MissingPatches);
            }

            foreach (var warning in report.Warnings)
            {
                lines.Add("Warning: " + warning);
            }

            if (report.DebugInfo.Count > 0)
            {
                lines.Add("Debug Info (" + report.DebugInfo.Count + "):");
                foreach (var debug in report.DebugInfo)
                {
                    lines.Add("  - " + debug);
                }
            }

            lines.Add("===============================");
            return lines;
        }

        private static List<ModInfo> FilterBySource(IList<ModInfo> mods, ModSource source)
        {
            var filtered = new List<ModInfo>();
            foreach (var mod in mods)
            {
                if (mod.Source == source)
                {
                    filtered.Add(mod);
                }
            }

            return filtered;
        }

        private static void AddMods(List<string> lines, List<ModInfo> mods)
        {
            if (mods.Count == 0)
            {
                lines.Add("  - None");
                return;
            }

            foreach (var mod in mods)
            {
                var identifierSuffix = string.IsNullOrEmpty(mod.Identifier)
                    ? string.Empty
                    : " [" + mod.Identifier + "]";

                string patchStatus;
                if (mod.ExpectedPatchCount >= 0)
                {
                    var failedCount = mod.ActivePatchCount < mod.ExpectedPatchCount ? (mod.ExpectedPatchCount - mod.ActivePatchCount) : 0;
                    if (mod.ActivePatchCount == mod.ExpectedPatchCount)
                    {
                        patchStatus = $" [{mod.ActivePatchCount} loaded/{mod.ExpectedPatchCount} expected, {failedCount} failed]";
                    }
                    else if (mod.ActivePatchCount < mod.ExpectedPatchCount)
                    {
                        patchStatus = $" [{mod.ActivePatchCount} loaded/{mod.ExpectedPatchCount} expected, {failedCount} failed]";
                    }
                    else
                    {
                        patchStatus = $" [{mod.ActivePatchCount} loaded/{mod.ExpectedPatchCount} expected, {failedCount} failed]";
                    }
                }
                else
                {
                    patchStatus = mod.HasActivePatches ? $" [{mod.ActivePatchCount} loaded]" : " [0 loaded]";
                }

                lines.Add("  - " + mod.Name + " v" + mod.Version + " [" + ToHarmonyVersionLabel(mod.HarmonyVersion) + "]" + patchStatus + identifierSuffix);
            }
        }

        private static void AddMissingPatches(List<string> lines, IList<MissingPatchInfo> missingPatches)
        {
            foreach (var missing in missingPatches)
            {
                var prefix = missing.PatchType == PatchType.Prefix ? "Prefix" : missing.PatchType == PatchType.Postfix ? "Postfix" : "Transpiler";
                lines.Add("  - [" + missing.PatchAssembly + "] " + prefix + " for " + missing.TargetMethod + " in " + missing.TargetType);
            }
        }

        public static string ToHarmonyVersionLabel(HarmonyVersion harmonyVersion)
        {
            switch (harmonyVersion)
            {
                case HarmonyVersion.Harmony1:
                    return "Harmony 1";
                case HarmonyVersion.Harmony2:
                    return "Harmony 2";
                default:
                    return "Unknown";
            }
        }
    }
}
