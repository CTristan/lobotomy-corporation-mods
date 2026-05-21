// SPDX-License-Identifier: MIT

#region

using System.Text;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Constants;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Models
{
    /// <summary>The first frame the mod sends after the WebSocket upgrades, before the server's <c>welcome</c>.</summary>
    public static class HelloFrame
    {
        public const string ClientName = "dont-chat-me";

        public static string Build(string token, string version)
        {
            ThrowHelper.ThrowIfNull(token, nameof(token));
            ThrowHelper.ThrowIfNull(version, nameof(version));

            var sb = new StringBuilder(128);
            sb.Append("{\"");
            sb.Append(JsonKeys.Type);
            sb.Append("\":\"");
            sb.Append(WireTypes.Hello);
            sb.Append("\",\"");
            sb.Append(JsonKeys.Token);
            sb.Append("\":");
            JsonStringEscaper.AppendQuoted(sb, token);
            sb.Append(",\"");
            sb.Append(JsonKeys.Client);
            sb.Append("\":");
            JsonStringEscaper.AppendQuoted(sb, ClientName);
            sb.Append(",\"");
            sb.Append(JsonKeys.Version);
            sb.Append("\":");
            JsonStringEscaper.AppendQuoted(sb, version);
            sb.Append('}');
            return sb.ToString();
        }

        public static string BuildPong()
        {
            return "{\"" + JsonKeys.Type + "\":\"" + WireTypes.Pong + "\"}";
        }
    }
}
