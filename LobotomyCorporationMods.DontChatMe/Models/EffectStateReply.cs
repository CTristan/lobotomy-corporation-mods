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
    ///     Outbound <c>effect_state</c> frame: announces whether a single effect slug is
    ///     selectable right now, and (when not) which <see cref="ErrorTags" /> reason applies.
    ///     The chat-side server uses these to grey out effects that would currently fail so
    ///     viewers don't waste redemptions.
    /// </summary>
    public sealed class EffectStateReply : IEquatable<EffectStateReply>
    {
        public EffectStateReply(string slug, bool selectable, string reason)
        {
            ThrowHelper.ThrowIfNull(slug, nameof(slug));
            Slug = slug;
            Selectable = selectable;
            Reason = reason;
        }

        public string Slug { get; }

        public bool Selectable { get; }

        /// <summary>One of the <see cref="ErrorTags" /> strings when <see cref="Selectable" /> is false; <c>null</c> otherwise.</summary>
        public string Reason { get; }

        public string ToJson()
        {
            var sb = new StringBuilder(96);
            sb.Append("{\"");
            sb.Append(JsonKeys.Type);
            sb.Append("\":");
            JsonStringEscaper.AppendQuoted(sb, WireTypes.EffectState);
            sb.Append(",\"");
            sb.Append(JsonKeys.Slug);
            sb.Append("\":");
            JsonStringEscaper.AppendQuoted(sb, Slug);
            sb.Append(",\"");
            sb.Append(JsonKeys.Selectable);
            sb.Append("\":");
            sb.Append(Selectable ? "true" : "false");
            if (!Selectable && Reason != null)
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

            return Slug == other.Slug && Selectable == other.Selectable && Reason == other.Reason;
        }

        public override bool Equals(object obj) => Equals(obj as EffectStateReply);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = Slug.GetHashCode();
                hash = (hash * 397) ^ Selectable.GetHashCode();
                hash = (hash * 397) ^ (Reason?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
