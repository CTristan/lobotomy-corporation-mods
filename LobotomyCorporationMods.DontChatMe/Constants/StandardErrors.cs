// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.Constants
{
    /// <summary>
    ///     Canonical <c>reason</c> strings emitted on outbound <c>effect_response</c> and
    ///     <c>effect_state</c> frames. Mirrors the server's
    ///     <c>Hemograce.Dispatch.StandardErrors</c> atom set; any string the server
    ///     doesn't recognise collapses to <c>unknown</c> on receipt, so a typo here is a
    ///     visibility bug not a parse failure.
    /// </summary>
    public static class StandardErrors
    {
        /// <summary>The mod doesn't know this effect slug.</summary>
        public const string EffectUnknown = "effect_unknown";

        /// <summary>The slug is disabled in mod config (e.g. danger effects switched off).</summary>
        public const string EffectDisabled = "effect_disabled";

        /// <summary>Slug is temporarily unavailable — no agents, no creatures, in-effect cooldown, etc.</summary>
        public const string EffectUnavailableNow = "effect_unavailable_now";

        /// <summary>Game phase blocks every effect (menu, briefing, mission-ended).</summary>
        public const string GameStateBlocked = "game_state_blocked";

        /// <summary>Per-streamer / per-mod viewer block; the requesting viewer isn't allowed to redeem.</summary>
        public const string ViewerBlocked = "viewer_blocked";

        /// <summary>Global or per-effect cooldown gate.</summary>
        public const string Cooldown = "cooldown";

        /// <summary>LRU cache says this redemption is already terminal.</summary>
        public const string DuplicateRedemption = "duplicate_redemption";

        /// <summary>Unhandled exception, parse failure, or any other unexpected mod-internal condition.</summary>
        public const string ModInternalError = "mod_internal_error";

        /// <summary>Reserved: server emits this when its per-game queue is at the depth cap.</summary>
        public const string QueueFull = "queue_full";

        /// <summary>Reserved: server emits this when a queue head sat past the gating-hold window.</summary>
        public const string GatingTimeout = "gating_timeout";

        /// <summary>Catch-all for any wire string the receiver doesn't recognise.</summary>
        public const string Unknown = "unknown";
    }
}
