// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.DontChatMe.Constants;

namespace LobotomyCorporationMods.DontChatMe.Models.EffectMessages
{
    public sealed class EffectRequest : WebSocketMessage
    {
        public string EffectId { get; set; }
        public string RequestId { get; set; }

        public override string MessageType
        {
            get => MessageTypes.EffectRequest;
        }

        public static EffectRequest FromJson(Dictionary<string, object> json)
        {
            return new EffectRequest
            {
                EffectId = json.TryGetString(JsonKeys.EffectId),
                RequestId = json.TryGetString(JsonKeys.RequestId)
            };
        }

        protected override Dictionary<string, object> SerializeFields()
        {
            return new Dictionary<string, object>
            {
                [JsonKeys.EffectId] = EffectId,
                [JsonKeys.RequestId] = RequestId
            };
        }
    }
}
