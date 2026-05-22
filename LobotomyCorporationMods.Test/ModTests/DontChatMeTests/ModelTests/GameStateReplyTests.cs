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
        public void ToJson_emits_game_state_with_phase()
        {
            var reply = new GameStateReply(GamePhases.Ready);

            reply.ToJson().Should().Be("{\"type\":\"game_state\",\"phase\":\"ready\"}");
        }

        [Fact]
        public void ToJson_escapes_special_characters_in_phase()
        {
            var reply = new GameStateReply("a\"b\\c");

            reply.ToJson().Should().Be("{\"type\":\"game_state\",\"phase\":\"a\\\"b\\\\c\"}");
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
            var a = new GameStateReply(GamePhases.MeltdownActive);
            var b = new GameStateReply(GamePhases.MeltdownActive);
            var c = new GameStateReply(GamePhases.Paused);

            a.Equals(b).Should().BeTrue();
            a.Equals(c).Should().BeFalse();
            a.GetHashCode().Should().Be(b.GetHashCode());
        }
    }
}
