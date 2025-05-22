// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Extensions;

namespace LobotomyCorporationMods.DontChatMe.Models.EffectMessages
{
    public sealed class EffectContext : EffectMessage
    {
        public Dictionary<string, object> Parameters { get; } = new Dictionary<string, object>();

        public string RequestID { get; set; }
        public string Source { get; set; }

        public static EffectContext FromJson(Dictionary<string, object> json)
        {
            return new EffectContext
            {
                RequestID = json.TryGetString(nameof(RequestID)),
                Source = json.TryGetString(nameof(Source))
            };
        }

        public override string ToJson()
        {
            FieldDictionary.Add(nameof(RequestID), RequestID);
            FieldDictionary.Add(nameof(Source), Source);
            FieldDictionary.Add(nameof(Parameters), Parameters);

            return JsonExtensions.BuildJson(FieldDictionary);
        }
    }
}
