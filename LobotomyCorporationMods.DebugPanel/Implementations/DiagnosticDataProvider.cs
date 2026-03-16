// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Models.Diagnostics;
using LobotomyCorporationMods.DebugPanel.Interfaces;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Implementations
{
    public sealed class DiagnosticDataProvider : IDiagnosticDataProvider
    {
        private readonly IDiagnosticReportBuilder _reportBuilder;
        private readonly IExternalLogSource _externalLogSource;
        private DiagnosticReport _cachedReport;

        public DiagnosticDataProvider(IDiagnosticReportBuilder reportBuilder, IExternalLogSource externalLogSource)
        {
            ThrowHelper.ThrowIfNull(reportBuilder);
            _reportBuilder = reportBuilder;
            ThrowHelper.ThrowIfNull(externalLogSource);
            _externalLogSource = externalLogSource;
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
            return _externalLogSource.GetExternalLogs();
        }

        private DiagnosticReport GetOrBuildReport()
        {
            return _cachedReport ?? (_cachedReport = _reportBuilder.BuildReport());
        }
    }
}
