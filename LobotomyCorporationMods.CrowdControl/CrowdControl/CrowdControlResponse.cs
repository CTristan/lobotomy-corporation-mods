// SPDX-License-Identifier: MIT

using System;
using System.Net.Sockets;
using System.Text;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.CrowdControl.CrowdControl
{
    public class CrowdControlResponse
    {
        internal CrowdControlResponse(int id,
            CrowdControlResponseStatus status,
            string message = "")
        {
            Id = id;
            Code = "";
            Message = message;
            Type = 0;
            Status = (int)status;
        }

        public string Code { get; set; }
        public int Id { get; set; }
        public string Message { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }

        internal static void KeepAlive([NotNull] Socket socket)
        {
            new CrowdControlResponse(0, CrowdControlResponseStatus.NotReady).Send(socket);
        }

        internal void Send([NotNull] Socket socket)
        {
            Guard.Against.Null(socket, nameof(socket));

            var json = ToJson();

            if (Status != (int)CrowdControlResponseStatus.NotReady)
            {
                Harmony_Patch.Instance.Logger.LogInfo($"Trying to serialize this JSON: {json}");
            }

            var tmpData = Encoding.ASCII.GetBytes(json);
            var outData = new byte[tmpData.Length + 1];
            Buffer.BlockCopy(tmpData, 0, outData, 0, tmpData.Length);
            outData[tmpData.Length] = 0;
            socket.Send(outData);
        }

        [NotNull]
        private string ToJson()
        {
            var jsonBuilder = new StringBuilder("{");

            jsonBuilder.Append($"\"id\":{Id},");
            jsonBuilder.Append($"\"code\":\"{Code}\",");
            jsonBuilder.Append($"\"message\":\"{Message}\",");
            jsonBuilder.Append($"\"status\":{Status},");
            jsonBuilder.Append($"\"type\":{Type}");

            jsonBuilder.Append('}');

            return jsonBuilder.ToString();
        }
    }
}
