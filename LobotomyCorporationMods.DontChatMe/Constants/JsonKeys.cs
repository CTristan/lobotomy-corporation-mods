// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.Constants
{
    /// <summary>JSON field names used in wire frames.</summary>
    /// <remarks>
    /// Canonical protocol: <c>apps/hemograce_web/docs/game_mod_protocol.md</c> in the
    /// hemograce repository. Both sides of the WebSocket conform to that spec; this
    /// class holds the field names the mod emits or matches.
    /// </remarks>
    public static class JsonKeys
    {
        // Frame envelope
        public const string Type = "type";
        public const string Id = "id";
        public const string RefId = "ref_id";

        // Hello / Welcome
        public const string GameId = "game_id";
        public const string ClientVersion = "client_version";
        public const string LastSeenRedemptionId = "last_seen_redemption_id";
        public const string ServerTime = "server_time";
        public const string AssignedClientId = "assigned_client_id";
        public const string QueueDepth = "queue_depth";
        public const string InFlightRedemptionId = "in_flight_redemption_id";

        // EffectDispatch
        public const string RedemptionId = "redemption_id";
        public const string EffectSlug = "effect_slug";
        public const string EffectName = "effect_name";
        public const string UserId = "user_id";
        public const string UserDisplayName = "user_display_name";
        public const string DispatchedAt = "dispatched_at";
        public const string Attempts = "attempts";
        public const string Replay = "replay";

        // EffectResponse
        public const string Status = "status";
        public const string Reason = "reason";
        public const string Message = "message";
        public const string RetryAfterMs = "retry_after_ms";

        // EffectState
        public const string Available = "available";

        // GameState
        public const string Phase = "phase";
        public const string Details = "details";

        // Error
        public const string Code = "code";
    }
}
