// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.BadLuckProtectionForGifts.Patches;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.BadLuckProtectionForGiftsTests.Patches
{
    public sealed class GameSceneControllerPatchOnStageStartTests : BadLuckProtectionForGiftsTests
    {
        [Fact]
        public void Restarting_the_day_reloads_the_saved_data_and_overwrites_the_progress_made_that_day()
        {
            var mockAgentWorkTracker = CreateMockAgentWorkTracker();

            GameSceneControllerPatchOnStageStart.Postfix();

            mockAgentWorkTracker.Verify(static tracker => tracker.Load(), Times.Once);
        }
    }
}
