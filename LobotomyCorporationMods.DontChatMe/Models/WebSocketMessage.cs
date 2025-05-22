// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Extensions;

namespace LobotomyCorporationMods.DontChatMe.Models
{
    public abstract class WebSocketMessage
    {
        public abstract string MessageType { get; }

        protected abstract Dictionary<string, object> SerializeFields();

        public string ToJson()
        {
            var fields = SerializeFields();
            fields["messageType"] = MessageType; // or "type", your call
            return JsonExtensions.BuildJson(fields);
        }
    }
}
