// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.UiComponents
{
    /// <summary>
    ///     Immutable point-in-time view of the Settings window. The OnGUI renderer reads one
    ///     of these per frame so it sees a consistent set of fields even while producers
    ///     mutate the underlying <see cref="SettingsState" />.
    /// </summary>
    public sealed class SettingsSnapshot
    {
        public SettingsSnapshot(
            bool isOpen,
            SettingsDraft draft,
            bool isServerUrlRevealed,
            bool isAuthTokenRevealed,
            string lastError
        )
        {
            IsOpen = isOpen;
            Draft = draft;
            IsServerUrlRevealed = isServerUrlRevealed;
            IsAuthTokenRevealed = isAuthTokenRevealed;
            LastError = lastError;
        }

        public bool IsOpen { get; }

        /// <summary>Never null. The editor's current field values.</summary>
        public SettingsDraft Draft { get; }

        public bool IsServerUrlRevealed { get; }
        public bool IsAuthTokenRevealed { get; }

        /// <summary>Validation error from the last <c>Apply</c> attempt; <c>null</c> when no error.</summary>
        public string LastError { get; }
    }
}
