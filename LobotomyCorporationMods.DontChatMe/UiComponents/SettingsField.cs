// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.UiComponents
{
    /// <summary>
    ///     Confidential fields in the Settings window that can be revealed on demand.
    ///     The bool <c>Enabled</c> field is never confidential so it isn't listed here.
    /// </summary>
    public enum SettingsField
    {
        ServerUrl,
        AuthToken,
    }
}
