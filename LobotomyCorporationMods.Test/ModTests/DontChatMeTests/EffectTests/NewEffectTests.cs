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
    public sealed class NewEffectTests : DontChatMeModTests
    {
        private static EffectDispatch MakeDispatch(string slug, string userDisplayName = null) =>
            new EffectDispatch(
                redemptionId: "r-1",
                effectSlug: slug,
                effectName: null,
                userId: null,
                userDisplayName: userDisplayName,
                gameId: 0,
                dispatchedAt: null
            );

        // ----- AddMoney -----

        [Fact]
        public void AddMoney_uses_the_configured_amount()
        {
            var adapter = new FakeGameAdapter { IsGameReady = true };
            var config = new FakeConfig { MoneyAmount = 250 };
            var effect = new AddMoneyEffect(adapter, config);

            effect.Execute(MakeDispatch(EffectSlugs.AddMoney)).Should().BeNull();
            adapter.LastMoneyAdded.Should().Be(250);
        }

        [Fact]
        public void AddMoney_returns_game_not_ready_when_game_is_not_playing()
        {
            var adapter = new FakeGameAdapter { IsGameReady = false };
            var effect = new AddMoneyEffect(adapter, new FakeConfig());

            effect.Execute(MakeDispatch(EffectSlugs.AddMoney)).Should().Be(ErrorTags.GameNotReady);
        }

        // ----- ShowSystemMessage -----

        [Fact]
        public void ShowSystemMessage_includes_the_user_display_name_when_present()
        {
            var adapter = new FakeGameAdapter { IsGameReady = true };
            var effect = new ShowSystemMessageEffect(adapter);

            effect
                .Execute(MakeDispatch(EffectSlugs.ShowSystemMessage, userDisplayName: "Bob"))
                .Should()
                .BeNull();
            adapter.LastSystemMessage.Should().Contain("Bob");
        }

        [Fact]
        public void ShowSystemMessage_falls_back_to_a_generic_string_when_user_is_anonymous()
        {
            var adapter = new FakeGameAdapter { IsGameReady = true };
            var effect = new ShowSystemMessageEffect(adapter);

            effect
                .Execute(MakeDispatch(EffectSlugs.ShowSystemMessage, userDisplayName: null))
                .Should()
                .BeNull();
            adapter.LastSystemMessage.Should().Be(ShowSystemMessageEffect.GenericMessage);
        }

        // ----- SetGameSpeed -----

        [Fact]
        public void SetGameSpeed_sets_the_fast_speed_value()
        {
            var adapter = new FakeGameAdapter { IsGameReady = true };
            var effect = new SetGameSpeedEffect(adapter);

            effect.Execute(MakeDispatch(EffectSlugs.SetGameSpeed)).Should().BeNull();
            adapter.LastGameSpeed.Should().Be(SetGameSpeedEffect.FastSpeed);
        }

        // ----- EscapeRandomCreature -----

        [Fact]
        public void EscapeRandomCreature_is_marked_as_a_danger_effect()
        {
            new EscapeRandomCreatureEffect(new FakeGameAdapter()).IsDanger.Should().BeTrue();
        }

        [Fact]
        public void EscapeRandomCreature_returns_no_creatures_when_the_facility_has_none()
        {
            var adapter = new FakeGameAdapter { IsGameReady = true, CreatureCount = 0 };
            var effect = new EscapeRandomCreatureEffect(adapter);

            effect
                .Execute(MakeDispatch(EffectSlugs.EscapeRandomCreature))
                .Should()
                .Be(ErrorTags.NoCreatures);
            adapter.Calls.Should().BeEmpty();
        }

        [Fact]
        public void EscapeRandomCreature_releases_one_when_a_creature_is_available()
        {
            var adapter = new FakeGameAdapter { IsGameReady = true, CreatureCount = 3 };
            var effect = new EscapeRandomCreatureEffect(adapter);

            effect.Execute(MakeDispatch(EffectSlugs.EscapeRandomCreature)).Should().BeNull();
            adapter.Calls.Should().Equal(nameof(adapter.EscapeRandomCreature));
        }

        [Fact]
        public void EscapeRandomCreature_returns_execution_error_when_no_non_escaped_creature_remains()
        {
            var adapter = new FakeGameAdapter
            {
                IsGameReady = true,
                CreatureCount = 2,
                EscapeRandomCreatureResult = false,
            };
            var effect = new EscapeRandomCreatureEffect(adapter);

            effect
                .Execute(MakeDispatch(EffectSlugs.EscapeRandomCreature))
                .Should()
                .Be(ErrorTags.ExecutionError);
        }
    }
}
