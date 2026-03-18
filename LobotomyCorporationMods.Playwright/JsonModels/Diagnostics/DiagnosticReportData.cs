// SPDX-License-Identifier: MIT

#region

using System;
using Hemocode.Common.Implementations;
using Hemocode.Common.Models.Diagnostics;

#endregion

namespace Hemocode.Playwright.JsonModels.Diagnostics
{
    [Serializable]
    public sealed class DiagnosticReportData
    {
        public DetectedModData[] mods;
        public PatchInfoData[] patches;
        public AssemblyInfoData[] assemblies;
        public PatchComparisonData patchComparison;
        public RetargetHarmonyData retargetHarmonyStatus;
        public EnvironmentInfoData environmentInfo;
        public DllIntegrityReportData dllIntegrity;
        public string[] warnings;
        public string[] debugInfo;
        public string collectedAt;

        public static DiagnosticReportData FromModel(DiagnosticReport model)
        {
            ThrowHelper.ThrowIfNull(model);

            var warningArr = new string[model.Warnings.Count];
            model.Warnings.CopyTo(warningArr, 0);

            var debugArr = new string[model.DebugInfo.Count];
            model.DebugInfo.CopyTo(debugArr, 0);

            return new DiagnosticReportData
            {
                mods = DetectedModData.FromModels(model.Mods),
                patches = PatchInfoData.FromModels(model.Patches),
                assemblies = AssemblyInfoData.FromModels(model.Assemblies),
                patchComparison = PatchComparisonData.FromModel(model.PatchComparison),
                retargetHarmonyStatus = RetargetHarmonyData.FromModel(model.RetargetHarmonyStatus),
                environmentInfo = EnvironmentInfoData.FromModel(model.EnvironmentInfo),
                dllIntegrity = DllIntegrityReportData.FromModel(model.DllIntegrity),
                warnings = warningArr,
                debugInfo = debugArr,
                collectedAt = model.CollectedAt.ToString("O"),
            };
        }
    }
}
