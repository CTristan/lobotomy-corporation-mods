// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.DebugPanel.Models;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public interface ICollectorFactory
    {
        IActivePatchCollector CreateActivePatchCollector();

        IInfoCollector<IList<DetectedModInfo>> CreateBaseModCollector();

        IInfoCollector<IList<DetectedModInfo>> CreateBepInExPluginCollector();

        IInfoCollector<IList<AssemblyInfo>> CreateAssemblyInfoCollector();

        IInfoCollector<RetargetHarmonyStatus> CreateRetargetHarmonyDetector();

        IExpectedPatchSource CreateExpectedPatchSource();
    }
}
