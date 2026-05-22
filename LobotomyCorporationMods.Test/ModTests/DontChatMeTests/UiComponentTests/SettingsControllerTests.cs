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
    ///     Covers the Settings window's Apply orchestration: URL validation, write-through to
    ///     <c>IDontChatMeConfig</c>, and the transport-restart hand-off.
    /// </summary>
    public sealed class SettingsControllerTests
    {
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

        [Fact]
        public void Apply_with_valid_ws_url_returns_Ok_and_writes_all_three_fields()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);
            var draft = new SettingsDraft("ws://127.0.0.1:8585/mod/socket/", "tok", true);

            var result = controller.Apply(draft);

            result.Should().Be(SettingsApplyResult.Ok);
            config.ServerUrl.Should().Be(new Uri("ws://127.0.0.1:8585/mod/socket/"));
            config.AuthToken.Should().Be("tok");
            config.Enabled.Should().BeTrue();
        }

        [Fact]
        public void Apply_with_valid_wss_url_is_accepted()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);
            var draft = new SettingsDraft("wss://chat.example/mod/socket", "tok", true);

            var result = controller.Apply(draft);

            result.Should().Be(SettingsApplyResult.Ok);
        }

        [Fact]
        public void Apply_restarts_the_transport_on_Ok()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);
            var draft = new SettingsDraft("ws://127.0.0.1:8585/mod/socket/", "tok", true);

            controller.Apply(draft);

            restarter.RestartCount.Should().Be(1);
        }

        [Fact]
        public void Apply_with_malformed_url_returns_InvalidServerUrl_and_does_not_write()
        {
            var config = new FakeConfig
            {
                ServerUrl = new Uri("ws://existing.example/mod/socket"),
                AuthToken = "existing-tok",
            };
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);
            var draft = new SettingsDraft("not a url", "new-tok", false);

            var result = controller.Apply(draft);

            result.Should().Be(SettingsApplyResult.InvalidServerUrl);
            config.ServerUrl.Should().Be(new Uri("ws://existing.example/mod/socket"));
            config.AuthToken.Should().Be("existing-tok");
        }

        [Fact]
        public void Apply_with_empty_url_returns_InvalidServerUrl()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);
            var draft = new SettingsDraft(string.Empty, "tok", true);

            var result = controller.Apply(draft);

            result.Should().Be(SettingsApplyResult.InvalidServerUrl);
        }

        [Fact]
        public void Apply_with_http_scheme_returns_InvalidScheme_and_does_not_write()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);
            var draft = new SettingsDraft("http://example.com/mod/socket", "tok", true);

            var result = controller.Apply(draft);

            result.Should().Be(SettingsApplyResult.InvalidScheme);
            config.ServerUrl.Should().BeNull();
        }

        [Fact]
        public void Apply_with_invalid_url_does_not_restart_the_transport()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);
            var draft = new SettingsDraft("not a url", "tok", true);

            controller.Apply(draft);

            restarter.RestartCount.Should().Be(0);
        }

        [Fact]
        public void Apply_with_invalid_scheme_does_not_restart_the_transport()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);
            var draft = new SettingsDraft("http://example.com/", "tok", true);

            controller.Apply(draft);

            restarter.RestartCount.Should().Be(0);
        }

        [Fact]
        public void Apply_writes_Enabled_false_when_user_unchecks_it()
        {
            var config = new FakeConfig { Enabled = true };
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);
            var draft = new SettingsDraft("ws://127.0.0.1:8585/mod/socket/", "tok", false);

            var result = controller.Apply(draft);

            result.Should().Be(SettingsApplyResult.Ok);
            config.Enabled.Should().BeFalse();
        }

        [Fact]
        public void Apply_still_restarts_when_Enabled_is_false_so_the_transport_can_stop()
        {
            var config = new FakeConfig();
            var restarter = new FakeRestarter();
            var controller = BuildController(config, restarter);
            var draft = new SettingsDraft("ws://127.0.0.1:8585/mod/socket/", "tok", false);

            controller.Apply(draft);

            restarter.RestartCount.Should().Be(1);
        }
    }
}
