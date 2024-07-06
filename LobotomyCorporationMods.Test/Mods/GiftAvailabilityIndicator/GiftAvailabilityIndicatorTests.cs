// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.GiftAvailabilityIndicator;
using LobotomyCorporationMods.Test.Extensions;

namespace LobotomyCorporationMods.Test.Mods.GiftAvailabilityIndicator
{
    public class GiftAvailabilityIndicatorTests
    {
        protected GiftAvailabilityIndicatorTests()
        {
            _ = new Harmony_Patch();
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);
        }
    }
}
