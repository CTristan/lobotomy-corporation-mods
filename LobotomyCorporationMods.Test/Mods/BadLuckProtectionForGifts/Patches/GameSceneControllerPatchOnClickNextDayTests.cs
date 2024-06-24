// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Patches;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.BadLuckProtectionForGifts.Patches
{
    public sealed class GameSceneControllerPatchOnClickNextDayTests : BadLuckProtectionForGiftsTests
    {
        [Fact]
        public void The_tracker_data_is_saved_when_going_to_the_next_day()
        {
            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();

            GameSceneControllerPatchOnClickNextDay.PatchAfterOnClickNextDay(mockAgentWorkTracker.Object);

            mockAgentWorkTracker.Verify(tracker => tracker.Save(), Times.Once);
        }
    }
}
