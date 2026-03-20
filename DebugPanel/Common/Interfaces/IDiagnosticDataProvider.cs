// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using DebugPanel.Common.Models.Diagnostics;

#endregion

namespace DebugPanel.Common.Interfaces
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
