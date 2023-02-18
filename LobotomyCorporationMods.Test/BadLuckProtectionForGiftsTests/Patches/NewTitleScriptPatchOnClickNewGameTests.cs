// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.BadLuckProtectionForGifts.Patches;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.BadLuckProtectionForGiftsTests.Patches
{
    public sealed class NewTitleScriptPatchOnClickNewGameTests : BadLuckProtectionForGiftsTests
    {
        [Fact]
        public void Starting_a_new_game_resets_the_tracker()
        {
            var mockAgentWorkTracker = CreateMockAgentWorkTracker();

            NewTitleScriptPatchOnClickNewGame.Postfix();

            mockAgentWorkTracker.Verify(static tracker => tracker.Reset(), Times.Once);
        }
    }
}
