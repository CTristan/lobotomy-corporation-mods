// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Models;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.ModelTests
{
    public sealed class GameStateReplyTests : DontChatMeModTests
    {
        [Fact]
        public void ToJson_emits_game_state_with_id_and_phase()
        {
            var reply = new GameStateReply(GamePhases.InPlay);

            reply
                .ToJson(id: 5)
                .Should()
                .Be("{\"type\":\"game_state\",\"id\":5,\"phase\":\"in_play\"}");
        }

        [Fact]
        public void ToJson_serializes_each_phase_value()
        {
            new GameStateReply(GamePhases.NotInPlay).ToJson(1).Should().Contain("\"not_in_play\"");
            new GameStateReply(GamePhases.Briefing).ToJson(1).Should().Contain("\"briefing\"");
            new GameStateReply(GamePhases.Paused).ToJson(1).Should().Contain("\"paused\"");
            new GameStateReply(GamePhases.MissionEnded)
                .ToJson(1)
                .Should()
                .Contain("\"mission_ended\"");
        }

        [Fact]
        public void ToJson_uses_invariant_culture_for_the_id_field()
        {
            new GameStateReply(GamePhases.InPlay).ToJson(id: 1234).Should().Contain("\"id\":1234");
        }

        [Fact]
        public void ToJson_escapes_special_characters_in_phase()
        {
            new GameStateReply("a\"b\\c")
                .ToJson(id: 1)
                .Should()
                .Be("{\"type\":\"game_state\",\"id\":1,\"phase\":\"a\\\"b\\\\c\"}");
        }

        [Fact]
        public void Constructor_throws_when_phase_is_null()
        {
            Action act = () => _ = new GameStateReply(phase: null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Equals_compares_phase()
        {
            var a = new GameStateReply(GamePhases.InPlay);
            var b = new GameStateReply(GamePhases.InPlay);
            var c = new GameStateReply(GamePhases.Paused);

            a.Equals(b).Should().BeTrue();
            a.Equals(c).Should().BeFalse();
            a.GetHashCode().Should().Be(b.GetHashCode());
        }
    }
}
