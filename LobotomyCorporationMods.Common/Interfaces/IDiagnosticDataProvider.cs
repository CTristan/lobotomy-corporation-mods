// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Models.Diagnostics;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces
{
    public interface IDiagnosticDataProvider
    {
        DiagnosticReport GetFullReport();

        IList<DetectedModInfo> GetMods();

        IList<PatchInfo> GetPatches();

        IList<AssemblyInfo> GetAssemblies();

        PatchComparisonResult GetPatchComparison();

        RetargetHarmonyStatus GetRetargetHarmonyStatus();

        EnvironmentInfo GetEnvironmentInfo();

        DllIntegrityReport GetDllIntegrity();

        ExternalLogData GetExternalLogs();
    }
}
