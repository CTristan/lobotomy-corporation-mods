// SPDX-License-Identifier: MIT

using System;
using System.Text;
using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Facades;

namespace LobotomyCorporationMods.ProjectNugway.Objects
{
    [Serializable]
    public class PresetData
    {
        public string EyeBattle { get; set; }
        public string EyeDead { get; set; }
        public string EyeDef { get; set; }
        public string EyePanic { get; set; }
        public string EyebrowBattle { get; set; }
        public string EyebrowDef { get; set; }
        public string EyebrowPanic { get; set; }
        public string FrontHair { get; set; }
        public string MouthBattle { get; set; }
        public string MouthDef { get; set; }
        public string MouthPanic { get; set; }
        public string RearHair { get; set; }
        public string HairColor { get; set; }
        public string EyeColor { get; set; }

        [NotNull]
        internal static PresetData FromAppearanceData([NotNull] Appearance appearance)
        {
            Guard.Against.Null(appearance, nameof(appearance));

            return new PresetData
            {
                EyeBattle = appearance.Eye_Battle.GetSpriteName(),
                EyeDead = appearance.Eye_Dead.GetSpriteName(),
                EyeDef = appearance.Eye_Def.GetSpriteName(),
                EyePanic = appearance.Eye_Panic.GetSpriteName(),
                EyebrowBattle = appearance.Eyebrow_Battle.GetSpriteName(),
                EyebrowDef = appearance.Eyebrow_Def.GetSpriteName(),
                EyebrowPanic = appearance.Eyebrow_Panic.GetSpriteName(),
                FrontHair = appearance.FrontHair.GetSpriteName(),
                MouthBattle = appearance.Mouth_Battle.GetSpriteName(),
                MouthDef = appearance.Mouth_Def.GetSpriteName(),
                MouthPanic = appearance.Mouth_Panic.GetSpriteName(),
                RearHair = appearance.RearHair.GetSpriteName(),
                HairColor = appearance.HairColor.ToHtmlStringRgb(),
                EyeColor = appearance.EyeColor.ToHtmlStringRgb(),
            };
        }

        /// <summary>Serializes the PresetData object into a JSON string.</summary>
        /// <returns>A JSON string representation of the PresetData object.</returns>
        /// <remarks>Needed because Unity's JsonUtility does not support serializing properties.</remarks>
        [NotNull]
        public string ToJson(int indentLevel = 0)
        {
            var indent = new string(' ', indentLevel * 2);
            var jsonBuilder = new StringBuilder($"{indent}{{{Environment.NewLine}");

            var nextIndent = new string(' ', (indentLevel + 1) * 2);
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(EyeBattle)}\": \"{EyeBattle}\",");
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(EyeDead)}\": \"{EyeDead}\",");
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(EyeDef)}\": \"{EyeDef}\",");
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(EyePanic)}\": \"{EyePanic}\",");
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(EyebrowBattle)}\": \"{EyebrowBattle}\",");
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(EyebrowDef)}\": \"{EyebrowDef}\",");
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(EyebrowPanic)}\": \"{EyebrowPanic}\",");
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(FrontHair)}\": \"{FrontHair}\",");
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(MouthBattle)}\": \"{MouthBattle}\",");
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(MouthDef)}\": \"{MouthDef}\",");
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(MouthPanic)}\": \"{MouthPanic}\",");
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(RearHair)}\": \"{RearHair}\",");
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(HairColor)}\": \"{HairColor}\",");
            jsonBuilder.AppendLine($"{nextIndent}\"{nameof(EyeColor)}\": \"{EyeColor}\"");

            jsonBuilder.Append($"{indent}}}");

            return jsonBuilder.ToString();
        }
    }
}
