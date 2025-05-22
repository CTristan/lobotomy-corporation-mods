// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Extensions;

namespace LobotomyCorporationMods.DontChatMe.Models.EffectMessages
{
    public sealed class EffectRequest : EffectMessage
    {
        public string EffectId { get; set; }
        public string RequestId { get; set; }

        public static EffectRequest FromJson(Dictionary<string, object> json)
        {
            return new EffectRequest
            {
                EffectId = json.TryGetString(nameof(EffectId)),
                RequestId = json.TryGetString(nameof(RequestId))
            };
        }

        protected override Dictionary<string, object> SerializeFields()
        {
            return new Dictionary<string, object>
            {
                { nameof(EffectId), EffectId },
                { nameof(RequestId), RequestId }
            };
        }
    }
}
