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
    ///     Outbound <c>effect_state</c> frame: announces whether a single effect slug is
    ///     currently available, and (when not) which <see cref="StandardErrors" /> reason
    ///     applies. The server's <c>Hemograce.Dispatch.GameQueue</c> uses these to gate the
    ///     queue head (Case B — per-effect scan-forward gate). The monotonic frame <c>id</c>
    ///     is assigned by the transport at serialization time.
    /// </summary>
    public sealed class EffectStateReply : IEquatable<EffectStateReply>
    {
        public EffectStateReply(string slug, bool available, string reason)
        {
            ThrowHelper.ThrowIfNull(slug, nameof(slug));
            Slug = slug;
            Available = available;
            Reason = reason;
        }

        public string Slug { get; }

        public bool Available { get; }

        /// <summary>One of the <see cref="StandardErrors" /> strings when <see cref="Available" /> is false; <c>null</c> otherwise.</summary>
        public string Reason { get; }

        public string ToJson(int id)
        {
            var sb = new StringBuilder(112);
            sb.Append("{\"");
            sb.Append(JsonKeys.Type);
            sb.Append("\":\"");
            sb.Append(WireTypes.EffectState);
            sb.Append("\",\"");
            sb.Append(JsonKeys.Id);
            sb.Append("\":");
            sb.Append(id.ToString(CultureInfo.InvariantCulture));
            sb.Append(",\"");
            sb.Append(JsonKeys.EffectSlug);
            sb.Append("\":");
            JsonStringEscaper.AppendQuoted(sb, Slug);
            sb.Append(",\"");
            sb.Append(JsonKeys.Available);
            sb.Append("\":");
            sb.Append(Available ? "true" : "false");
            if (!Available && Reason != null)
            {
                sb.Append(",\"");
                sb.Append(JsonKeys.Reason);
                sb.Append("\":");
                JsonStringEscaper.AppendQuoted(sb, Reason);
            }

            sb.Append('}');
            return sb.ToString();
        }

        public bool Equals(EffectStateReply other)
        {
            if (other is null)
            {
                return false;
            }

            return Slug == other.Slug && Available == other.Available && Reason == other.Reason;
        }

        public override bool Equals(object obj) => Equals(obj as EffectStateReply);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = Slug.GetHashCode();
                hash = (hash * 397) ^ Available.GetHashCode();
                hash = (hash * 397) ^ (Reason?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
