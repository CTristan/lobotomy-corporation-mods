// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Patches;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.BadLuckProtectionForGifts.Patches
{
    public sealed class GameSceneControllerPatchOnStageStartTests : BadLuckProtectionForGiftsTests
    {
        [Fact]
        public void Restarting_the_day_reloads_the_saved_data_and_overwrites_the_progress_made_that_day()
        {
            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();

            GameSceneControllerPatchOnStageStart.PatchAfterOnStageStart(mockAgentWorkTracker.Object);

            mockAgentWorkTracker.Verify(static tracker => tracker.Load(), Times.Once);
        }
    }
}
