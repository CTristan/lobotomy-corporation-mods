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
            CrowdControlResponseStatus status = CrowdControlResponseStatus.STATUS_SUCCESS,
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
            new CrowdControlResponse(0, CrowdControlResponseStatus.STATUS_KEEPALIVE).Send(socket);
        }

        internal void Send([NotNull] Socket socket)
        {
            Guard.Against.Null(socket, nameof(socket));

            var json = ToJson();

            if (Status != (int)CrowdControlResponseStatus.STATUS_KEEPALIVE)
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
        private string ToJson(int indentLevel = 0)
        {
            var indent = new string(' ', indentLevel * 2);
            var jsonBuilder = new StringBuilder($"{indent}{{{Environment.NewLine}");

            var nextIndent = new string(' ', (indentLevel + 1) * 2);
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(Id)}\": \"{Id}\",");
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(Code)}\": \"{Code}\",");
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(Message)}\": \"{Message}\",");
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(Status)}\": \"{Status}\",");
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(Type)}\": \"{Type}\",");

            jsonBuilder.Append($"{indent}}}");

            return jsonBuilder.ToString();
        }
    }
}
