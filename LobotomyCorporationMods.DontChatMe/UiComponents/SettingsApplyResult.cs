// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.UiComponents
{
    /// <summary>
    ///     Result of <see cref="SettingsController.Apply" />. The Settings window uses this to
    ///     decide whether to close (on <see cref="Ok" />) or show an inline error message.
    /// </summary>
    public enum SettingsApplyResult
    {
        /// <summary>All fields validated; config was written and the transport was restarted.</summary>
        Ok,

        /// <summary>The Server URL field could not be parsed as an absolute URI.</summary>
        InvalidServerUrl,

        /// <summary>The Server URL parsed but used a scheme other than <c>ws</c> or <c>wss</c>.</summary>
        InvalidScheme,
    }
}
