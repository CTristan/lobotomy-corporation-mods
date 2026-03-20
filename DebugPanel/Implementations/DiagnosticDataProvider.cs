// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using DebugPanel.Common.Implementations;
using DebugPanel.Common.Interfaces;
using DebugPanel.Common.Models.Diagnostics;
using DebugPanel.Interfaces;

#endregion

namespace DebugPanel.Implementations
{
    public sealed class DiagnosticDataProvider : IDiagnosticDataProvider
    {
        private readonly IDiagnosticReportBuilder _reportBuilder;
        private readonly IInfoCollector<ExternalLogData> _externalLogCollector;
        private DiagnosticReport _cachedReport;

        public DiagnosticDataProvider(IDiagnosticReportBuilder reportBuilder, IInfoCollector<ExternalLogData> externalLogCollector)
        {
            ThrowHelper.ThrowIfNull(reportBuilder);
            _reportBuilder = reportBuilder;
            ThrowHelper.ThrowIfNull(externalLogCollector);
            _externalLogCollector = externalLogCollector;
        }

        public DiagnosticReport GetFullReport()
        {
            _cachedReport = _reportBuilder.BuildReport();

            return _cachedReport;
        }

        public IList<DetectedModInfo> GetMods()
        {
            return GetOrBuildReport().Mods;
        }

        public IList<PatchInfo> GetPatches()
        {
            return GetOrBuildReport().Patches;
        }

        public IList<AssemblyInfo> GetAssemblies()
        {
            return GetOrBuildReport().Assemblies;
        }

        public PatchComparisonResult GetPatchComparison()
        {
            return GetOrBuildReport().PatchComparison;
        }

        public RetargetHarmonyStatus GetRetargetHarmonyStatus()
        {
            return GetOrBuildReport().RetargetHarmonyStatus;
        }

        public EnvironmentInfo GetEnvironmentInfo()
        {
            return GetOrBuildReport().EnvironmentInfo;
        }

        public DllIntegrityReport GetDllIntegrity()
        {
            return GetOrBuildReport().DllIntegrity;
        }

        public ExternalLogData GetExternalLogs()
        {
            return _externalLogCollector.Collect();
        }

        private DiagnosticReport GetOrBuildReport()
        {
            return _cachedReport ?? (_cachedReport = _reportBuilder.BuildReport());
        }
    }
}
