// SPDX-License-Identifier: MIT

#region

using System;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Configuration
{
    /// <summary>
    ///     Player-visible knobs for the mod.
    ///     Exposed as an interface so tests can stand in a fake without depending on Unity's
    ///     <c>PlayerPrefs</c> / ConfigurationManager runtime.
    /// </summary>
    public interface IDontChatMeConfig
    {
        /// <summary>
        ///     URL of the chat-side server's WebSocket endpoint, e.g. <c>wss://example.com/mod/socket</c>.
        ///     Returns <c>null</c> when the configured value is empty or not a parseable URI; the transport
        ///     treats <c>null</c> as "disabled". Setting writes the URL through to the persisted store.
        /// </summary>
        Uri ServerUrl { get; set; }

        /// <summary>
        ///     Authentication identifier negotiated in the WebSocket subprotocol header at
        ///     upgrade time. Treat as a secret.
        /// </summary>
        string AuthToken { get; set; }

        /// <summary>
        ///     The game-row identifier this mod instance represents. Embedded in the
        ///     <c>hello.game_id</c> field so the server can route dispatches and bind a
        ///     per-game queue. Streamers configure this once per channel.
        /// </summary>
        int GameId { get; set; }

        /// <summary>Master switch. When false, the transport stays disconnected and effects don't run.</summary>
        bool Enabled { get; set; }

        /// <summary>
        ///     When false, effects in the "danger" category (escape a creature, etc.) reply
        ///     <c>effect_disabled</c>.
        /// </summary>
        bool DangerEffectsEnabled { get; }

        /// <summary>Minimum seconds between any two executed effects. <c>0</c> disables global throttling.</summary>
        float GlobalCooldownSeconds { get; }

        /// <summary>Energy delta for <c>add_energy</c> / <c>remove_energy</c>.</summary>
        float EnergyAmount { get; }

        /// <summary>LOB Points granted by <c>add_money</c>.</summary>
        int MoneyAmount { get; }

        /// <summary>
        ///     Persists the three Connection-section settings (<c>ServerUrl</c>, <c>AuthToken</c>,
        ///     <c>Enabled</c>) to disk so they survive game restarts. No-op when no backing file
        ///     is configured (e.g. in tests).
        /// </summary>
        void Save();
    }
}
