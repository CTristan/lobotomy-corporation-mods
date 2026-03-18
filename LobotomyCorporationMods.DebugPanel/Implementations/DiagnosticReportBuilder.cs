// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Reflection;
using Hemocode.Common.Enums.Diagnostics;
using Hemocode.Common.Implementations;
using Hemocode.DebugPanel.Interfaces;
using Hemocode.Common.Models.Diagnostics;

#endregion

namespace Hemocode.DebugPanel.Implementations
{
    public sealed class DiagnosticReportBuilder : IDiagnosticReportBuilder
    {
        private static readonly HashSet<string> s_frameworkAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "mscorlib",
            "System",
            "System.Core",
            "Microsoft.Xna.Framework",
            "Microsoft.Xna.Framework.Game",
            "UnityEditor",
            "UnityEngine",
            "UnityEngine.CoreModule",
            "UnityEngine.UI",
            "UnityEngine.Networking",
            "0Harmony",
            "0Harmony12",
            "0Harmony109",
            "BepInEx",
            "BepInEx.Core",
            "BepInEx.Preloader",
            "LobotomyBaseModLib",
            "BepInEx.ConfigurationManager",
            "Assembly-CSharp",
        };

        private readonly ICollectorFactory _collectorFactory;
        private readonly IEnvironmentDetector _environmentDetector;
        private readonly IHarmonyVersionClassifier _harmonyVersionClassifier;

        public DiagnosticReportBuilder(ICollectorFactory collectorFactory, IEnvironmentDetector environmentDetector, IHarmonyVersionClassifier harmonyVersionClassifier)
        {
            ThrowHelper.ThrowIfNull(collectorFactory);
            _collectorFactory = collectorFactory;
            ThrowHelper.ThrowIfNull(environmentDetector);
            _environmentDetector = environmentDetector;
            ThrowHelper.ThrowIfNull(harmonyVersionClassifier);
            _harmonyVersionClassifier = harmonyVersionClassifier;
        }

        public DiagnosticReport BuildReport()
        {
            var warnings = new List<string>();
            var debugInfo = new List<string>();

            var mods = new List<DetectedModInfo>();
            Append(mods, CollectSafe(_collectorFactory.CreateBepInExPluginCollector(), "BepInExPluginCollector", warnings));
            Append(mods, CollectSafe(_collectorFactory.CreateBaseModCollector(debugInfo), "BaseModCollector", warnings));

            var patches = CollectSafe(_collectorFactory.CreateActivePatchCollector(debugInfo), "ActivePatchCollector", warnings);
            Append(debugInfo, Harmony2PatchInspectionSource.LastDiagnostics);
            Append(debugInfo, Harmony1PatchInspectionSource.LastDiagnostics);
            var assemblies = CollectSafe(_collectorFactory.CreateAssemblyInfoCollector(), "AssemblyInfoCollector", warnings);
            var retargetHarmonyStatus = CollectRetargetHarmonyStatus(warnings);

            IList<ExpectedPatchInfo> expectedPatches;
            try
            {
                expectedPatches = _collectorFactory.CreateExpectedPatchSource(debugInfo).GetExpectedPatches(debugInfo);
            }
            catch (Exception ex)
            {
                warnings.Add("ExpectedPatchSource failed: " + ex.Message);
                expectedPatches = new List<ExpectedPatchInfo>();
            }

            var modsSynthesized = false;
            if (!HasLmmMods(mods) && expectedPatches.Count > 0)
            {
                var synthesized = SynthesizeModsFromExpectedPatches(expectedPatches, assemblies);
                Append(mods, synthesized);
                modsSynthesized = synthesized.Count > 0;

                if (modsSynthesized && patches.Count == 0)
                {
                    Append(patches, SynthesizePatchesFromExpected(expectedPatches));
                }
            }

            if (!modsSynthesized)
            {
                CorrelatePatchesWithMods(mods, patches);
            }

            var comparisonExpectedPatches = modsSynthesized ? new List<ExpectedPatchInfo>() : expectedPatches;
            var patchComparison = CompareExpectedWithActual(mods, comparisonExpectedPatches, patches);

            var dllIntegrity = CollectDllIntegritySafe(warnings);
            var environmentInfo = DetectEnvironmentSafe(warnings);
            var filesystemValidation = CollectFilesystemValidationSafe(warnings);
            var errorLogReport = CollectErrorLogReportSafe(warnings);
            var knownIssuesReport = CollectKnownIssuesSafe(mods, assemblies, warnings);
            var dependencyReport = CollectDependencyReportSafe(mods, assemblies, warnings);
            var gameplayLogErrorReport = CollectGameplayLogErrorReportSafe(warnings);

            var report = new DiagnosticReport(
                mods,
                patches,
                assemblies,
                patchComparison,
                retargetHarmonyStatus,
                environmentInfo,
                dllIntegrity,
                warnings,
                debugInfo,
                DateTime.UtcNow,
                filesystemValidation,
                errorLogReport,
                knownIssuesReport,
                dependencyReport,
                gameplayLogErrorReport: gameplayLogErrorReport);

            var aggregatedIssues = AggregateIssuesSafe(report, warnings);

            return new DiagnosticReport(
                report.Mods,
                report.Patches,
                report.Assemblies,
                report.PatchComparison,
                report.RetargetHarmonyStatus,
                report.EnvironmentInfo,
                report.DllIntegrity,
                report.Warnings,
                report.DebugInfo,
                report.CollectedAt,
                report.FilesystemValidation,
                report.ErrorLogReport,
                report.KnownIssuesReport,
                report.DependencyReport,
                aggregatedIssues,
                report.GameplayLogErrorReport);
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

        private DllIntegrityReport CollectDllIntegritySafe(IList<string> warnings)
        {
            try
            {
                return _collectorFactory.CreateDllIntegrityCollector().Collect();
            }
            catch (Exception ex)
            {
                warnings.Add("DllIntegrityCollector failed: " + ex.Message);

                return new DllIntegrityReport(
                    new List<DllIntegrityFinding>(),
                    false,
                    string.Empty,
                    false,
                    string.Empty,
                    -1,
                    false,
                    0,
                    new List<string>(),
                    "Collection failed: " + ex.Message);
            }
        }

        private KnownIssuesReport CollectKnownIssuesSafe(IList<DetectedModInfo> mods, IList<AssemblyInfo> assemblies, IList<string> warnings)
        {
            try
            {
                return _collectorFactory.CreateKnownIssuesChecker(mods, assemblies).Collect();
            }
            catch (Exception ex)
            {
                warnings.Add("KnownIssuesChecker failed: " + ex.Message);

                return new KnownIssuesReport(new List<KnownIssueMatch>(), string.Empty);
            }
        }

        private DependencyReport CollectDependencyReportSafe(IList<DetectedModInfo> mods, IList<AssemblyInfo> assemblies, IList<string> warnings)
        {
            try
            {
                return _collectorFactory.CreateDependencyChecker(mods, assemblies).Collect();
            }
            catch (Exception ex)
            {
                warnings.Add("DependencyChecker failed: " + ex.Message);

                return new DependencyReport(new List<DiagnosticIssue>(), string.Empty, false);
            }
        }

        private static IList<DiagnosticIssue> AggregateIssuesSafe(DiagnosticReport report, IList<string> warnings)
        {
            try
            {
                var aggregator = new IssuesAggregator();

                return aggregator.AggregateIssues(report);
            }
            catch (Exception ex)
            {
                warnings.Add("IssuesAggregator failed: " + ex.Message);

                return new List<DiagnosticIssue>();
            }
        }

        private FilesystemValidationReport CollectFilesystemValidationSafe(IList<string> warnings)
        {
            try
            {
                return _collectorFactory.CreateFilesystemValidationCollector().Collect();
            }
            catch (Exception ex)
            {
                warnings.Add("FilesystemValidationCollector failed: " + ex.Message);

                return new FilesystemValidationReport(new List<DiagnosticIssue>(), "Collection failed: " + ex.Message);
            }
        }

        private ErrorLogReport CollectErrorLogReportSafe(IList<string> warnings)
        {
            try
            {
                return _collectorFactory.CreateErrorLogCollector().Collect();
            }
            catch (Exception ex)
            {
                warnings.Add("ErrorLogCollector failed: " + ex.Message);

                return new ErrorLogReport(new List<ErrorLogEntry>());
            }
        }

        private GameplayLogErrorReport CollectGameplayLogErrorReportSafe(IList<string> warnings)
        {
            try
            {
                return _collectorFactory.CreateGameplayLogErrorCollector().Collect();
            }
            catch (Exception ex)
            {
                warnings.Add("GameplayLogErrorCollector failed: " + ex.Message);

                return new GameplayLogErrorReport(new List<GameplayLogErrorEntry>());
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

        public static bool HasLmmMods(IList<DetectedModInfo> mods)
        {
            if (mods == null)
            {
                return false;
            }

            foreach (var mod in mods)
            {
                if (mod != null && mod.Source == ModSource.Lmm)
                {
                    return true;
                }
            }

            return false;
        }

        public static IList<PatchInfo> SynthesizePatchesFromExpected(IList<ExpectedPatchInfo> expectedPatches)
        {
            ThrowHelper.ThrowIfNull(expectedPatches);

            var patches = new List<PatchInfo>();
            foreach (var expected in expectedPatches)
            {
                if (expected == null)
                {
                    continue;
                }

                patches.Add(new PatchInfo(
                    expected.TargetType,
                    expected.TargetMethod,
                    expected.PatchType,
                    expected.PatchAssembly,
                    expected.PatchMethod,
                    expected.PatchAssembly));
            }

            return patches;
        }

        public IList<DetectedModInfo> SynthesizeModsFromExpectedPatches(
            IList<ExpectedPatchInfo> expectedPatches,
            IList<AssemblyInfo> assemblies)
        {
            ThrowHelper.ThrowIfNull(expectedPatches);

            var patchesByAssembly = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var patch in expectedPatches)
            {
                if (patch == null || string.IsNullOrEmpty(patch.PatchAssembly))
                {
                    continue;
                }

                if (ShouldSkipAssembly(patch.PatchAssembly))
                {
                    continue;
                }

                if (!patchesByAssembly.ContainsKey(patch.PatchAssembly))
                {
                    patchesByAssembly[patch.PatchAssembly] = 0;
                }

                patchesByAssembly[patch.PatchAssembly]++;
            }

            var versionLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var referenceLookup = new Dictionary<string, IList<AssemblyName>>(StringComparer.OrdinalIgnoreCase);
            if (assemblies != null)
            {
                foreach (var asm in assemblies)
                {
                    if (asm != null && !string.IsNullOrEmpty(asm.Name))
                    {
                        versionLookup[asm.Name] = asm.Version ?? string.Empty;
                        referenceLookup[asm.Name] = asm.References;
                    }
                }
            }

            var mods = new List<DetectedModInfo>();
            foreach (var kvp in patchesByAssembly)
            {
                var assemblyName = kvp.Key;
                var patchCount = kvp.Value;
                var version = versionLookup.TryGetValue(assemblyName, out var v) ? v : string.Empty;
                var references = referenceLookup.TryGetValue(assemblyName, out var refs) ? refs : new List<AssemblyName>();
                var harmonyVersion = _harmonyVersionClassifier.Classify(references);

                mods.Add(new DetectedModInfo(
                    assemblyName,
                    version,
                    ModSource.Lmm,
                    harmonyVersion,
                    assemblyName,
                    string.Empty,
                    true,
                    patchCount,
                    patchCount));
            }

            return mods;
        }

        private static bool ShouldSkipAssembly(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
            {
                return true;
            }

            if (s_frameworkAssemblies.Contains(assemblyName))
            {
                return true;
            }

            return assemblyName.StartsWith("UnityEngine.", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("System.", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("Mono.", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("MonoMod.", StringComparison.OrdinalIgnoreCase);
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
