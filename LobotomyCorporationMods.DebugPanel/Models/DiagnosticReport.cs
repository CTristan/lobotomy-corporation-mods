// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Models
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
            IList<string> warnings,
            IList<string> debugInfo,
            DateTime collectedAt)
        {
            Mods = Guard.Against.Null(mods, nameof(mods));
            Patches = Guard.Against.Null(patches, nameof(patches));
            Assemblies = Guard.Against.Null(assemblies, nameof(assemblies));
            PatchComparison = Guard.Against.Null(patchComparison, nameof(patchComparison));
            RetargetHarmonyStatus = Guard.Against.Null(retargetHarmonyStatus, nameof(retargetHarmonyStatus));
            EnvironmentInfo = Guard.Against.Null(environmentInfo, nameof(environmentInfo));
            Warnings = Guard.Against.Null(warnings, nameof(warnings));
            DebugInfo = Guard.Against.Null(debugInfo, nameof(debugInfo));
            CollectedAt = collectedAt;
        }

        public IList<DetectedModInfo> Mods { get; private set; }

        public IList<PatchInfo> Patches { get; private set; }

        public IList<AssemblyInfo> Assemblies { get; private set; }

        public PatchComparisonResult PatchComparison { get; private set; }

        public RetargetHarmonyStatus RetargetHarmonyStatus { get; private set; }

        public EnvironmentInfo EnvironmentInfo { get; private set; }

        public IList<string> Warnings { get; private set; }

        public IList<string> DebugInfo { get; private set; }

        public DateTime CollectedAt { get; private set; }
    }
}
