// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Configuration;

#endregion

namespace LobotomyCorporationMods.DontChatMe.UiComponents
{
    /// <summary>
    ///     Editor-side draft of the Connection-section settings while the Settings window is open.
    ///     Strings (not parsed types) — the OnGUI text fields write strings directly, and
    ///     <see cref="SettingsController.Apply" /> is responsible for parsing and validation.
    /// </summary>
    [SuppressMessage(
        "Design",
        "CA1054:URI-like parameters should not be strings",
        Justification = "ServerUrl is the user's in-progress edit text and may be partial or invalid; only SettingsController.Apply parses it into a Uri."
    )]
    [SuppressMessage(
        "Design",
        "CA1056:URI-like properties should not be strings",
        Justification = "Same as the parameter: the draft holds the literal text the user has typed, not a parsed URI."
    )]
    public sealed class SettingsDraft
    {
        public SettingsDraft(string serverUrl, string authToken, string gameId, bool enabled)
        {
            ServerUrl = serverUrl ?? string.Empty;
            AuthToken = authToken ?? string.Empty;
            GameId = gameId ?? string.Empty;
            Enabled = enabled;
        }

        public string ServerUrl { get; }
        public string AuthToken { get; }

        /// <summary>
        ///     The user's in-progress text for the Game ID field. Held as a string so the editor
        ///     can show whatever they've typed mid-edit, including invalid values like "" or "abc".
        ///     <see cref="SettingsController.Apply" /> parses and validates this into an <c>int</c>.
        /// </summary>
        public string GameId { get; }

        public bool Enabled { get; }

        /// <summary>
        ///     Builds an initial draft from the live config so both entry points (gear button,
        ///     F9 keybind) start the editor with the user's existing values.
        /// </summary>
        public static SettingsDraft FromConfig(IDontChatMeConfig config)
        {
            ThrowHelper.ThrowIfNull(config, nameof(config));
            var serverUrl = config.ServerUrl == null ? string.Empty : config.ServerUrl.ToString();
            var gameId =
                config.GameId == 0
                    ? string.Empty
                    : config.GameId.ToString(CultureInfo.InvariantCulture);
            return new SettingsDraft(serverUrl, config.AuthToken, gameId, config.Enabled);
        }
    }
}
