// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
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

        public DiagnosticReportBuilder()
            : this(
                new BepInExPluginCollector(),
                new BaseModCollector(),
                new ActivePatchCollector(),
                new AssemblyInfoCollector(),
                new RetargetHarmonyDetector())
        {
        }

        public DiagnosticReportBuilder(
            IInfoCollector<IList<ModInfo>> bepInExPluginCollector,
            IInfoCollector<IList<ModInfo>> baseModCollector,
            IInfoCollector<IList<PatchInfo>> activePatchCollector,
            IInfoCollector<IList<AssemblyInfo>> assemblyInfoCollector,
            IInfoCollector<RetargetHarmonyStatus> retargetHarmonyDetector)
        {
            _bepInExPluginCollector = bepInExPluginCollector ?? throw new ArgumentNullException(nameof(bepInExPluginCollector));
            _baseModCollector = baseModCollector ?? throw new ArgumentNullException(nameof(baseModCollector));
            _activePatchCollector = activePatchCollector ?? throw new ArgumentNullException(nameof(activePatchCollector));
            _assemblyInfoCollector = assemblyInfoCollector ?? throw new ArgumentNullException(nameof(assemblyInfoCollector));
            _retargetHarmonyDetector = retargetHarmonyDetector ?? throw new ArgumentNullException(nameof(retargetHarmonyDetector));
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

            return report;
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
#pragma warning restore CA1031
    }
}
