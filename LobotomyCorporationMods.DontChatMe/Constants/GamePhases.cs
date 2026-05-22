// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.Constants
{
    /// <summary>
    ///     String values for the <c>phase</c> field of a <c>game_state</c> frame. The chat-side
    ///     server uses these to drive its UI (e.g. show a "day in progress" banner or grey out
    ///     all effects during a meltdown). Values are stable identifiers — never localized.
    /// </summary>
    public static class GamePhases
    {
        /// <summary>Game is playing and no special state (meltdown/ordeal/pause) is active.</summary>
        public const string Ready = "ready";

        /// <summary>Pre-shift management screen (agent hiring/equipping). Reserved for future use.</summary>
        public const string AgentManagement = "agent_management";

        /// <summary>End-of-day animation or summary screen. Reserved for future use.</summary>
        public const string DayEnding = "day_ending";

        /// <summary>One or more abnormalities are currently overloaded.</summary>
        public const string MeltdownActive = "meltdown_active";

        /// <summary>One or more ordeals are currently active.</summary>
        public const string OrdealActive = "ordeal_active";

        /// <summary>Game is paused (player-initiated or by an in-game event).</summary>
        public const string Paused = "paused";

        /// <summary>No day is in progress (main menu, between-day, or the GameManager singleton is unavailable).</summary>
        public const string NoDay = "no_day";
    }
}
