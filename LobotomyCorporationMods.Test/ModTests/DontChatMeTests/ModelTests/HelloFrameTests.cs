// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.Models;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.ModelTests
{
    public sealed class HelloFrameTests : DontChatMeModTests
    {
        [Fact]
        public void Build_emits_a_hello_frame_with_all_four_fields()
        {
            HelloFrame
                .Build("tok-1", "1.0.0")
                .Should()
                .Be(
                    "{\"type\":\"hello\",\"token\":\"tok-1\",\"client\":\"dont-chat-me\",\"version\":\"1.0.0\"}"
                );
        }

        [Fact]
        public void Build_escapes_secret_characters_in_the_token()
        {
            HelloFrame.Build("a\"b\\c", "1.0.0").Should().Contain("\"token\":\"a\\\"b\\\\c\"");
        }

        [Fact]
        public void Build_throws_when_token_is_null()
        {
            Action act = () => HelloFrame.Build(token: null, version: "1.0.0");

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void BuildPong_emits_only_the_type_field()
        {
            HelloFrame.BuildPong().Should().Be("{\"type\":\"pong\"}");
        }
    }
}
