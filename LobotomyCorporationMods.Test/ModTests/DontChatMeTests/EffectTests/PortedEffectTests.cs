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
    public sealed class PortedEffectTests : DontChatMeModTests
    {
        private static EffectDispatch MakeDispatch(string slug) =>
            new EffectDispatch(
                redemptionId: "r-1",
                effectSlug: slug,
                effectName: null,
                userId: null,
                userDisplayName: null,
                gameId: 0,
                dispatchedAt: null,
                attempts: 1,
                replay: false
            );

        // ----- RandomMeltdown -----

        [Fact]
        public void RandomMeltdown_returns_game_not_ready_when_game_is_not_playing()
        {
            var adapter = new FakeGameAdapter { IsGameReady = false };
            var effect = new RandomMeltdownEffect(adapter);

            effect
                .Execute(MakeDispatch(EffectSlugs.RandomMeltdown))
                .Should()
                .Be(StandardErrors.GameStateBlocked);
            adapter.Calls.Should().BeEmpty();
        }

        [Fact]
        public void RandomMeltdown_fires_meltdown_when_game_is_ready()
        {
            var adapter = new FakeGameAdapter { IsGameReady = true };
            var effect = new RandomMeltdownEffect(adapter);

            effect.Execute(MakeDispatch(EffectSlugs.RandomMeltdown)).Should().BeNull();
            adapter
                .Calls.Should()
                .ContainSingle()
                .Which.Should()
                .Be(nameof(adapter.ActivateRandomMeltdown));
        }

        [Fact]
        public void RandomMeltdown_returns_execution_error_when_no_abnormality_is_available()
        {
            var adapter = new FakeGameAdapter
            {
                IsGameReady = true,
                ActivateRandomMeltdownResult = false,
            };
            var effect = new RandomMeltdownEffect(adapter);

            effect
                .Execute(MakeDispatch(EffectSlugs.RandomMeltdown))
                .Should()
                .Be(StandardErrors.ModInternalError);
        }

        // ----- KillRandomAgent -----

        [Fact]
        public void KillRandomAgent_returns_no_agents_when_facility_is_empty()
        {
            var adapter = new FakeGameAdapter { IsGameReady = true, LivingAgentCount = 0 };
            var effect = new KillRandomAgentEffect(adapter);

            effect
                .Execute(MakeDispatch(EffectSlugs.KillRandomAgent))
                .Should()
                .Be(StandardErrors.EffectUnavailableNow);
            adapter.Calls.Should().BeEmpty();
        }

        [Fact]
        public void KillRandomAgent_kills_an_agent_when_available()
        {
            var adapter = new FakeGameAdapter { IsGameReady = true, LivingAgentCount = 3 };
            var effect = new KillRandomAgentEffect(adapter);

            effect.Execute(MakeDispatch(EffectSlugs.KillRandomAgent)).Should().BeNull();
            adapter.Calls.Should().Equal(nameof(adapter.KillRandomLivingAgent));
        }

        // ----- RandomAgentPanic -----

        [Fact]
        public void RandomAgentPanic_returns_no_agents_when_none_are_controllable()
        {
            var adapter = new FakeGameAdapter { IsGameReady = true, ControllableAgentCount = 0 };
            var effect = new RandomAgentPanicEffect(adapter);

            effect
                .Execute(MakeDispatch(EffectSlugs.RandomAgentPanic))
                .Should()
                .Be(StandardErrors.EffectUnavailableNow);
            adapter.Calls.Should().BeEmpty();
        }

        [Fact]
        public void RandomAgentPanic_forces_panic_when_a_controllable_agent_exists()
        {
            var adapter = new FakeGameAdapter { IsGameReady = true, ControllableAgentCount = 2 };
            var effect = new RandomAgentPanicEffect(adapter);

            effect.Execute(MakeDispatch(EffectSlugs.RandomAgentPanic)).Should().BeNull();
            adapter.Calls.Should().Equal(nameof(adapter.ForcePanicOnRandomControllableAgent));
        }

        // ----- AddEnergy / RemoveEnergy -----

        [Fact]
        public void AddEnergy_uses_the_configured_amount()
        {
            var adapter = new FakeGameAdapter { IsGameReady = true };
            var config = new FakeConfig { EnergyAmount = 25f };
            var effect = new AddEnergyEffect(adapter, config);

            effect.Execute(MakeDispatch(EffectSlugs.AddEnergy)).Should().BeNull();
            adapter.LastEnergyAdded.Should().Be(25f);
        }

        [Fact]
        public void AddEnergy_returns_game_not_ready_when_game_is_not_playing()
        {
            var adapter = new FakeGameAdapter { IsGameReady = false };
            var config = new FakeConfig { EnergyAmount = 25f };
            var effect = new AddEnergyEffect(adapter, config);

            effect
                .Execute(MakeDispatch(EffectSlugs.AddEnergy))
                .Should()
                .Be(StandardErrors.GameStateBlocked);
            adapter.Calls.Should().BeEmpty();
        }

        [Fact]
        public void RemoveEnergy_uses_the_configured_amount()
        {
            var adapter = new FakeGameAdapter { IsGameReady = true };
            var config = new FakeConfig { EnergyAmount = 7f };
            var effect = new RemoveEnergyEffect(adapter, config);

            effect.Execute(MakeDispatch(EffectSlugs.RemoveEnergy)).Should().BeNull();
            adapter.LastEnergySubtracted.Should().Be(7f);
        }
    }
}
