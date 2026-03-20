// SPDX-License-Identifier: MIT

#region

using DebugPanel;
using DebugPanel.Common.Interfaces;
using Moq;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public class DebugPanelModTests
    {
        protected DebugPanelModTests()
        {
            _ = new Harmony_Patch();
            var mockLogger = new Mock<ILogger>();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);
        }
    }
}
