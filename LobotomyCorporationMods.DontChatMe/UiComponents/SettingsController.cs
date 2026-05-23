// SPDX-License-Identifier: MIT

#region

using System;
using System.Globalization;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Configuration;
using LobotomyCorporationMods.DontChatMe.Transport;

#endregion

namespace LobotomyCorporationMods.DontChatMe.UiComponents
{
    /// <summary>
    ///     Orchestrates the Apply step of the Settings window. Validates the user's draft,
    ///     writes the Connection-section settings through to the persistent config, and
    ///     restarts the transport so the new values take effect immediately. Fully testable
    ///     — the OnGUI body in <c>SettingsWindow</c> calls this and only this.
    /// </summary>
    public sealed class SettingsController
    {
        private readonly IDontChatMeConfig _config;
        private readonly ITransportRestarter _transport;

        public SettingsController(IDontChatMeConfig config, ITransportRestarter transport)
        {
            ThrowHelper.ThrowIfNull(config, nameof(config));
            ThrowHelper.ThrowIfNull(transport, nameof(transport));
            _config = config;
            _transport = transport;
        }

        /// <summary>
        ///     Validates and commits the draft. On a non-<see cref="SettingsApplyResult.Ok" /> result,
        ///     no config writes occur and the transport is not restarted — the caller leaves the
        ///     window open and shows the error.
        /// </summary>
        public SettingsApplyResult Apply(SettingsDraft draft)
        {
            ThrowHelper.ThrowIfNull(draft, nameof(draft));

            Uri parsed;
            if (!Uri.TryCreate(draft.ServerUrl, UriKind.Absolute, out parsed))
            {
                return SettingsApplyResult.InvalidServerUrl;
            }

            if (!IsWebSocketScheme(parsed.Scheme))
            {
                return SettingsApplyResult.InvalidScheme;
            }

            int parsedGameId;
            if (
                !int.TryParse(
                    draft.GameId,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out parsedGameId
                )
                || parsedGameId <= 0
            )
            {
                return SettingsApplyResult.InvalidGameId;
            }

            _config.ServerUrl = parsed;
            _config.AuthToken = draft.AuthToken;
            _config.GameId = parsedGameId;
            _config.Enabled = draft.Enabled;
            _config.Save();
            _transport.Restart();
            return SettingsApplyResult.Ok;
        }

        private static bool IsWebSocketScheme(string scheme)
        {
            return string.Equals(scheme, "ws", StringComparison.OrdinalIgnoreCase)
                || string.Equals(scheme, "wss", StringComparison.OrdinalIgnoreCase);
        }
    }
}
