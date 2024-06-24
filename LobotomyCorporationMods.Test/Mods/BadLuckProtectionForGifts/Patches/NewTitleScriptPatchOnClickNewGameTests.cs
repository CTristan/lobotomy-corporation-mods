// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Patches;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.BadLuckProtectionForGifts.Patches
{
    public sealed class NewTitleScriptPatchOnClickNewGameTests : BadLuckProtectionForGiftsTests
    {
        [Fact]
        public void Starting_a_new_game_resets_the_tracker()
        {
            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();

            NewTitleScriptPatchOnClickNewGame.PatchAfterOnClickNewGame(mockAgentWorkTracker.Object);

            mockAgentWorkTracker.Verify(tracker => tracker.Reset(), Times.Once);
        }
    }
}
