// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using Hemocode.Common.Implementations;
using Hemocode.Playwright.JsonModels;
using Hemocode.Playwright.JsonModels.Diagnostics;

#endregion

namespace Hemocode.Playwright.Queries
{
    public static class DiagnosticsQueries
    {
        public static Response HandleQuery(string requestId, Dictionary<string, object> parameters)
        {
            ThrowHelper.ThrowIfNull(parameters);

            if (!DiagnosticDataRegistry.IsRegistered)
            {
                return Response.CreateError(
                    requestId,
                    "DebugPanel mod is not loaded. Install LobotomyCorporationMods.DebugPanel to use diagnostics queries.",
                    "DIAGNOSTICS_NOT_AVAILABLE"
                );
            }

            var provider = DiagnosticDataRegistry.Provider;
            var section = parameters.ContainsKey("section")
                ? parameters["section"].ToString().ToLowerInvariant()
                : "full";

            switch (section)
            {
                case "full":
                    var report = provider.GetFullReport();
                    return Response.CreateSuccess(requestId, DiagnosticReportData.FromModel(report));
                case "mods":
                    var mods = provider.GetMods();
                    return Response.CreateSuccess(requestId, DetectedModData.FromModels(mods));
                case "patches":
                    var patches = provider.GetPatches();
                    return Response.CreateSuccess(requestId, PatchInfoData.FromModels(patches));
                case "assemblies":
                    var assemblies = provider.GetAssemblies();
                    return Response.CreateSuccess(requestId, AssemblyInfoData.FromModels(assemblies));
                case "patch-comparison":
                    var comparison = provider.GetPatchComparison();
                    return Response.CreateSuccess(requestId, PatchComparisonData.FromModel(comparison));
                case "retarget":
                    var retarget = provider.GetRetargetHarmonyStatus();
                    return Response.CreateSuccess(requestId, RetargetHarmonyData.FromModel(retarget));
                case "environment":
                    var env = provider.GetEnvironmentInfo();
                    return Response.CreateSuccess(requestId, EnvironmentInfoData.FromModel(env));
                case "dll-integrity":
                    var integrity = provider.GetDllIntegrity();
                    return Response.CreateSuccess(requestId, DllIntegrityReportData.FromModel(integrity));
                case "external-logs":
                    var logs = provider.GetExternalLogs();
                    return Response.CreateSuccess(requestId, ExternalLogDataModel.FromModel(logs));
                default:
                    return Response.CreateError(
                        requestId,
                        $"Unknown diagnostics section: {section}. Valid sections: full, mods, patches, assemblies, patch-comparison, retarget, environment, dll-integrity, external-logs",
                        "UNKNOWN_SECTION"
                    );
            }
        }
    }
}
