// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.Common.Models.Diagnostics
{
    public sealed class DiagnosticReport
    {
        public DiagnosticReport(
            IList<DetectedModInfo> mods,
            IList<PatchInfo> patches,
            IList<AssemblyInfo> assemblies,
            PatchComparisonResult patchComparison,
            RetargetHarmonyStatus retargetHarmonyStatus,
            EnvironmentInfo environmentInfo,
            DllIntegrityReport dllIntegrity,
            IList<string> warnings,
            IList<string> debugInfo,
            DateTime collectedAt)
        {
            ThrowHelper.ThrowIfNull(mods);
            Mods = mods;
            ThrowHelper.ThrowIfNull(patches);
            Patches = patches;
            ThrowHelper.ThrowIfNull(assemblies);
            Assemblies = assemblies;
            ThrowHelper.ThrowIfNull(patchComparison);
            PatchComparison = patchComparison;
            ThrowHelper.ThrowIfNull(retargetHarmonyStatus);
            RetargetHarmonyStatus = retargetHarmonyStatus;
            ThrowHelper.ThrowIfNull(environmentInfo);
            EnvironmentInfo = environmentInfo;
            ThrowHelper.ThrowIfNull(dllIntegrity);
            DllIntegrity = dllIntegrity;
            ThrowHelper.ThrowIfNull(warnings);
            Warnings = warnings;
            ThrowHelper.ThrowIfNull(debugInfo);
            DebugInfo = debugInfo;
            CollectedAt = collectedAt;
        }

        public IList<DetectedModInfo> Mods { get; private set; }

        public IList<PatchInfo> Patches { get; private set; }

        public IList<AssemblyInfo> Assemblies { get; private set; }

        public PatchComparisonResult PatchComparison { get; private set; }

        public RetargetHarmonyStatus RetargetHarmonyStatus { get; private set; }

        public EnvironmentInfo EnvironmentInfo { get; private set; }

        public DllIntegrityReport DllIntegrity { get; private set; }

        public IList<string> Warnings { get; private set; }

        public IList<string> DebugInfo { get; private set; }

        public DateTime CollectedAt { get; private set; }
    }
}
