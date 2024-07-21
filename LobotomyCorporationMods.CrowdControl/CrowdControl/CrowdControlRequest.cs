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

        [NotNull]
        internal static CrowdControlRequest Receive([NotNull] CrowdControlClient client,
            [NotNull] Socket socket)
        {
            Guard.Against.Null(client, nameof(client));
            Guard.Against.Null(socket, nameof(socket));

            var content = ReadDataFromSocket(client, socket);

            Harmony_Patch.Instance.Logger.LogInfo($"Trying to deserialize this JSON: {content}");

            return DeserializeReceivedData(content);
        }

        [NotNull]
        private static StringBuilder ReadDataFromSocket([NotNull] CrowdControlClient client,
            Socket socket)
        {
            var buffer = new byte[ByteBuffer];
            var content = new StringBuilder();

            ReceiveDataUntilFinished(client, socket, buffer, content);

            return content;
        }

        private static void ReceiveDataUntilFinished([NotNull] CrowdControlClient client,
            Socket socket,
            byte[] buffer,
            StringBuilder content)
        {
            int bytesRead;
            do
            {
                bytesRead = ReceiveDataFromClient(client, socket, buffer, content);
            } while (ShouldContinueReading(bytesRead, buffer));
        }

        private static int ReceiveDataFromClient([NotNull] CrowdControlClient client,
            Socket socket,
            byte[] buffer,
            StringBuilder content)
        {
            if (!client.IsRunning)
            {
                return -1;
            }

            if (socket.Poll(TimeToWait, SelectMode.SelectRead))
            {
                return ReceiveDataFromSocket(socket, buffer, content);
            }

            CrowdControlResponse.KeepAlive(socket);
            return 0;
        }

        private static bool ShouldContinueReading(int bytesRead,
            byte[] buffer)
        {
            return bytesRead == 0 || (bytesRead == ByteBuffer && buffer[ByteBuffer - 1] != 0);
        }

        private static int ReceiveDataFromSocket([NotNull] Socket socket,
            [NotNull] byte[] buffer,
            StringBuilder content)
        {
            var bytesRead = socket.Receive(buffer);

            if (bytesRead >= 0)
            {
                content.Append(Encoding.ASCII.GetString(buffer));
            }

            return bytesRead;
        }

        [NotNull]
        private static CrowdControlRequest DeserializeReceivedData([NotNull] StringBuilder content)
        {
            var decodedText = content.ToString().DecodeText();
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
