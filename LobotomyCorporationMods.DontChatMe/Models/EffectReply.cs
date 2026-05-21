// SPDX-License-Identifier: MIT

#region

using System;
using System.Text;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Constants;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Models
{
    /// <summary>
    ///     An outbound frame replying to a server-pushed effect dispatch.
    ///     One of three kinds: <c>dispatch_ack</c>, <c>effect_executed</c>, or <c>effect_failed</c>.
    /// </summary>
    public sealed class EffectReply
    {
        private EffectReply(ReplyKind kind, string redemptionId, string error)
        {
            Kind = kind;
            RedemptionId = redemptionId;
            Error = error;
        }

        public ReplyKind Kind { get; }

        public string RedemptionId { get; }

        /// <summary>Error tag for <c>effect_failed</c>; <c>null</c> for ack/executed.</summary>
        public string Error { get; }

        public static EffectReply Ack(string redemptionId)
        {
            ThrowHelper.ThrowIfNull(redemptionId, nameof(redemptionId));
            return new EffectReply(ReplyKind.DispatchAck, redemptionId, error: null);
        }

        public static EffectReply Executed(string redemptionId)
        {
            ThrowHelper.ThrowIfNull(redemptionId, nameof(redemptionId));
            return new EffectReply(ReplyKind.EffectExecuted, redemptionId, error: null);
        }

        public static EffectReply Failed(string redemptionId, string error)
        {
            ThrowHelper.ThrowIfNull(redemptionId, nameof(redemptionId));
            ThrowHelper.ThrowIfNull(error, nameof(error));
            return new EffectReply(ReplyKind.EffectFailed, redemptionId, error);
        }

        public string ToJson()
        {
            var sb = new StringBuilder(128);
            sb.Append("{\"");
            sb.Append(JsonKeys.Type);
            sb.Append("\":");
            JsonStringEscaper.AppendQuoted(sb, KindToWireType(Kind));
            sb.Append(",\"");
            sb.Append(JsonKeys.RedemptionId);
            sb.Append("\":");
            JsonStringEscaper.AppendQuoted(sb, RedemptionId);

            if (Kind == ReplyKind.EffectFailed)
            {
                sb.Append(",\"");
                sb.Append(JsonKeys.Error);
                sb.Append("\":");
                JsonStringEscaper.AppendQuoted(sb, Error);
            }

            sb.Append('}');
            return sb.ToString();
        }

        private static string KindToWireType(ReplyKind kind)
        {
            switch (kind)
            {
                case ReplyKind.DispatchAck:
                    return WireTypes.DispatchAck;
                case ReplyKind.EffectExecuted:
                    return WireTypes.EffectExecuted;
                case ReplyKind.EffectFailed:
                    return WireTypes.EffectFailed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
    }
}
