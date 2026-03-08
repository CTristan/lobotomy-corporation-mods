// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyDebugPanel.Implementations.Collectors;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;

namespace HarmonyDebugPanel
{
    public sealed class DiagnosticReportBuilder
    {
        private readonly IInfoCollector<IList<ModInfo>> _bepInExPluginCollector;
        private readonly IInfoCollector<IList<ModInfo>> _baseModCollector;
        private readonly IInfoCollector<IList<PatchInfo>> _activePatchCollector;
        private readonly IInfoCollector<IList<AssemblyInfo>> _assemblyInfoCollector;
        private readonly IInfoCollector<RetargetHarmonyStatus> _retargetHarmonyDetector;
        private readonly IExpectedPatchSource _expectedPatchSource;

        public DiagnosticReportBuilder()
        {
            var patchInspectionSource = new HarmonyPatchInspectionSource();
            _bepInExPluginCollector = new BepInExPluginCollector();
            _baseModCollector = new BaseModCollector(patchInspectionSource, new HarmonyVersionClassifier());
            _activePatchCollector = new ActivePatchCollector(patchInspectionSource);
            _assemblyInfoCollector = new AssemblyInfoCollector();
            _retargetHarmonyDetector = new RetargetHarmonyDetector();
            _expectedPatchSource = new ExpectedPatchSource();
        }

        public DiagnosticReportBuilder(
            IInfoCollector<IList<ModInfo>> bepInExPluginCollector,
            IInfoCollector<IList<ModInfo>> baseModCollector,
            IInfoCollector<IList<PatchInfo>> activePatchCollector,
            IInfoCollector<IList<AssemblyInfo>> assemblyInfoCollector,
            IInfoCollector<RetargetHarmonyStatus> retargetHarmonyDetector,
            IExpectedPatchSource expectedPatchSource)
        {
            _bepInExPluginCollector = bepInExPluginCollector ?? throw new ArgumentNullException(nameof(bepInExPluginCollector));
            _baseModCollector = baseModCollector ?? throw new ArgumentNullException(nameof(baseModCollector));
            _activePatchCollector = activePatchCollector ?? throw new ArgumentNullException(nameof(activePatchCollector));
            _assemblyInfoCollector = assemblyInfoCollector ?? throw new ArgumentNullException(nameof(assemblyInfoCollector));
            _retargetHarmonyDetector = retargetHarmonyDetector ?? throw new ArgumentNullException(nameof(retargetHarmonyDetector));
            _expectedPatchSource = expectedPatchSource ?? throw new ArgumentNullException(nameof(expectedPatchSource));
        }

        public DiagnosticReport BuildReport()
        {
            var report = new DiagnosticReport();

            Append(report.Mods, CollectSafe(_bepInExPluginCollector, "BepInExPluginCollector", report.Warnings));
            Append(report.Mods, CollectSafe(_baseModCollector, "BaseModCollector", report.Warnings));
            Append(report.Patches, CollectSafe(_activePatchCollector, "ActivePatchCollector", report.Warnings));
            Append(report.Assemblies, CollectSafe(_assemblyInfoCollector, "AssemblyInfoCollector", report.Warnings));
            report.RetargetHarmonyStatus = CollectSafe(_retargetHarmonyDetector, "RetargetHarmonyDetector", report.Warnings);
            report.CollectedAt = DateTime.UtcNow;

            // Collect expected patches and compare with actual patches
#pragma warning disable CA1031
            IList<ExpectedPatchInfo> expectedPatches;
            try
            {
                expectedPatches = _expectedPatchSource.GetExpectedPatches(report.DebugInfo);
            }
            catch (Exception ex)
            {
                report.Warnings.Add("ExpectedPatchSource failed: " + ex.Message);
                expectedPatches = new List<ExpectedPatchInfo>();
            }
#pragma warning restore CA1031
            CompareExpectedWithActual(report, expectedPatches, report.Patches);

            // Correlate patches with their originating mods
            CorrelatePatchesWithMods(report.Mods, report.Patches);

            return report;
        }

        private static void CompareExpectedWithActual(DiagnosticReport report, IEnumerable<ExpectedPatchInfo> expectedPatches, IList<PatchInfo> actualPatches)
        {
            if (report == null || expectedPatches == null || actualPatches == null)
            {
                return;
            }

            // Group expected patches by assembly
            var expectedByAssembly = new Dictionary<string, List<ExpectedPatchInfo>>(StringComparer.OrdinalIgnoreCase);
            foreach (var expected in expectedPatches)
            {
                if (expected == null)
                {
                    continue;
                }

                if (!expectedByAssembly.TryGetValue(expected.PatchAssembly, out var list))
                {
                    list = new List<ExpectedPatchInfo>();
                    expectedByAssembly[expected.PatchAssembly] = list;
                }
                list.Add(expected);
            }

            // Group actual patches by assembly
            var actualByAssembly = new Dictionary<string, List<PatchInfo>>(StringComparer.OrdinalIgnoreCase);
            foreach (var actual in actualPatches)
            {
                if (actual == null || string.IsNullOrEmpty(actual.PatchAssemblyName))
                {
                    continue;
                }

                if (!actualByAssembly.TryGetValue(actual.PatchAssemblyName, out var list))
                {
                    list = new List<PatchInfo>();
                    actualByAssembly[actual.PatchAssemblyName] = list;
                }
                list.Add(actual);
            }

            // Update mods with expected patch counts and find missing patches
            foreach (var mod in report.Mods)
            {
                if (mod == null || string.IsNullOrEmpty(mod.AssemblyName))
                {
                    continue;
                }

                if (expectedByAssembly.TryGetValue(mod.AssemblyName, out var expectedForMod))
                {
                    mod.SetExpectedPatchCount(expectedForMod.Count);

                    // Find missing patches
                    foreach (var expected in expectedForMod)
                    {
                        var isMissing = true;

                        if (actualByAssembly.TryGetValue(mod.AssemblyName, out var actualForMod))
                        {
                            // Check if there's a matching actual patch
                            foreach (var actual in actualForMod)
                            {
                                if (Matches(expected, actual))
                                {
                                    isMissing = false;
                                    break;
                                }
                            }
                        }

                        if (isMissing)
                        {
                            report.MissingPatches.Add(new MissingPatchInfo(
                                expected.PatchAssembly,
                                expected.TargetType,
                                expected.TargetMethod,
                                expected.PatchMethod,
                                expected.PatchType));
                        }
                    }
                }
            }
        }

        private static bool Matches(ExpectedPatchInfo expected, PatchInfo actual)
        {
            // Compare target type, target method, and patch type
            // TargetType from source might not have namespace, so we check if it matches the end
            bool typeMatches = expected.TargetType == actual.TargetType ||
                              actual.TargetType.EndsWith("." + expected.TargetType, StringComparison.Ordinal);

            return expected.PatchAssembly == actual.PatchAssemblyName &&
                   typeMatches &&
                   expected.TargetMethod == actual.TargetMethod &&
                   expected.PatchType == actual.PatchType;
        }

        private static void Append<T>(IList<T> destination, IList<T> source)
        {
            if (destination == null || source == null)
            {
                return;
            }

            foreach (var value in source)
            {
                destination.Add(value);
            }
        }

#pragma warning disable CA1031 // Diagnostics should tolerate individual collector failures and continue
        private static IList<T> CollectSafe<T>(IInfoCollector<IList<T>> collector, string collectorName, IList<string> warnings)
        {
            try
            {
                return collector.Collect();
            }
            catch (Exception ex)
            {
                warnings.Add(collectorName + " failed: " + ex.Message);
                return new List<T>();
            }
        }

        private static T CollectSafe<T>(IInfoCollector<T> collector, string collectorName, IList<string> warnings)
            where T : new()
        {
            try
            {
                return collector.Collect();
            }
            catch (Exception ex)
            {
                warnings.Add(collectorName + " failed: " + ex.Message);
                return new T();
            }
        }

        private static T CollectSafe<TCollector, T>(TCollector collector, string collectorName, IList<string> warnings)
            where TCollector : IInfoCollector<T>
            where T : class
        {
            try
            {
                return collector.Collect();
            }
            catch (Exception ex)
            {
                warnings.Add(collectorName + " failed: " + ex.Message);
                return null;
            }
        }

        private static void CorrelatePatchesWithMods(IList<ModInfo> mods, IList<PatchInfo> patches)
        {
            if (mods == null || patches == null)
            {
                return;
            }

            // Create a mapping from assembly name to mod info
            var assemblyNameToMod = new Dictionary<string, ModInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var mod in mods)
            {
                if (mod != null && !string.IsNullOrEmpty(mod.AssemblyName))
                {
                    assemblyNameToMod[mod.AssemblyName] = mod;
                    // Reset counts to ensure no double counting from collectors
                    mod.SetPatchInfo(false, 0);
                }
            }

            // For each patch, find the originating mod by matching patch assembly name to mod assembly name
            foreach (var patch in patches)
            {
                if (patch == null || string.IsNullOrEmpty(patch.PatchAssemblyName))
                {
                    continue;
                }

                // Direct match on assembly name
                if (assemblyNameToMod.TryGetValue(patch.PatchAssemblyName, out var mod))
                {
                    mod.SetPatchInfo(true, mod.ActivePatchCount + 1);
                }
            }
        }
#pragma warning restore CA1031
    }
}
