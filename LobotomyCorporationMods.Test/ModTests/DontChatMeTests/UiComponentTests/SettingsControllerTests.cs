// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.Transport;
using LobotomyCorporationMods.DontChatMe.UiComponents;
using LobotomyCorporationMods.Test.ModTests.DontChatMeTests.Fakes;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.UiComponentTests
{
    /// <summary>
    ///     Covers the Settings window's Apply orchestration: URL validation, game-id validation,
    ///     write-through to <c>IDontChatMeConfig</c>, and the transport-restart hand-off.
    /// </summary>
    public sealed class SettingsControllerTests
    {
        private const string ValidUrl = "ws://127.0.0.1:8585/ws/game_mod";
        private const string ValidGameId = "42";

        private sealed class FakeRestarter : ITransportRestarter
        {
            public int RestartCount { get; private set; }

            public void Restart()
            {
                RestartCount++;
            }
        }

        private static SettingsController BuildController(
            FakeConfig config,
            FakeRestarter restarter
        ) => new SettingsController(config, restarter);

        private static SettingsDraft Draft(
            string url = ValidUrl,
            string token = "tok",
            string gameId = ValidGameId,
            bool enabled = true
        ) => new SettingsDraft(url, token, gameId, enabled);

        [Fact]
        public void Apply_with_valid_ws_url_returns_Ok_and_writes_all_four_fields()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);

            var result = controller.Apply(Draft(gameId: "7"));

            result.Should().Be(SettingsApplyResult.Ok);
            config.ServerUrl.Should().Be(new Uri(ValidUrl));
            config.AuthToken.Should().Be("tok");
            config.GameId.Should().Be(7);
            config.Enabled.Should().BeTrue();
        }

        [Fact]
        public void Apply_with_valid_wss_url_is_accepted()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);
            var draft = Draft(url: "wss://chat.example/ws/game_mod");

            var result = controller.Apply(draft);

            result.Should().Be(SettingsApplyResult.Ok);
        }

        [Fact]
        public void Apply_restarts_the_transport_on_Ok()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);

            controller.Apply(Draft());

            restarter.RestartCount.Should().Be(1);
        }

        [Fact]
        public void Apply_with_malformed_url_returns_InvalidServerUrl_and_does_not_write()
        {
            var config = new FakeConfig
            {
                ServerUrl = new Uri("ws://existing.example/ws/game_mod"),
                AuthToken = "existing-tok",
                GameId = 99,
            };
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);

            var result = controller.Apply(Draft(url: "not a url"));

            result.Should().Be(SettingsApplyResult.InvalidServerUrl);
            config.ServerUrl.Should().Be(new Uri("ws://existing.example/ws/game_mod"));
            config.AuthToken.Should().Be("existing-tok");
            config.GameId.Should().Be(99);
        }

        [Fact]
        public void Apply_with_empty_url_returns_InvalidServerUrl()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);

            var result = controller.Apply(Draft(url: string.Empty));

            result.Should().Be(SettingsApplyResult.InvalidServerUrl);
        }

        [Fact]
        public void Apply_with_http_scheme_returns_InvalidScheme_and_does_not_write()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);

            var result = controller.Apply(Draft(url: "http://example.com/ws/game_mod"));

            result.Should().Be(SettingsApplyResult.InvalidScheme);
            config.ServerUrl.Should().BeNull();
        }

        [Fact]
        public void Apply_with_non_numeric_game_id_returns_InvalidGameId_and_does_not_write()
        {
            var config = new FakeConfig { GameId = 99 };
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);

            var result = controller.Apply(Draft(gameId: "abc"));

            result.Should().Be(SettingsApplyResult.InvalidGameId);
            config.GameId.Should().Be(99);
        }

        [Fact]
        public void Apply_with_empty_game_id_returns_InvalidGameId()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);

            var result = controller.Apply(Draft(gameId: string.Empty));

            result.Should().Be(SettingsApplyResult.InvalidGameId);
        }

        [Fact]
        public void Apply_with_zero_game_id_returns_InvalidGameId()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);

            var result = controller.Apply(Draft(gameId: "0"));

            result.Should().Be(SettingsApplyResult.InvalidGameId);
        }

        [Fact]
        public void Apply_with_negative_game_id_returns_InvalidGameId()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);

            var result = controller.Apply(Draft(gameId: "-1"));

            result.Should().Be(SettingsApplyResult.InvalidGameId);
        }

        [Fact]
        public void Apply_with_invalid_url_does_not_restart_the_transport()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);

            controller.Apply(Draft(url: "not a url"));

            restarter.RestartCount.Should().Be(0);
        }

        [Fact]
        public void Apply_with_invalid_scheme_does_not_restart_the_transport()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);

            controller.Apply(Draft(url: "http://example.com/"));

            restarter.RestartCount.Should().Be(0);
        }

        [Fact]
        public void Apply_with_invalid_game_id_does_not_restart_the_transport()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);

            controller.Apply(Draft(gameId: "abc"));

            restarter.RestartCount.Should().Be(0);
        }

        [Fact]
        public void Apply_writes_Enabled_false_when_user_unchecks_it()
        {
            var config = new FakeConfig { Enabled = true };
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);

            var result = controller.Apply(Draft(enabled: false));

            result.Should().Be(SettingsApplyResult.Ok);
            config.Enabled.Should().BeFalse();
        }

        [Fact]
        public void Apply_still_restarts_when_Enabled_is_false_so_the_transport_can_stop()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);

            controller.Apply(Draft(enabled: false));

            restarter.RestartCount.Should().Be(1);
        }

        [Fact]
        public void Apply_persists_settings_to_config_on_Ok()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);

            controller.Apply(Draft());

            config.SaveCount.Should().Be(1);
        }

        [Fact]
        public void Apply_does_not_persist_settings_when_url_is_invalid()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);

            controller.Apply(Draft(url: "not a url"));

            config.SaveCount.Should().Be(0);
        }

        [Fact]
        public void Apply_does_not_persist_settings_when_game_id_is_invalid()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);

            controller.Apply(Draft(gameId: "abc"));

            config.SaveCount.Should().Be(0);
        }
    }
}
