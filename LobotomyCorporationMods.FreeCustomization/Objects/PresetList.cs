// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace LobotomyCorporationMods.FreeCustomization.Objects
{
    [Serializable]
    public class PresetList
    {
        public Dictionary<string, PresetData> Presets { get; } = new Dictionary<string, PresetData>();

        [NotNull]
        public string ToJson()
        {
            const int InitialIndentLevel = 1;
            const int ValueIndentLevel = 2;
            var jsonBuilder = new StringBuilder($"{{{Environment.NewLine}");
            var first = true;
            foreach (var kvp in Presets)
            {
                if (!first)
                {
                    jsonBuilder.AppendLine(",");
                }
                else
                {
                    first = false;
                }

                var indent = new string(' ', InitialIndentLevel * 2);
                jsonBuilder.AppendLine($"{indent}\"{kvp.Key}\" : ");
                jsonBuilder.Append(kvp.Value.ToJson(ValueIndentLevel));
            }

            jsonBuilder.Append($"{Environment.NewLine}}}");

            return jsonBuilder.ToString();
        }
    }
}
