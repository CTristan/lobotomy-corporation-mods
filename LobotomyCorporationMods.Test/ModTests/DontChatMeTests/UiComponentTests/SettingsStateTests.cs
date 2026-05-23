// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.UiComponents;
using LobotomyCorporationMods.Test.ModTests.DontChatMeTests.Fakes;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.UiComponentTests
{
    /// <summary>
    ///     Covers the thread-safe state holder backing the Settings window. The most important
    ///     invariant — reveal flags reset on every <c>Open</c> and <c>Close</c> — is the streamer
    ///     safety guarantee documented in the design.
    /// </summary>
    public sealed class SettingsStateTests
    {
        private static SettingsDraft Draft() =>
            new SettingsDraft("ws://example.com/ws/game_mod", "tok", "42", enabled: true);

        [Fact]
        public void Default_state_is_closed()
        {
            var state = new SettingsState();

            state.Snapshot.IsOpen.Should().BeFalse();
        }

        [Fact]
        public void Default_draft_has_empty_strings_and_enabled_true()
        {
            var state = new SettingsState();

            state.Snapshot.Draft.ServerUrl.Should().BeEmpty();
            state.Snapshot.Draft.AuthToken.Should().BeEmpty();
            state.Snapshot.Draft.GameId.Should().BeEmpty();
            state.Snapshot.Draft.Enabled.Should().BeTrue();
        }

        [Fact]
        public void Open_sets_IsOpen_true_and_replaces_draft()
        {
            var state = new SettingsState();
            var draft = Draft();

            state.Open(draft);

            state.Snapshot.IsOpen.Should().BeTrue();
            state.Snapshot.Draft.Should().BeSameAs(draft);
        }

        [Fact]
        public void Open_resets_reveal_flags_to_false()
        {
            var state = new SettingsState();
            state.Open(Draft());
            state.ToggleReveal(SettingsField.ServerUrl);
            state.ToggleReveal(SettingsField.AuthToken);

            state.Open(Draft());

            state.Snapshot.IsServerUrlRevealed.Should().BeFalse();
            state.Snapshot.IsAuthTokenRevealed.Should().BeFalse();
        }

        [Fact]
        public void Close_sets_IsOpen_false()
        {
            var state = new SettingsState();
            state.Open(Draft());

            state.Close();

            state.Snapshot.IsOpen.Should().BeFalse();
        }

        [Fact]
        public void Close_resets_reveal_flags_to_false()
        {
            var state = new SettingsState();
            state.Open(Draft());
            state.ToggleReveal(SettingsField.ServerUrl);
            state.ToggleReveal(SettingsField.AuthToken);

            state.Close();

            state.Snapshot.IsServerUrlRevealed.Should().BeFalse();
            state.Snapshot.IsAuthTokenRevealed.Should().BeFalse();
        }

        [Fact]
        public void Reveal_state_does_not_leak_across_open_and_close_cycles()
        {
            var state = new SettingsState();
            state.Open(Draft());
            state.ToggleReveal(SettingsField.AuthToken);
            state.Close();

            state.Open(Draft());

            state.Snapshot.IsAuthTokenRevealed.Should().BeFalse();
        }

        [Fact]
        public void ToggleReveal_flips_only_the_specified_field()
        {
            var state = new SettingsState();
            state.Open(Draft());

            state.ToggleReveal(SettingsField.ServerUrl);

            state.Snapshot.IsServerUrlRevealed.Should().BeTrue();
            state.Snapshot.IsAuthTokenRevealed.Should().BeFalse();
        }

        [Fact]
        public void ToggleReveal_twice_returns_to_masked()
        {
            var state = new SettingsState();
            state.Open(Draft());

            state.ToggleReveal(SettingsField.ServerUrl);
            state.ToggleReveal(SettingsField.ServerUrl);

            state.Snapshot.IsServerUrlRevealed.Should().BeFalse();
        }

        [Fact]
        public void UpdateDraft_replaces_the_current_draft()
        {
            var state = new SettingsState();
            state.Open(Draft());
            var newDraft = new SettingsDraft(
                "wss://new.example/ws/game_mod",
                "tok-2",
                "100",
                enabled: false
            );

            state.UpdateDraft(newDraft);

            state.Snapshot.Draft.Should().BeSameAs(newDraft);
        }

        [Fact]
        public void SetError_with_message_is_observable_via_snapshot()
        {
            var state = new SettingsState();

            state.SetError("bad url");

            state.Snapshot.LastError.Should().Be("bad url");
        }

        [Fact]
        public void SetError_with_null_clears_the_previous_error()
        {
            var state = new SettingsState();
            state.SetError("bad url");

            state.SetError(null);

            state.Snapshot.LastError.Should().BeNull();
        }

        [Fact]
        public void Open_clears_a_lingering_error_from_a_previous_session()
        {
            var state = new SettingsState();
            state.Open(Draft());
            state.SetError("bad url");
            state.Close();

            state.Open(Draft());

            state.Snapshot.LastError.Should().BeNull();
        }

        [Fact]
        public void FromConfig_populates_the_draft_from_config_values()
        {
            var config = new FakeConfig
            {
                ServerUrl = new Uri("ws://example.com/ws/game_mod"),
                AuthToken = "tok-7",
                GameId = 42,
                Enabled = false,
            };

            var draft = SettingsDraft.FromConfig(config);

            draft.ServerUrl.Should().Be("ws://example.com/ws/game_mod");
            draft.AuthToken.Should().Be("tok-7");
            draft.GameId.Should().Be("42");
            draft.Enabled.Should().BeFalse();
        }

        [Fact]
        public void FromConfig_uses_empty_string_when_ServerUrl_is_null()
        {
            var config = new FakeConfig
            {
                ServerUrl = null,
                AuthToken = "t",
                GameId = 0,
                Enabled = true,
            };

            var draft = SettingsDraft.FromConfig(config);

            draft.ServerUrl.Should().BeEmpty();
        }

        [Fact]
        public void FromConfig_uses_empty_string_when_GameId_is_zero()
        {
            var config = new FakeConfig
            {
                ServerUrl = new Uri("ws://x"),
                AuthToken = "t",
                GameId = 0,
                Enabled = true,
            };

            var draft = SettingsDraft.FromConfig(config);

            draft.GameId.Should().BeEmpty();
        }

        [Fact]
        public void SettingsDraft_constructor_coalesces_null_strings_to_empty()
        {
            var draft = new SettingsDraft(
                serverUrl: null,
                authToken: null,
                gameId: null,
                enabled: true
            );

            draft.ServerUrl.Should().BeEmpty();
            draft.AuthToken.Should().BeEmpty();
            draft.GameId.Should().BeEmpty();
        }
    }
}
