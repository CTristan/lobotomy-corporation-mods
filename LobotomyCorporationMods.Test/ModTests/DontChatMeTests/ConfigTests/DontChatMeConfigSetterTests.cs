// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.ConfigTests
{
    /// <summary>
    ///     Verifies that the new Connection-section setters on <c>DontChatMeConfig</c> round-trip
    ///     through the underlying <c>IConfigEntry&lt;T&gt;</c> store and are observable via the getter.
    ///     The first-party Settings UI relies on this round-trip.
    /// </summary>
    public sealed class DontChatMeConfigSetterTests : DontChatMeModTests
    {
        [Fact]
        public void Setting_ServerUrl_round_trips_to_the_getter()
        {
            var config = Harmony_Patch.Instance.Config;
            var expected = new Uri("ws://127.0.0.1:8585/mod/socket/");

            config.ServerUrl = expected;

            config.ServerUrl.Should().Be(expected);
        }

        [Fact]
        public void Setting_ServerUrl_to_null_clears_the_persisted_value()
        {
            var config = Harmony_Patch.Instance.Config;
            config.ServerUrl = new Uri("wss://example.com/mod/socket");

            config.ServerUrl = null;

            config.ServerUrl.Should().BeNull();
        }

        [Fact]
        public void Setting_AuthToken_round_trips_to_the_getter()
        {
            var config = Harmony_Patch.Instance.Config;

            config.AuthToken = "secret-token";

            config.AuthToken.Should().Be("secret-token");
        }

        [Fact]
        public void Setting_AuthToken_to_null_stores_empty_string()
        {
            var config = Harmony_Patch.Instance.Config;
            config.AuthToken = "previous";

            config.AuthToken = null;

            config.AuthToken.Should().BeEmpty();
        }

        [Fact]
        public void Setting_Enabled_round_trips_to_the_getter()
        {
            var config = Harmony_Patch.Instance.Config;

            config.Enabled = false;

            config.Enabled.Should().BeFalse();
        }

        [Fact]
        public void Setting_GameId_round_trips_to_the_getter()
        {
            var config = Harmony_Patch.Instance.Config;

            config.GameId = 42;

            config.GameId.Should().Be(42);
        }
    }
}
