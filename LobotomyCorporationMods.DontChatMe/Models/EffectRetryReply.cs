// SPDX-License-Identifier: MIT

#region

using System;
using System.Globalization;
using System.Text;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Constants;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Models
{
    /// <summary>
    ///     Outbound <c>effect_retry</c> frame: tells the chat-side server that this redemption
    ///     couldn't be applied right now but is worth resubmitting in <see cref="DelayMs" />
    ///     milliseconds with the same <see cref="RedemptionId" />. Used in place of
    ///     <c>effect_failed</c> for transient blocks like <c>game_not_ready</c> and
    ///     <c>overloaded</c>, so viewers don't immediately get refunded for a problem that's
    ///     about to resolve on its own.
    /// </summary>
    public sealed class EffectRetryReply : IEquatable<EffectRetryReply>
    {
        public EffectRetryReply(string redemptionId, int delayMs, string error)
        {
            ThrowHelper.ThrowIfNull(redemptionId, nameof(redemptionId));
            ThrowHelper.ThrowIfNull(error, nameof(error));
            if (delayMs < 0)
            {
                delayMs = 0;
            }

            RedemptionId = redemptionId;
            DelayMs = delayMs;
            Error = error;
        }

        public string RedemptionId { get; }

        public int DelayMs { get; }

        /// <summary>One of the retryable <see cref="ErrorTags" /> values.</summary>
        public string Error { get; }

        public string ToJson()
        {
            var sb = new StringBuilder(96);
            sb.Append("{\"");
            sb.Append(JsonKeys.Type);
            sb.Append("\":");
            JsonStringEscaper.AppendQuoted(sb, WireTypes.EffectRetry);
            sb.Append(",\"");
            sb.Append(JsonKeys.RedemptionId);
            sb.Append("\":");
            JsonStringEscaper.AppendQuoted(sb, RedemptionId);
            sb.Append(",\"");
            sb.Append(JsonKeys.DelayMs);
            sb.Append("\":");
            sb.Append(DelayMs.ToString(CultureInfo.InvariantCulture));
            sb.Append(",\"");
            sb.Append(JsonKeys.Error);
            sb.Append("\":");
            JsonStringEscaper.AppendQuoted(sb, Error);
            sb.Append('}');
            return sb.ToString();
        }

        public bool Equals(EffectRetryReply other)
        {
            if (other is null)
            {
                return false;
            }

            return RedemptionId == other.RedemptionId
                && DelayMs == other.DelayMs
                && Error == other.Error;
        }

        public override bool Equals(object obj) => Equals(obj as EffectRetryReply);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = RedemptionId.GetHashCode();
                hash = (hash * 397) ^ DelayMs;
                hash = (hash * 397) ^ Error.GetHashCode();
                return hash;
            }
        }
    }
}
