// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.DebugPanel.Models;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Implementations
{
    public sealed class ReportFormatter : IReportFormatter
    {
        public IList<string> FormatForOverlay(DiagnosticReport report)
        {
            _ = Guard.Against.Null(report, nameof(report));

            var bepInExPlugins = FilterBySource(report.Mods, ModSource.BepInExPlugin);
            var lmmMods = FilterBySource(report.Mods, ModSource.Lmm);

            var lines = new List<string>
            {
                "Collected: " + report.CollectedAt.ToString("u"),
                "Mode: " + GetEnvironmentModeLabel(report.EnvironmentInfo),
                "BepInEx Plugins (" + bepInExPlugins.Count + "):"
            };
            AddMods(lines, bepInExPlugins);

            lines.Add("LMM/Basemod Mods (" + lmmMods.Count + "):");
            AddMods(lines, lmmMods);

            lines.Add("RetargetHarmony: " + report.RetargetHarmonyStatus.Message);
            lines.Add("Active Harmony Patches: " + report.Patches.Count + " patches");

            if (report.PatchComparison.HasMissingPatches)
            {
                lines.Add("Missing Harmony Patches (" + report.PatchComparison.MissingPatches.Count + "):");
                AddMissingPatches(lines, report.PatchComparison.MissingPatches);
            }

            if (report.Warnings.Count > 0)
            {
                foreach (var warning in report.Warnings)
                {
                    lines.Add("Warning: " + warning);
                }
            }

            return lines;
        }

        public IList<string> FormatForLogFile(DiagnosticReport report)
        {
            _ = Guard.Against.Null(report, nameof(report));

            var bepInExPlugins = FilterBySource(report.Mods, ModSource.BepInExPlugin);
            var lmmMods = FilterBySource(report.Mods, ModSource.Lmm);

            var lines = new List<string>
            {
                "###############################################################################",
                "#                                                                             #",
                "#                     Harmony Diagnostic Report                               #",
                "#                                                                             #",
                "###############################################################################",
                "Collected At: " + report.CollectedAt.ToString("u"),
                "Mode: " + GetEnvironmentModeLabel(report.EnvironmentInfo),
                "BepInEx Plugins (" + bepInExPlugins.Count + "):"
            };
            AddMods(lines, bepInExPlugins);

            lines.Add("LMM/Basemod Mods (" + lmmMods.Count + "):");
            AddMods(lines, lmmMods);

            lines.Add("RetargetHarmony: " + report.RetargetHarmonyStatus.Message);
            lines.Add("Active Harmony Patches: " + report.Patches.Count + " patches");

            if (report.PatchComparison.HasMissingPatches)
            {
                lines.Add("Missing Harmony Patches (" + report.PatchComparison.MissingPatches.Count + "):");
                AddMissingPatches(lines, report.PatchComparison.MissingPatches);
            }

            if (report.Warnings.Count > 0)
            {
                foreach (var warning in report.Warnings)
                {
                    lines.Add("Warning: " + warning);
                }
            }

            lines.Add(string.Empty);
            lines.Add("========== ENVIRONMENT ==========");
            AddEnvironmentInfo(lines, report.EnvironmentInfo);

            lines.Add(string.Empty);
            lines.Add("========== LOADED ASSEMBLIES (" + report.Assemblies.Count + ") ==========");
            AddAssemblies(lines, report.Assemblies);

            if (report.DebugInfo.Count > 0)
            {
                lines.Add(string.Empty);
                lines.Add("========== DEBUG INFO (" + report.DebugInfo.Count + ") ==========");
                foreach (var debug in report.DebugInfo)
                {
                    lines.Add("  - " + debug);
                }
            }

            lines.Add("===============================");

            return lines;
        }

        public static string ToHarmonyVersionLabel(HarmonyVersion harmonyVersion)
        {
            if (harmonyVersion == HarmonyVersion.Harmony1)
            {
                return "Harmony 1";
            }

            if (harmonyVersion == HarmonyVersion.Harmony2)
            {
                return "Harmony 2";
            }

            return "Unknown";
        }

        private static string GetEnvironmentModeLabel(EnvironmentInfo environmentInfo)
        {
            if (environmentInfo.IsHarmony2Available && environmentInfo.IsBepInExAvailable)
            {
                return "Enhanced (BepInEx + Harmony 2)";
            }

            if (environmentInfo.IsHarmony2Available)
            {
                return "Enhanced (Harmony 2)";
            }

            return "Standalone (Harmony 1.09)";
        }

        private static List<DetectedModInfo> FilterBySource(IList<DetectedModInfo> mods, ModSource source)
        {
            var filtered = new List<DetectedModInfo>();
            foreach (var mod in mods)
            {
                if (mod.Source == source)
                {
                    filtered.Add(mod);
                }
            }

            return filtered;
        }

        private static void AddMods(List<string> lines, List<DetectedModInfo> mods)
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
                    var failedCount = mod.ActivePatchCount < mod.ExpectedPatchCount
                        ? mod.ExpectedPatchCount - mod.ActivePatchCount
                        : 0;
                    patchStatus = $" [{mod.ActivePatchCount} loaded/{mod.ExpectedPatchCount} expected, {failedCount} failed]";
                }
                else
                {
                    patchStatus = mod.HasActivePatches
                        ? $" [{mod.ActivePatchCount} loaded]"
                        : " [0 loaded]";
                }

                lines.Add("  - " + mod.Name + " v" + mod.Version + " [" + ToHarmonyVersionLabel(mod.HarmonyVersion) + "]" + patchStatus + identifierSuffix);
            }
        }

        private static void AddMissingPatches(List<string> lines, IList<MissingPatchInfo> missingPatches)
        {
            foreach (var missing in missingPatches)
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

                lines.Add("  - [" + missing.PatchAssembly + "] " + patchTypeLabel + " for " + missing.TargetMethod + " in " + missing.TargetType);
            }
        }

        private static void AddEnvironmentInfo(List<string> lines, EnvironmentInfo environmentInfo)
        {
            lines.Add("  Harmony 2: " + (environmentInfo.IsHarmony2Available ? "Available" : "Not detected"));
            lines.Add("  BepInEx: " + (environmentInfo.IsBepInExAvailable ? "Available" : "Not detected"));
            lines.Add("  Mono.Cecil: " + (environmentInfo.IsMonoCecilAvailable ? "Available" : "Not detected"));
        }

        private static void AddAssemblies(List<string> lines, IList<AssemblyInfo> assemblies)
        {
            if (assemblies.Count == 0)
            {
                lines.Add("  - None");

                return;
            }

            foreach (var assembly in assemblies)
            {
                var harmonyLabel = assembly.IsHarmonyRelated ? " [Harmony]" : string.Empty;
                lines.Add("  - " + assembly.Name + " v" + assembly.Version + " :: " + assembly.Location + harmonyLabel);
            }
        }
    }
}
