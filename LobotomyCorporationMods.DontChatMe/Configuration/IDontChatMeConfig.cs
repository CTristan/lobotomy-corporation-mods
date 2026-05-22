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

        /// <summary>Auth token sent in the <c>hello</c> frame. Treat as a secret.</summary>
        string AuthToken { get; set; }

        /// <summary>Master switch. When false, the transport stays disconnected and effects don't run.</summary>
        bool Enabled { get; set; }

        /// <summary>When false, effects in the "danger" category (escape a creature, etc.) reply <c>danger_effects_disabled</c>.</summary>
        bool DangerEffectsEnabled { get; }

        /// <summary>Max simultaneous in-flight redemptions before the queue rejects new arrivals.</summary>
        int MaxInFlight { get; }

        /// <summary>Minimum seconds between any two executed effects. <c>0</c> disables global throttling.</summary>
        float GlobalCooldownSeconds { get; }

        /// <summary>Energy delta for <c>add_energy</c> / <c>remove_energy</c>.</summary>
        float EnergyAmount { get; }

        /// <summary>LOB Points granted by <c>add_money</c>.</summary>
        int MoneyAmount { get; }
    }
}
