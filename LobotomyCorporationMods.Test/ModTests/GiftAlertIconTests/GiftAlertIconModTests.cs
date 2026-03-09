// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.GiftAlertIcon;
using LobotomyCorporationMods.Test.Extensions;
using Moq;

namespace LobotomyCorporationMods.Test.ModTests.GiftAlertIconTests
{
    internal class GiftAlertIconModTests
    {
        protected GiftAlertIconModTests()
        {
            _ = new Harmony_Patch();
            Mock<ILogger> mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);
        }
    }
}
