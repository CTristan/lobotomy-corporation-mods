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
        public void Build_emits_hello_with_id_game_id_and_client_version()
        {
            HelloFrame
                .Build(id: 1, gameId: 42, clientVersion: "1.0.0", lastSeenRedemptionId: null)
                .Should()
                .Be("{\"type\":\"hello\",\"id\":1,\"game_id\":42,\"client_version\":\"1.0.0\"}");
        }

        [Fact]
        public void Build_includes_last_seen_redemption_id_when_present()
        {
            HelloFrame
                .Build(id: 2, gameId: 7, clientVersion: "1.2.3", lastSeenRedemptionId: "r-99")
                .Should()
                .Be(
                    "{\"type\":\"hello\",\"id\":2,\"game_id\":7,\"client_version\":\"1.2.3\",\"last_seen_redemption_id\":\"r-99\"}"
                );
        }

        [Fact]
        public void Build_omits_last_seen_redemption_id_when_empty()
        {
            HelloFrame
                .Build(id: 3, gameId: 7, clientVersion: "1.0.0", lastSeenRedemptionId: "")
                .Should()
                .Be("{\"type\":\"hello\",\"id\":3,\"game_id\":7,\"client_version\":\"1.0.0\"}");
        }

        [Fact]
        public void Build_escapes_special_characters_in_client_version()
        {
            HelloFrame
                .Build(
                    id: 4,
                    gameId: 1,
                    clientVersion: "1.0.0-\"beta\"",
                    lastSeenRedemptionId: null
                )
                .Should()
                .Contain("\"client_version\":\"1.0.0-\\\"beta\\\"\"");
        }

        [Fact]
        public void Build_escapes_special_characters_in_last_seen_redemption_id()
        {
            HelloFrame
                .Build(id: 5, gameId: 1, clientVersion: "1.0.0", lastSeenRedemptionId: "r\\1\"2")
                .Should()
                .Contain("\"last_seen_redemption_id\":\"r\\\\1\\\"2\"");
        }

        [Fact]
        public void Build_uses_invariant_culture_for_numeric_ids()
        {
            // 1234 must render as "1234", never as "1,234" or "1.234" under a locale
            // that uses thousands separators.
            HelloFrame
                .Build(id: 1234, gameId: 5678, clientVersion: "1.0.0", lastSeenRedemptionId: null)
                .Should()
                .Contain("\"id\":1234")
                .And.Contain("\"game_id\":5678");
        }

        [Fact]
        public void Build_throws_when_client_version_is_null()
        {
            Action act = () =>
                HelloFrame.Build(id: 1, gameId: 1, clientVersion: null, lastSeenRedemptionId: null);

            act.Should().Throw<ArgumentNullException>();
        }
    }
}
