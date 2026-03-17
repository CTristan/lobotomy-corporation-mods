// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Models.Diagnostics;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public interface ICollectorFactory
    {
        IActivePatchCollector CreateActivePatchCollector(IList<string> debugInfo);

        IInfoCollector<IList<DetectedModInfo>> CreateBaseModCollector(IList<string> debugInfo);

        IInfoCollector<IList<DetectedModInfo>> CreateBepInExPluginCollector();

        IInfoCollector<IList<AssemblyInfo>> CreateAssemblyInfoCollector();

        IInfoCollector<RetargetHarmonyStatus> CreateRetargetHarmonyDetector();

        IExpectedPatchSource CreateExpectedPatchSource(IList<string> debugInfo);

        IInfoCollector<DllIntegrityReport> CreateDllIntegrityCollector();

        IInfoCollector<FilesystemValidationReport> CreateFilesystemValidationCollector();

        IInfoCollector<ErrorLogReport> CreateErrorLogCollector();

        IInfoCollector<KnownIssuesReport> CreateKnownIssuesChecker(IList<DetectedModInfo> mods, IList<AssemblyInfo> assemblies);

        IInfoCollector<DependencyReport> CreateDependencyChecker(IList<DetectedModInfo> mods, IList<AssemblyInfo> assemblies);

        IInfoCollector<ExternalLogData> CreateExternalLogCollector();
    }
}
