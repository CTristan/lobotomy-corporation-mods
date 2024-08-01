// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.GiftAlertIcon;
using LobotomyCorporationMods.Test.Extensions;

#endregion

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
