// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.Common.Enums.Diagnostics;
using LobotomyCorporationMods.Common.Models.Diagnostics;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Implementations
{
    public sealed class ReportFormatter : IReportFormatter
    {
        public IList<string> FormatForOverlay(DiagnosticReport report)
        {
            ThrowHelper.ThrowIfNull(report);
            _ = report;

            var bepInExPlugins = FilterBySource(report.Mods, ModSource.BepInExPlugin);
            var lmmMods = FilterBySource(report.Mods, ModSource.Lmm);

            var lines = new List<string>
            {
                "Collected: " + report.CollectedAt.ToString("u"),
                "Mode: " + GetEnvironmentModeLabel(report.EnvironmentInfo)
            };

            if (report.EnvironmentInfo.IsBepInExAvailable)
            {
                lines.Add("BepInEx Plugins (" + bepInExPlugins.Count + "):");
                AddMods(lines, bepInExPlugins);
            }

            lines.Add("LMM/Basemod Mods (" + lmmMods.Count + "):");
            AddMods(lines, lmmMods);

            if (report.EnvironmentInfo.IsBepInExAvailable)
            {
                lines.Add("RetargetHarmony: " + report.RetargetHarmonyStatus.Message);
            }
            AddDllIntegrityOverlay(lines, report.DllIntegrity);
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

        public IList<string> FormatForLogFile(DiagnosticReport report, ExternalLogData externalLogs)
        {
            ThrowHelper.ThrowIfNull(report);
            _ = report;
            ThrowHelper.ThrowIfNull(externalLogs);
            _ = externalLogs;

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

            lines.Add(string.Empty);
            lines.Add("========== DLL INTEGRITY (" + report.DllIntegrity.Findings.Count + ") ==========");
            AddDllIntegrityLogFile(lines, report.DllIntegrity);

            lines.Add(string.Empty);
            lines.Add("========== RETARGETHARMONY LOG ==========");
            AddExternalLogContent(lines, externalLogs.RetargetHarmonyLog, "RetargetHarmony log");

            lines.Add(string.Empty);
            lines.Add("========== BEPINEX LOGOUTPUT.LOG ==========");
            AddExternalLogContent(lines, externalLogs.BepInExLog, "BepInEx LogOutput.log");

            lines.Add(string.Empty);
            lines.Add("========== UNITY OUTPUT_LOG.TXT ==========");
            AddExternalLogContent(lines, externalLogs.UnityLog, "Unity output_log.txt");

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

        private static void AddDllIntegrityOverlay(List<string> lines, DllIntegrityReport dllIntegrity)
        {
            lines.Add("DLL Integrity (" + dllIntegrity.Findings.Count + " checked, " + dllIntegrity.TotalRewrittenCount + " rewritten):");

            if (dllIntegrity.Findings.Count == 0)
            {
                lines.Add("  - None");
            }
            else
            {
                foreach (var finding in dllIntegrity.Findings)
                {
                    lines.Add("  " + finding.DllName + " [" + GetSeverityLabel(finding.Severity) + "] \u2014 " + finding.Summary);
                    if (finding.WasRewritten)
                    {
                        lines.Add("    On-disk: " + JoinStrings(finding.OnDiskHarmonyReferences) + " | Original: " + JoinStrings(finding.OriginalHarmonyReferences));
                    }
                }
            }

            lines.Add("  Inspection: " + (dllIntegrity.MonoCecilAvailable ? "Deep (Mono.Cecil)" : "Basic (byte scan)"));
        }

        private static void AddDllIntegrityLogFile(List<string> lines, DllIntegrityReport dllIntegrity)
        {
            lines.Add("  Summary: " + dllIntegrity.Summary);
            lines.Add("  Total Checked: " + dllIntegrity.Findings.Count);
            lines.Add("  Total Rewritten: " + dllIntegrity.TotalRewrittenCount);
            lines.Add("  Inspection Mode: " + (dllIntegrity.MonoCecilAvailable ? "Deep (Mono.Cecil)" : "Basic (byte scan)"));

            if (dllIntegrity.ShimBackupDirectoryExists)
            {
                lines.Add("  Shim Backup: " + dllIntegrity.ShimBackupDirectoryPath);
            }
            else
            {
                lines.Add("  Shim Backup: Not found");
            }

            if (dllIntegrity.InteropCacheExists)
            {
                lines.Add("  Interop Cache: " + dllIntegrity.InteropCachePath + " (" + dllIntegrity.InteropCacheEntryCount + " entries)");
            }
            else
            {
                lines.Add("  Interop Cache: Not found");
            }

            if (dllIntegrity.Findings.Count > 0)
            {
                lines.Add(string.Empty);
                lines.Add("  Findings:");
                foreach (var finding in dllIntegrity.Findings)
                {
                    lines.Add("    [" + GetSeverityLabel(finding.Severity) + "] " + finding.DllName + " \u2014 " + finding.Summary);
                    lines.Add("      Path: " + finding.DllPath);
                    if (finding.WasRewritten)
                    {
                        lines.Add("      On-disk refs: " + JoinStrings(finding.OnDiskHarmonyReferences));
                        lines.Add("      Original refs: " + JoinStrings(finding.OriginalHarmonyReferences));
                    }

                    if (finding.HasBackup)
                    {
                        lines.Add("      Backup: " + finding.BackupPath);
                    }
                }
            }

            if (dllIntegrity.Warnings.Count > 0)
            {
                foreach (var warning in dllIntegrity.Warnings)
                {
                    lines.Add("  Warning: " + warning);
                }
            }
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

        private static void AddExternalLogContent(List<string> lines, string logContent, string logName)
        {
            if (string.IsNullOrEmpty(logContent))
            {
                lines.Add("  - No " + logName + " found or file does not exist");

                return;
            }

            var logLines = logContent.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
            foreach (var line in logLines)
            {
                lines.Add("  " + line);
            }
        }

        private static string JoinStrings(IList<string> items)
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
