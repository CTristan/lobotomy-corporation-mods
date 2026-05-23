// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.Constants
{
    /// <summary>
    ///     String values for the <c>phase</c> field of a <c>game_state</c> frame. The server's
    ///     <c>Hemograce.Dispatch.GameQueue</c> only dispatches when phase is
    ///     <see cref="InPlay" />; every other phase pauses the entire queue (Case A — game-wide
    ///     gate). Values are stable wire identifiers — never localized.
    /// </summary>
    public static class GamePhases
    {
        /// <summary>No mission is active — main menu, between-day, agent management.</summary>
        public const string NotInPlay = "not_in_play";

        /// <summary>Pre-mission briefing or setup screens. Queue stays paused.</summary>
        public const string Briefing = "briefing";

        /// <summary>Mission is active and effects can run. Only phase that unblocks the queue.</summary>
        public const string InPlay = "in_play";

        /// <summary>Game is paused (player-initiated or by an in-game event).</summary>
        public const string Paused = "paused";

        /// <summary>End-of-mission animation or summary screen.</summary>
        public const string MissionEnded = "mission_ended";
    }
}
