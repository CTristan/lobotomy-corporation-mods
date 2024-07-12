// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
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
                HairColor = value[nameof(PresetData.HairColor)].ToString(),
                EyeColor = value[nameof(PresetData.EyeColor)].ToString(),
            };
        }
    }
}
