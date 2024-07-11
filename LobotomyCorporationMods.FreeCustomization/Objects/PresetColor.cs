// SPDX-License-Identifier: MIT

using System;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace LobotomyCorporationMods.FreeCustomization.Objects
{
    [Serializable]
    public class PresetColor
    {
        public float Red { get; set; }
        public float Green { get; set; }
        public float Blue { get; set; }
        public float Alpha { get; set; }

        [NotNull]
        internal static PresetColor FromColor(Color color)
        {
            return new PresetColor
            {
                Red = color.r,
                Green = color.g,
                Blue = color.b,
                Alpha = color.a,
            };
        }

        [NotNull]
        public Color ToColor()
        {
            return new Color(Red, Green, Blue, Alpha);
        }

        /// <summary>Serializes the PresetColor object into a JSON string.</summary>
        /// <returns>A JSON string representation of the PresetData object.</returns>
        /// <remarks>Needed because Unity's JsonUtility does not support serializing properties.</remarks>
        [NotNull]
        public string ToJson(int indentLevel = 2)
        {
            var indent = new string(' ', indentLevel * 2);
            var jsonBuilder = new StringBuilder($"{indent}{{{Environment.NewLine}");

            var nextIndent = new string(' ', (indentLevel + 1) * 2);
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(Red)}\": \"{Red.ToString(CultureInfo.InvariantCulture)}\",");
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(Green)}\": \"{Green.ToString(CultureInfo.InvariantCulture)}\",");
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(Blue)}\": \"{Blue.ToString(CultureInfo.InvariantCulture)}\",");
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(Alpha)}\": \"{Alpha.ToString(CultureInfo.InvariantCulture)}\"");

            jsonBuilder.Append($"{indent}}}");

            return jsonBuilder.ToString();
        }
    }
}
