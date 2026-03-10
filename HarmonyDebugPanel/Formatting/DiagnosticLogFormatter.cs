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

            List<string> lines =
            [
                "###############################################################################",
                "#                                                                             #",
                "#                     Harmony Diagnostic Report                             #",
                "#                                                                             #",
                "###############################################################################",
                "Collected At: " + report.CollectedAt.ToString("u"),
            ];

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
            List<ModInfo> filtered = [];
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
                    patchStatus = mod.ActivePatchCount == mod.ExpectedPatchCount
                        ? $" [{mod.ActivePatchCount} loaded/{mod.ExpectedPatchCount} expected, {failedCount} failed]"
                        : mod.ActivePatchCount < mod.ExpectedPatchCount
                            ? $" [{mod.ActivePatchCount} loaded/{mod.ExpectedPatchCount} expected, {failedCount} failed]"
                            : $" [{mod.ActivePatchCount} loaded/{mod.ExpectedPatchCount} expected, {failedCount} failed]";
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

        public static IList<string> FormatExtended(DiagnosticReport report, string runtimeLogContent, string retargetHarmonyLogContent = null, string bepInExLogContent = null, string unityLogContent = null)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            var lines = Format(report) as List<string> ?? [.. Format(report)];

            // Add Loaded Assemblies section
            lines.Add(string.Empty);
            lines.Add("========== LOADED ASSEMBLIES (" + report.Assemblies.Count + ") ==========");
            if (report.Assemblies.Count == 0)
            {
                lines.Add("  - None");
            }
            else
            {
                foreach (var assembly in report.Assemblies)
                {
                    var harmonyLabel = assembly.IsHarmonyRelated ? " [Harmony]" : string.Empty;
                    lines.Add("  - " + assembly.Name + " v" + assembly.Version + " :: " + assembly.Location + harmonyLabel);
                }
            }

            // Add Runtime Log section
            lines.Add(string.Empty);
            lines.Add("========== RUNTIME LOG ==========");
            if (string.IsNullOrEmpty(runtimeLogContent))
            {
                lines.Add("  - No runtime log entries");
            }
            else
            {
                var runtimeLines = runtimeLogContent.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in runtimeLines)
                {
                    lines.Add("  " + line);
                }
            }

            // Add RetargetHarmony Log section
            lines.Add(string.Empty);
            lines.Add("========== RETARGETHARMONY LOG ==========");
            if (string.IsNullOrEmpty(retargetHarmonyLogContent))
            {
                lines.Add("  - No RetargetHarmony log entries");
            }
            else
            {
                var rhLines = retargetHarmonyLogContent.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in rhLines)
                {
                    lines.Add("  " + line);
                }
            }

            // Add BepInEx LogOutput.log section
            lines.Add(string.Empty);
            lines.Add("========== BEPINEX LOGOUTPUT.LOG ==========");
            if (string.IsNullOrEmpty(bepInExLogContent))
            {
                lines.Add("  - No BepInEx LogOutput.log found or file does not exist");
            }
            else
            {
                var bepInExLines = bepInExLogContent.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in bepInExLines)
                {
                    lines.Add("  " + line);
                }
            }

            // Add Unity output_log.txt section
            lines.Add(string.Empty);
            lines.Add("========== UNITY OUTPUT_LOG.TXT ==========");
            if (string.IsNullOrEmpty(unityLogContent))
            {
                lines.Add("  - No Unity output_log.txt found or file does not exist");
            }
            else
            {
                var unityLines = unityLogContent.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in unityLines)
                {
                    lines.Add("  " + line);
                }
            }

            lines.Add("===============================");
            return lines;
        }

        public static string ToHarmonyVersionLabel(HarmonyVersion harmonyVersion)
        {
            switch (harmonyVersion)
            {
                case HarmonyVersion.Harmony1:
                    return "Harmony 1";
                case HarmonyVersion.Harmony2:
                    return "Harmony 2";
                case HarmonyVersion.Unknown:
                default:
                    return "Unknown";
            }
        }
    }
}
