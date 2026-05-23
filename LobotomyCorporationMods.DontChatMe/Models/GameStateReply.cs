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
    ///     Outbound <c>game_state</c> frame: announces the current <see cref="GamePhases" /> value
    ///     so the chat-side server can show day/pause/meltdown context to viewers and apply the
    ///     game-wide queue gate (Case A — when phase ≠ <c>in_play</c> the entire per-game queue
    ///     pauses). Sent on phase transitions and on a slow heartbeat. The monotonic frame
    ///     <c>id</c> is assigned by the transport at serialization time.
    /// </summary>
    public sealed class GameStateReply : IEquatable<GameStateReply>
    {
        public GameStateReply(string phase)
        {
            ThrowHelper.ThrowIfNull(phase, nameof(phase));
            Phase = phase;
        }

        /// <summary>One of the <see cref="GamePhases" /> string constants.</summary>
        public string Phase { get; }

        public string ToJson(int id)
        {
            var sb = new StringBuilder(80);
            sb.Append("{\"");
            sb.Append(JsonKeys.Type);
            sb.Append("\":\"");
            sb.Append(WireTypes.GameState);
            sb.Append("\",\"");
            sb.Append(JsonKeys.Id);
            sb.Append("\":");
            sb.Append(id.ToString(CultureInfo.InvariantCulture));
            sb.Append(",\"");
            sb.Append(JsonKeys.Phase);
            sb.Append("\":");
            JsonStringEscaper.AppendQuoted(sb, Phase);
            sb.Append('}');
            return sb.ToString();
        }

        public bool Equals(GameStateReply other)
        {
            if (other is null)
            {
                return false;
            }

            return Phase == other.Phase;
        }

        public override bool Equals(object obj) => Equals(obj as GameStateReply);

        public override int GetHashCode() => Phase.GetHashCode();
    }
}
