// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Implementations.Effects;
using LobotomyCorporationMods.DontChatMe.Models;
using LobotomyCorporationMods.Test.ModTests.DontChatMeTests.Fakes;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.EffectTests
{
    /// <summary>
    ///     Every effect must reject with <c>game_not_ready</c> when the game isn't in PLAYING state.
    ///     One test per effect, written explicitly so the branch is exercised in coverage.
    /// </summary>
    public sealed class GameNotReadyTests : DontChatMeModTests
    {
        private static EffectDispatch Dispatch =>
            new EffectDispatch(
                redemptionId: "r",
                effectSlug: "any",
                effectName: null,
                userId: null,
                userDisplayName: null,
                gameId: 0,
                dispatchedAt: null,
                attempts: 1,
                replay: false
            );

        private static FakeGameAdapter NotReady() => new FakeGameAdapter { IsGameReady = false };

        [Fact]
        public void KillRandomAgent_rejects_when_game_is_not_ready()
        {
            new KillRandomAgentEffect(NotReady())
                .Execute(Dispatch)
                .Should()
                .Be(StandardErrors.GameStateBlocked);
        }

        [Fact]
        public void RandomAgentPanic_rejects_when_game_is_not_ready()
        {
            new RandomAgentPanicEffect(NotReady())
                .Execute(Dispatch)
                .Should()
                .Be(StandardErrors.GameStateBlocked);
        }

        [Fact]
        public void RemoveEnergy_rejects_when_game_is_not_ready()
        {
            new RemoveEnergyEffect(NotReady(), new FakeConfig())
                .Execute(Dispatch)
                .Should()
                .Be(StandardErrors.GameStateBlocked);
        }

        [Fact]
        public void SetGameSpeed_rejects_when_game_is_not_ready()
        {
            new SetGameSpeedEffect(NotReady())
                .Execute(Dispatch)
                .Should()
                .Be(StandardErrors.GameStateBlocked);
        }

        [Fact]
        public void ShowSystemMessage_rejects_when_game_is_not_ready()
        {
            new ShowSystemMessageEffect(NotReady())
                .Execute(Dispatch)
                .Should()
                .Be(StandardErrors.GameStateBlocked);
        }

        [Fact]
        public void EscapeRandomCreature_rejects_when_game_is_not_ready()
        {
            new EscapeRandomCreatureEffect(NotReady())
                .Execute(Dispatch)
                .Should()
                .Be(StandardErrors.GameStateBlocked);
        }
    }
}
