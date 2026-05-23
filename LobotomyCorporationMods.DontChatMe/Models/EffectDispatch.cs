// SPDX-License-Identifier: MIT

#region

using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Constants;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Models
{
    /// <summary>
    ///     One inbound <c>effect_dispatch</c> frame from the chat-side server. Mirrors the
    ///     wire contract documented in <c>apps/hemograce_web/docs/game_mod_protocol.md</c>.
    /// </summary>
    public sealed class EffectDispatch
    {
        public EffectDispatch(
            string redemptionId,
            string effectSlug,
            string effectName,
            string userId,
            string userDisplayName,
            int gameId,
            string dispatchedAt,
            int attempts,
            bool replay
        )
        {
            ThrowHelper.ThrowIfNull(redemptionId, nameof(redemptionId));
            ThrowHelper.ThrowIfNull(effectSlug, nameof(effectSlug));
            RedemptionId = redemptionId;
            EffectSlug = effectSlug;
            EffectName = effectName;
            UserId = userId;
            UserDisplayName = userDisplayName;
            GameId = gameId;
            DispatchedAt = dispatchedAt;
            Attempts = attempts;
            Replay = replay;
        }

        public string RedemptionId { get; }
        public string EffectSlug { get; }
        public string EffectName { get; }
        public string UserId { get; }
        public string UserDisplayName { get; }
        public int GameId { get; }
        public string DispatchedAt { get; }

        /// <summary>1-based attempt counter; rises on each server-side retry of the same redemption.</summary>
        public int Attempts { get; }

        /// <summary>
        ///     <c>true</c> when the server resent this dispatch because a new socket registered
        ///     while it was still in-flight. The mod's idempotency cache dedupes via
        ///     <see cref="RedemptionId" /> so a replay is safe even if the prior attempt completed.
        /// </summary>
        public bool Replay { get; }

        /// <summary>
        ///     Tries to project a parsed JSON object into an <see cref="EffectDispatch" />.
        ///     Returns <c>false</c> when the required fields (<c>redemption_id</c>, <c>effect_slug</c>) are absent or blank.
        /// </summary>
        internal static bool TryFromJson(JsonDict json, out EffectDispatch dispatch)
        {
            dispatch = null;
            if (json == null)
            {
                return false;
            }

            var redemptionId = json.GetString(JsonKeys.RedemptionId);
            var effectSlug = json.GetString(JsonKeys.EffectSlug);
            if (string.IsNullOrEmpty(redemptionId) || string.IsNullOrEmpty(effectSlug))
            {
                return false;
            }

            dispatch = new EffectDispatch(
                redemptionId,
                effectSlug,
                json.GetString(JsonKeys.EffectName),
                json.GetString(JsonKeys.UserId),
                json.GetString(JsonKeys.UserDisplayName),
                json.GetInt(JsonKeys.GameId, defaultValue: 0),
                json.GetString(JsonKeys.DispatchedAt),
                json.GetInt(JsonKeys.Attempts, defaultValue: 1),
                json.GetBool(JsonKeys.Replay, defaultValue: false)
            );
            return true;
        }
    }
}
