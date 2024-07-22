// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.CrowdControl.Extensions;

namespace LobotomyCorporationMods.CrowdControl.CrowdControl
{
    public sealed class CrowdControlRequest
    {
        private const int ByteBuffer = 4096;
        private const int TimeToWait = 5_000_000;
        internal string Code { get; private set; }
        internal int Id { get; private set; }
        internal string Type { get; private set; }
        internal bool IsKeepAlive => Id == 0 && Type == "255";

        [CanBeNull]
        internal static CrowdControlRequest Receive([NotNull] CrowdControlClient client,
            [NotNull] Socket socket)
        {
            Guard.Against.Null(client, nameof(client));
            Guard.Against.Null(socket, nameof(socket));

            var buf = new byte[ByteBuffer];
            var bytesRead = 0;
            var content = new StringBuilder();

            do
            {
                if (!client.IsRunning)
                {
                    return null;
                }

                if (socket.Poll(TimeToWait, SelectMode.SelectRead))
                {
                    bytesRead = socket.Receive(buf);
                    if (bytesRead < 0)
                    {
                        return null;
                    }

                    content.Append(Encoding.ASCII.GetString(buf));
                }
                else
                {
                    CrowdControlResponse.KeepAlive(socket);
                }
            } while (bytesRead == 0 || (bytesRead == ByteBuffer && buf[ByteBuffer - 1] != 0));

            Harmony_Patch.Instance.Logger.LogInfo($"Trying to deserialize this JSON: {content}");

            return DeserializeReceivedData(content.ToString());
        }

        [NotNull]
        private static CrowdControlRequest DeserializeReceivedData([NotNull] string content)
        {
            var decodedText = content.DecodeText();
            return FromJson(decodedText);
        }

        [NotNull]
        private static CrowdControlRequest FromJson([NotNull] Dictionary<string, object> value)
        {
            Guard.Against.Null(value, nameof(value));

            return new CrowdControlRequest
            {
                Id = value.TryGetValue("id", out var id) ? int.Parse(id.ToString(), CultureInfo.InvariantCulture) : 0,
                Code = value.TryGetValue("code", out var code) ? code.ToString() : string.Empty,
                Type = value.TryGetValue("type", out var type) ? type.ToString() : string.Empty,
            };
        }
    }
}
