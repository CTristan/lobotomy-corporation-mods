// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.DebugPanel;
using LobotomyCorporationMods.Test.Extensions;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public class DebugPanelModTests
    {
        protected DebugPanelModTests()
        {
            _ = new Harmony_Patch();
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);
        }
    }
}
