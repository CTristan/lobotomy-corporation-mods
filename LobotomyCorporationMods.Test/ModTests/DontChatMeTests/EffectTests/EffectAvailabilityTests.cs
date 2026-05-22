// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Implementations.Effects;
using LobotomyCorporationMods.Test.ModTests.DontChatMeTests.Fakes;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.EffectTests
{
    /// <summary>
    ///     Each effect reports whether it would fire right now via <c>IsAvailableNow</c>. The
    ///     dispatcher uses this to push <c>effect_state</c> deltas to the chat-side so its UI can
    ///     grey out effects that would currently fail. The danger-flag gate is applied by the probe,
    ///     not here — these tests cover only game-state preconditions.
    /// </summary>
    public sealed class EffectAvailabilityTests : DontChatMeModTests
    {
        // ----- not-ready: every effect reports game_not_ready -----

        [Fact]
        public void RandomMeltdown_is_unavailable_when_game_is_not_ready()
        {
            AssertUnavailable(
                new RandomMeltdownEffect(new FakeGameAdapter { IsGameReady = false }),
                ErrorTags.GameNotReady
            );
        }

        [Fact]
        public void KillRandomAgent_is_unavailable_when_game_is_not_ready()
        {
            AssertUnavailable(
                new KillRandomAgentEffect(
                    new FakeGameAdapter { IsGameReady = false, LivingAgentCount = 5 }
                ),
                ErrorTags.GameNotReady
            );
        }

        [Fact]
        public void RandomAgentPanic_is_unavailable_when_game_is_not_ready()
        {
            AssertUnavailable(
                new RandomAgentPanicEffect(
                    new FakeGameAdapter { IsGameReady = false, ControllableAgentCount = 5 }
                ),
                ErrorTags.GameNotReady
            );
        }

        [Fact]
        public void AddEnergy_is_unavailable_when_game_is_not_ready()
        {
            AssertUnavailable(
                new AddEnergyEffect(new FakeGameAdapter { IsGameReady = false }, new FakeConfig()),
                ErrorTags.GameNotReady
            );
        }

        [Fact]
        public void RemoveEnergy_is_unavailable_when_game_is_not_ready()
        {
            AssertUnavailable(
                new RemoveEnergyEffect(
                    new FakeGameAdapter { IsGameReady = false },
                    new FakeConfig()
                ),
                ErrorTags.GameNotReady
            );
        }

        [Fact]
        public void AddMoney_is_unavailable_when_game_is_not_ready()
        {
            AssertUnavailable(
                new AddMoneyEffect(new FakeGameAdapter { IsGameReady = false }, new FakeConfig()),
                ErrorTags.GameNotReady
            );
        }

        [Fact]
        public void ShowSystemMessage_is_unavailable_when_game_is_not_ready()
        {
            AssertUnavailable(
                new ShowSystemMessageEffect(new FakeGameAdapter { IsGameReady = false }),
                ErrorTags.GameNotReady
            );
        }

        [Fact]
        public void SetGameSpeed_is_unavailable_when_game_is_not_ready()
        {
            AssertUnavailable(
                new SetGameSpeedEffect(new FakeGameAdapter { IsGameReady = false }),
                ErrorTags.GameNotReady
            );
        }

        [Fact]
        public void EscapeRandomCreature_is_unavailable_when_game_is_not_ready()
        {
            AssertUnavailable(
                new EscapeRandomCreatureEffect(
                    new FakeGameAdapter { IsGameReady = false, CreatureCount = 3 }
                ),
                ErrorTags.GameNotReady
            );
        }

        // ----- per-effect: precondition-specific reasons -----

        [Fact]
        public void KillRandomAgent_is_unavailable_with_no_agents_when_facility_is_empty()
        {
            AssertUnavailable(
                new KillRandomAgentEffect(
                    new FakeGameAdapter { IsGameReady = true, LivingAgentCount = 0 }
                ),
                ErrorTags.NoAgents
            );
        }

        [Fact]
        public void RandomAgentPanic_is_unavailable_with_no_agents_when_none_are_controllable()
        {
            AssertUnavailable(
                new RandomAgentPanicEffect(
                    new FakeGameAdapter { IsGameReady = true, ControllableAgentCount = 0 }
                ),
                ErrorTags.NoAgents
            );
        }

        [Fact]
        public void EscapeRandomCreature_is_unavailable_with_no_creatures_when_facility_has_none()
        {
            AssertUnavailable(
                new EscapeRandomCreatureEffect(
                    new FakeGameAdapter { IsGameReady = true, CreatureCount = 0 }
                ),
                ErrorTags.NoCreatures
            );
        }

        // ----- available when all preconditions are met -----

        [Fact]
        public void RandomMeltdown_is_available_when_game_is_ready()
        {
            AssertAvailable(new RandomMeltdownEffect(new FakeGameAdapter { IsGameReady = true }));
        }

        [Fact]
        public void KillRandomAgent_is_available_when_an_agent_lives()
        {
            AssertAvailable(
                new KillRandomAgentEffect(
                    new FakeGameAdapter { IsGameReady = true, LivingAgentCount = 1 }
                )
            );
        }

        [Fact]
        public void RandomAgentPanic_is_available_when_a_controllable_agent_exists()
        {
            AssertAvailable(
                new RandomAgentPanicEffect(
                    new FakeGameAdapter { IsGameReady = true, ControllableAgentCount = 1 }
                )
            );
        }

        [Fact]
        public void EscapeRandomCreature_is_available_when_a_creature_exists()
        {
            AssertAvailable(
                new EscapeRandomCreatureEffect(
                    new FakeGameAdapter { IsGameReady = true, CreatureCount = 1 }
                )
            );
        }

        [Fact]
        public void ShowSystemMessage_is_available_when_game_is_ready()
        {
            AssertAvailable(
                new ShowSystemMessageEffect(new FakeGameAdapter { IsGameReady = true })
            );
        }

        [Fact]
        public void SetGameSpeed_is_available_when_game_is_ready()
        {
            AssertAvailable(new SetGameSpeedEffect(new FakeGameAdapter { IsGameReady = true }));
        }

        [Fact]
        public void AddEnergy_is_available_when_game_is_ready()
        {
            AssertAvailable(
                new AddEnergyEffect(new FakeGameAdapter { IsGameReady = true }, new FakeConfig())
            );
        }

        [Fact]
        public void RemoveEnergy_is_available_when_game_is_ready()
        {
            AssertAvailable(
                new RemoveEnergyEffect(new FakeGameAdapter { IsGameReady = true }, new FakeConfig())
            );
        }

        [Fact]
        public void AddMoney_is_available_when_game_is_ready()
        {
            AssertAvailable(
                new AddMoneyEffect(new FakeGameAdapter { IsGameReady = true }, new FakeConfig())
            );
        }

        private static void AssertAvailable(IEffectExecutor executor)
        {
            string reason;
            executor.IsAvailableNow(out reason).Should().BeTrue();
            reason.Should().BeNull();
        }

        private static void AssertUnavailable(IEffectExecutor executor, string expectedReason)
        {
            string reason;
            executor.IsAvailableNow(out reason).Should().BeFalse();
            reason.Should().Be(expectedReason);
        }
    }
}
