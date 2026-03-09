// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Patches;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.BadLuckProtectionForGiftsTests.PatchTests
{
    public sealed class NewTitleScriptPatchOnClickNewGameTests : BadLuckProtectionForGiftsModTests
    {
        [Fact]
        public void Starting_a_new_game_resets_the_tracker()
        {
            Mock<IAgentWorkTracker> mockAgentWorkTracker = new();

            NewTitleScriptPatchOnClickNewGame.PatchAfterOnClickNewGame(mockAgentWorkTracker.Object);

            mockAgentWorkTracker.Verify(tracker => tracker.Reset(), Times.Once);
        }
    }
}
