// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.DontChatMe.Models.EffectMessages
{
    public sealed class EffectResponse : EffectMessage
    {
        public string EffectId { get; set; }
        public string Message { get; set; }
        public string RequestId { get; set; }
        public int? RetryHint { get; set; }
        public string Status { get; set; }

        public static EffectResponse FromJson(Dictionary<string, object> json)
        {
            Guard.Against.Null(json, nameof(json));

            return new EffectResponse
            {
                RequestId = json.TryGetString(nameof(RequestId)),
                EffectId = json.TryGetString(nameof(EffectId)),
                Status = json.TryGetString(nameof(Status)),
                Message = json.TryGetString(nameof(Message)),
                RetryHint = json.TryGetNullableInt(nameof(RetryHint))
            };
        }

        public string ToJson()
        {
            FieldDictionary.Add(nameof(RequestId), RequestId);
            FieldDictionary.Add(nameof(EffectId), EffectId);
            FieldDictionary.Add(nameof(Status), Status);
            FieldDictionary.Add(nameof(Message), Message);
            FieldDictionary.Add(nameof(RetryHint), RetryHint);

            return JsonExtensions.BuildJson(FieldDictionary);
        }
    }
}
