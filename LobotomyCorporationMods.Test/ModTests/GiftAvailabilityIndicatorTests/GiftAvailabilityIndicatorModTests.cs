// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.GiftAvailabilityIndicator;
using LobotomyCorporationMods.Test.Extensions;

namespace LobotomyCorporationMods.Test.ModTests.GiftAvailabilityIndicatorTests
{
    public class GiftAvailabilityIndicatorModTests
    {
        protected GiftAvailabilityIndicatorModTests()
        {
            _ = new Harmony_Patch();
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);
        }
    }
}
