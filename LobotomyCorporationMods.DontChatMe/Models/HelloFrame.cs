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
    ///     The first frame the mod sends after the WebSocket upgrades, before the server's
    ///     <c>welcome</c>. Carries no auth material — the token rides in the
    ///     <c>Sec-WebSocket-Protocol</c> subprotocol negotiated at upgrade time.
    /// </summary>
    public static class HelloFrame
    {
        /// <summary>
        ///     Builds a <c>hello</c> frame. <paramref name="id" /> is the monotonic frame
        ///     identifier owned by the transport. <paramref name="lastSeenRedemptionId" />
        ///     is the most-recently terminal-acked dispatch from the prior session (or
        ///     <c>null</c> on a fresh connect) — the server uses it to resync the queue
        ///     head on reconnect.
        /// </summary>
        public static string Build(
            int id,
            int gameId,
            string clientVersion,
            string lastSeenRedemptionId
        )
        {
            ThrowHelper.ThrowIfNull(clientVersion, nameof(clientVersion));

            var sb = new StringBuilder(160);
            sb.Append("{\"");
            sb.Append(JsonKeys.Type);
            sb.Append("\":\"");
            sb.Append(WireTypes.Hello);
            sb.Append("\",\"");
            sb.Append(JsonKeys.Id);
            sb.Append("\":");
            sb.Append(id.ToString(CultureInfo.InvariantCulture));
            sb.Append(",\"");
            sb.Append(JsonKeys.GameId);
            sb.Append("\":");
            sb.Append(gameId.ToString(CultureInfo.InvariantCulture));
            sb.Append(",\"");
            sb.Append(JsonKeys.ClientVersion);
            sb.Append("\":");
            JsonStringEscaper.AppendQuoted(sb, clientVersion);
            if (!string.IsNullOrEmpty(lastSeenRedemptionId))
            {
                sb.Append(",\"");
                sb.Append(JsonKeys.LastSeenRedemptionId);
                sb.Append("\":");
                JsonStringEscaper.AppendQuoted(sb, lastSeenRedemptionId);
            }

            sb.Append('}');
            return sb.ToString();
        }
    }
}
