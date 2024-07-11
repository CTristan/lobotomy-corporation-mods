// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace LobotomyCorporationMods.FreeCustomization.Objects
{
    [Serializable]
    public class SerializablePresetList
    {
        public Dictionary<string, object> Presets { get; } = new Dictionary<string, object>();

        [NotNull]
        public PresetList ToPresetList()
        {
            var presetList = new PresetList();
            foreach (var kvp in Presets)
            {
                presetList.Presets[kvp.Key] = ToPresetData(kvp.Value);
            }

            return presetList;
        }

        [NotNull]
        private static PresetData ToPresetData([NotNull] object value)
        {
            return value is Dictionary<string, object> dictionary ? ToPresetData(dictionary) : throw new ArgumentException("The value must be a dictionary");
        }

        [NotNull]
        private static PresetData ToPresetData([NotNull] Dictionary<string, object> value)
        {
            return new PresetData
            {
                EyeBattle = value[nameof(PresetData.EyeBattle)].ToString(),
                EyebrowBattle = value[nameof(PresetData.EyebrowBattle)].ToString(),
                EyebrowDef = value[nameof(PresetData.EyebrowDef)].ToString(),
                EyebrowPanic = value[nameof(PresetData.EyebrowPanic)].ToString(),
                EyeDef = value[nameof(PresetData.EyeDef)].ToString(),
                EyePanic = value[nameof(PresetData.EyePanic)].ToString(),
                EyeDead = value[nameof(PresetData.EyeDead)].ToString(),
                RearHair = value[nameof(PresetData.RearHair)].ToString(),
                FrontHair = value[nameof(PresetData.FrontHair)].ToString(),
                MouthBattle = value[nameof(PresetData.MouthBattle)].ToString(),
                MouthDef = value[nameof(PresetData.MouthDef)].ToString(),
                MouthPanic = value[nameof(PresetData.MouthPanic)].ToString(),
                HairColor = ToPresetColor(value[nameof(PresetData.HairColor)]),
                EyeColor = ToPresetColor(value[nameof(PresetData.EyeColor)]),
            };
        }

        [NotNull]
        private static PresetColor ToPresetColor([NotNull] object value)
        {
            return value is Dictionary<string, object> dictionary ? ToPresetColor(dictionary) : throw new ArgumentException("The value must be a dictionary");
        }

        [NotNull]
        private static PresetColor ToPresetColor([NotNull] Dictionary<string, object> value)
        {
            var redValue = float.TryParse(value[nameof(PresetColor.Red)].ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var red) ? red : 0f;
            var greenValue = float.TryParse(value[nameof(PresetColor.Green)].ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var green) ? green : 0f;
            var blueValue = float.TryParse(value[nameof(PresetColor.Blue)].ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var blue) ? blue : 0f;
            var alphaValue = float.TryParse(value[nameof(PresetColor.Alpha)].ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var alpha) ? alpha : 0f;

            return new PresetColor
            {
                Red = redValue,
                Green = greenValue,
                Blue = blueValue,
                Alpha = alphaValue,
            };
        }
    }
}
