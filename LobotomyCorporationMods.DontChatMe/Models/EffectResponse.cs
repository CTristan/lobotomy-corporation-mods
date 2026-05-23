// SPDX-License-Identifier: MIT

#region

using System.Globalization;
using System.Text;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Constants;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Models
{
    /// <summary>
    ///     The single outbound reply to a server-pushed <c>effect_dispatch</c>. Exactly one is
    ///     sent per dispatch. The <see cref="Status" /> determines what the server's per-game
    ///     queue does next:
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="ResponseStatuses.Success" /> — head terminates, queue advances.
    ///         </item>
    ///         <item>
    ///             <see cref="ResponseStatuses.Failure" /> — head terminates with a refund.
    ///         </item>
    ///         <item>
    ///             <see cref="ResponseStatuses.Retry" /> — head holds; server resends after
    ///             <see cref="RetryAfterMs" /> (advisory, clamped to <c>[0, 30_000]</c> server-side).
    ///         </item>
    ///         <item>
    ///             <see cref="ResponseStatuses.Duplicate" /> — mod's idempotency cache says this
    ///             redemption already terminated; server advances as if successful.
    ///         </item>
    ///     </list>
    ///     The outbound frame's monotonic <c>id</c> is assigned by the transport at serialization
    ///     time, not by the dispatcher — so the model stays id-agnostic.
    /// </summary>
    public sealed class EffectResponse
    {
        /// <summary>Sentinel meaning "no <c>retry_after_ms</c> in the frame" (only emitted for retry status).</summary>
        public const int NoRetryAfter = -1;

        private EffectResponse(
            string redemptionId,
            string status,
            string reason,
            string message,
            int retryAfterMs
        )
        {
            RedemptionId = redemptionId;
            Status = status;
            Reason = reason;
            Message = message;
            RetryAfterMs = retryAfterMs;
        }

        public string RedemptionId { get; }

        /// <summary>One of the <see cref="ResponseStatuses" /> values.</summary>
        public string Status { get; }

        /// <summary>One of the <see cref="StandardErrors" /> strings; <c>null</c> for plain success.</summary>
        public string Reason { get; }

        /// <summary>Optional free-text detail surfaced for diagnostics; <c>null</c> when not set.</summary>
        public string Message { get; }

        /// <summary>Advisory resend delay for retry status; <see cref="NoRetryAfter" /> when not applicable.</summary>
        public int RetryAfterMs { get; }

        /// <summary>Plain success — the most common case.</summary>
        public static EffectResponse Success(string redemptionId)
        {
            ThrowHelper.ThrowIfNull(redemptionId, nameof(redemptionId));
            return new EffectResponse(
                redemptionId,
                ResponseStatuses.Success,
                reason: null,
                message: null,
                retryAfterMs: NoRetryAfter
            );
        }

        /// <summary>Hard failure — head will be refunded, no further attempts.</summary>
        public static EffectResponse Failure(string redemptionId, string reason)
        {
            ThrowHelper.ThrowIfNull(redemptionId, nameof(redemptionId));
            ThrowHelper.ThrowIfNull(reason, nameof(reason));
            return new EffectResponse(
                redemptionId,
                ResponseStatuses.Failure,
                reason,
                message: null,
                retryAfterMs: NoRetryAfter
            );
        }

        /// <summary>Transient block — server resends after <paramref name="retryAfterMs" /> (clamped to [0, 30000]).</summary>
        public static EffectResponse Retry(string redemptionId, string reason, int retryAfterMs)
        {
            ThrowHelper.ThrowIfNull(redemptionId, nameof(redemptionId));
            ThrowHelper.ThrowIfNull(reason, nameof(reason));
            if (retryAfterMs < 0)
            {
                retryAfterMs = 0;
            }

            return new EffectResponse(
                redemptionId,
                ResponseStatuses.Retry,
                reason,
                message: null,
                retryAfterMs
            );
        }

        /// <summary>
        ///     Idempotency-cache says this redemption already terminated in a prior session. The
        ///     server treats this as success for queue-advance purposes and will not refund.
        /// </summary>
        public static EffectResponse Duplicate(string redemptionId)
        {
            ThrowHelper.ThrowIfNull(redemptionId, nameof(redemptionId));
            return new EffectResponse(
                redemptionId,
                ResponseStatuses.Duplicate,
                StandardErrors.DuplicateRedemption,
                message: null,
                retryAfterMs: NoRetryAfter
            );
        }

        /// <summary>Serializes the frame, embedding the transport-assigned monotonic <paramref name="id" />.</summary>
        public string ToJson(int id)
        {
            var sb = new StringBuilder(160);
            sb.Append("{\"");
            sb.Append(JsonKeys.Type);
            sb.Append("\":\"");
            sb.Append(WireTypes.EffectResponse);
            sb.Append("\",\"");
            sb.Append(JsonKeys.Id);
            sb.Append("\":");
            sb.Append(id.ToString(CultureInfo.InvariantCulture));
            sb.Append(",\"");
            sb.Append(JsonKeys.RedemptionId);
            sb.Append("\":");
            JsonStringEscaper.AppendQuoted(sb, RedemptionId);
            sb.Append(",\"");
            sb.Append(JsonKeys.Status);
            sb.Append("\":");
            JsonStringEscaper.AppendQuoted(sb, Status);

            if (Reason != null)
            {
                sb.Append(",\"");
                sb.Append(JsonKeys.Reason);
                sb.Append("\":");
                JsonStringEscaper.AppendQuoted(sb, Reason);
            }

            if (Message != null)
            {
                sb.Append(",\"");
                sb.Append(JsonKeys.Message);
                sb.Append("\":");
                JsonStringEscaper.AppendQuoted(sb, Message);
            }

            if (RetryAfterMs != NoRetryAfter)
            {
                sb.Append(",\"");
                sb.Append(JsonKeys.RetryAfterMs);
                sb.Append("\":");
                sb.Append(RetryAfterMs.ToString(CultureInfo.InvariantCulture));
            }

            sb.Append('}');
            return sb.ToString();
        }
    }
}
