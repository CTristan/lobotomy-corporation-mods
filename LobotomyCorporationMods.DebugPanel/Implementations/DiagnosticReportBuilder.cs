// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.DebugPanel.Models;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Implementations
{
    public sealed class DiagnosticReportBuilder : IDiagnosticReportBuilder
    {
        private readonly ICollectorFactory _collectorFactory;
        private readonly IEnvironmentDetector _environmentDetector;

        public DiagnosticReportBuilder(ICollectorFactory collectorFactory, IEnvironmentDetector environmentDetector)
        {
            _collectorFactory = Guard.Against.Null(collectorFactory, nameof(collectorFactory));
            _environmentDetector = Guard.Against.Null(environmentDetector, nameof(environmentDetector));
        }

        public DiagnosticReport BuildReport()
        {
            var warnings = new List<string>();
            var debugInfo = new List<string>();

            var mods = new List<DetectedModInfo>();
            Append(mods, CollectSafe(_collectorFactory.CreateBepInExPluginCollector(), "BepInExPluginCollector", warnings));
            Append(mods, CollectSafe(_collectorFactory.CreateBaseModCollector(), "BaseModCollector", warnings));

            var patches = CollectSafe(_collectorFactory.CreateActivePatchCollector(), "ActivePatchCollector", warnings);
            var assemblies = CollectSafe(_collectorFactory.CreateAssemblyInfoCollector(), "AssemblyInfoCollector", warnings);
            var retargetHarmonyStatus = CollectRetargetHarmonyStatus(warnings);

            IList<ExpectedPatchInfo> expectedPatches;
            try
            {
                expectedPatches = _collectorFactory.CreateExpectedPatchSource().GetExpectedPatches(debugInfo);
            }
            catch (Exception ex)
            {
                warnings.Add("ExpectedPatchSource failed: " + ex.Message);
                expectedPatches = new List<ExpectedPatchInfo>();
            }

            var patchComparison = CompareExpectedWithActual(mods, expectedPatches, patches);
            CorrelatePatchesWithMods(mods, patches);

            var environmentInfo = DetectEnvironmentSafe(warnings);

            return new DiagnosticReport(
                mods,
                patches,
                assemblies,
                patchComparison,
                retargetHarmonyStatus,
                environmentInfo,
                warnings,
                debugInfo,
                DateTime.UtcNow);
        }

        private RetargetHarmonyStatus CollectRetargetHarmonyStatus(IList<string> warnings)
        {
            try
            {
                return _collectorFactory.CreateRetargetHarmonyDetector().Collect();
            }
            catch (Exception ex)
            {
                warnings.Add("RetargetHarmonyDetector failed: " + ex.Message);

                return new RetargetHarmonyStatus(false, false, false, "Detection failed: " + ex.Message);
            }
        }

        private EnvironmentInfo DetectEnvironmentSafe(IList<string> warnings)
        {
            try
            {
                return _environmentDetector.DetectEnvironment();
            }
            catch (Exception ex)
            {
                warnings.Add("EnvironmentDetector failed: " + ex.Message);

                return new EnvironmentInfo(false, false, false);
            }
        }

        private static PatchComparisonResult CompareExpectedWithActual(
            IList<DetectedModInfo> mods,
            IList<ExpectedPatchInfo> expectedPatches,
            IList<PatchInfo> actualPatches)
        {
            var missingPatches = new List<MissingPatchInfo>();
            var totalExpected = 0;
            var totalMatched = 0;

            if (mods == null || expectedPatches == null || actualPatches == null)
            {
                return new PatchComparisonResult(missingPatches, totalExpected, totalMatched);
            }

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

            foreach (var mod in mods)
            {
                if (mod == null || string.IsNullOrEmpty(mod.AssemblyName))
                {
                    continue;
                }

                if (!expectedByAssembly.TryGetValue(mod.AssemblyName, out var expectedForMod))
                {
                    continue;
                }

                mod.SetExpectedPatchCount(expectedForMod.Count);
                totalExpected += expectedForMod.Count;

                foreach (var expected in expectedForMod)
                {
                    var isMissing = true;

                    if (actualByAssembly.TryGetValue(mod.AssemblyName, out var actualForMod))
                    {
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
                        missingPatches.Add(new MissingPatchInfo(
                            expected.PatchAssembly,
                            expected.TargetType,
                            expected.TargetMethod,
                            expected.PatchMethod,
                            expected.PatchType));
                    }
                    else
                    {
                        totalMatched++;
                    }
                }
            }

            return new PatchComparisonResult(missingPatches, totalExpected, totalMatched);
        }

        private static bool Matches(ExpectedPatchInfo expected, PatchInfo actual)
        {
            var typeMatches = expected.TargetType == actual.TargetType ||
                              actual.TargetType.EndsWith("." + expected.TargetType, StringComparison.Ordinal);

            return expected.PatchAssembly == actual.PatchAssemblyName &&
                   typeMatches &&
                   expected.TargetMethod == actual.TargetMethod &&
                   expected.PatchType == actual.PatchType;
        }

        private static void CorrelatePatchesWithMods(IList<DetectedModInfo> mods, IList<PatchInfo> patches)
        {
            if (mods == null || patches == null)
            {
                return;
            }

            var assemblyNameToMod = new Dictionary<string, DetectedModInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var mod in mods)
            {
                if (mod != null && !string.IsNullOrEmpty(mod.AssemblyName))
                {
                    assemblyNameToMod[mod.AssemblyName] = mod;
                    mod.SetPatchInfo(false, 0);
                }
            }

            foreach (var patch in patches)
            {
                if (patch == null || string.IsNullOrEmpty(patch.PatchAssemblyName))
                {
                    continue;
                }

                if (assemblyNameToMod.TryGetValue(patch.PatchAssemblyName, out var mod))
                {
                    mod.SetPatchInfo(true, mod.ActivePatchCount + 1);
                }
            }
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
    }
}
