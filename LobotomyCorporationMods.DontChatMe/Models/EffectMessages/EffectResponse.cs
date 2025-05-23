// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.DontChatMe.Constants;

namespace LobotomyCorporationMods.DontChatMe.Models.EffectMessages
{
    public sealed class EffectResponse : WebSocketMessage
    {
        public string EffectId { get; set; }
        public string RequestId { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }

        public override string MessageType
        {
            get => MessageTypes.EffectResponse;
        }

        public static EffectResponse FromJson(Dictionary<string, object> json)
        {
            Guard.Against.Null(json, nameof(json));

            return new EffectResponse
            {
                RequestId = json.TryGetString(JsonKeys.RequestId),
                EffectId = json.TryGetString(JsonKeys.EffectId),
                Status = json.TryGetString(JsonKeys.Status),
                Message = json.TryGetString(JsonKeys.Message)
            };
        }

        protected override Dictionary<string, object> SerializeFields()
        {
            return new Dictionary<string, object>
            {
                [JsonKeys.RequestId] = RequestId,
                [JsonKeys.EffectId] = EffectId,
                [JsonKeys.Status] = Status,
                [JsonKeys.Message] = Message
            };
        }
    }
}
