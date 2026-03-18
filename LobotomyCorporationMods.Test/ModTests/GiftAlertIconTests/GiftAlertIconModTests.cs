// SPDX-License-Identifier: MIT

using Hemocode.GiftAlertIcon;
using LobotomyCorporationMods.Test.Extensions;

namespace LobotomyCorporationMods.Test.ModTests.GiftAlertIconTests
{
    public class GiftAlertIconModTests
    {
        protected GiftAlertIconModTests()
        {
            _ = new Harmony_Patch();
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);
        }
    }
}
