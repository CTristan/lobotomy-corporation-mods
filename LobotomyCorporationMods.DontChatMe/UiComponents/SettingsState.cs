// SPDX-License-Identifier: MIT

#region

using LobotomyCorporation.Mods.Common;

#endregion

namespace LobotomyCorporationMods.DontChatMe.UiComponents
{
    /// <summary>
    ///     Thread-safe state holder for the Settings window. Producers (F9 keybind on the main thread,
    ///     gear-button click on the main thread, OnGUI Apply callbacks on the main thread, validation
    ///     errors from the controller) update through methods that take the lock; the OnGUI renderer
    ///     reads a single immutable <see cref="SettingsSnapshot" /> per frame.
    /// </summary>
    /// <remarks>
    ///     Reveal flags for confidential fields reset to <c>false</c> on every <see cref="Open" /> and
    ///     <see cref="Close" />. This is intentional: a streamer who reveals a value during one editing
    ///     session must explicitly re-reveal it next time, so a forgotten reveal doesn't leak across
    ///     window opens.
    /// </remarks>
    public sealed class SettingsState
    {
        private readonly object _lock = new object();
        private bool _isOpen;
        private SettingsDraft _draft = new SettingsDraft(
            string.Empty,
            string.Empty,
            string.Empty,
            enabled: true
        );
        private bool _isServerUrlRevealed;
        private bool _isAuthTokenRevealed;
        private string _lastError;

        /// <summary>
        ///     Opens the Settings window with the supplied draft as the starting field values.
        ///     Resets reveal flags and any prior validation error.
        /// </summary>
        public void Open(SettingsDraft initialDraft)
        {
            ThrowHelper.ThrowIfNull(initialDraft, nameof(initialDraft));
            lock (_lock)
            {
                _isOpen = true;
                _draft = initialDraft;
                _isServerUrlRevealed = false;
                _isAuthTokenRevealed = false;
                _lastError = null;
            }
        }

        /// <summary>Closes the window and clears all reveal flags and the last error.</summary>
        public void Close()
        {
            lock (_lock)
            {
                _isOpen = false;
                _isServerUrlRevealed = false;
                _isAuthTokenRevealed = false;
                _lastError = null;
            }
        }

        /// <summary>Replaces the current draft (called from OnGUI as the user types).</summary>
        public void UpdateDraft(SettingsDraft draft)
        {
            ThrowHelper.ThrowIfNull(draft, nameof(draft));
            lock (_lock)
            {
                _draft = draft;
            }
        }

        /// <summary>Flips the reveal flag for the given confidential field.</summary>
        public void ToggleReveal(SettingsField field)
        {
            lock (_lock)
            {
                if (field == SettingsField.ServerUrl)
                {
                    _isServerUrlRevealed = !_isServerUrlRevealed;
                }
                else if (field == SettingsField.AuthToken)
                {
                    _isAuthTokenRevealed = !_isAuthTokenRevealed;
                }
            }
        }

        /// <summary>Sets or clears (pass <c>null</c>) the last-error message shown under the URL field.</summary>
        public void SetError(string error)
        {
            lock (_lock)
            {
                _lastError = error;
            }
        }

        public SettingsSnapshot Snapshot
        {
            get
            {
                lock (_lock)
                {
                    return new SettingsSnapshot(
                        _isOpen,
                        _draft,
                        _isServerUrlRevealed,
                        _isAuthTokenRevealed,
                        _lastError
                    );
                }
            }
        }
    }
}
