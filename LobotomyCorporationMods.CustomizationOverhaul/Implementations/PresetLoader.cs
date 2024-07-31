// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using LobotomyCorporationMods.CustomizationOverhaul.Interfaces;
using LobotomyCorporationMods.CustomizationOverhaul.Objects;
using SharpJson;
using UnityEngine;
using WorkerSprite;

namespace LobotomyCorporationMods.CustomizationOverhaul.Implementations
{
    internal sealed class PresetLoader : IPresetLoader
    {
        private readonly IFileManager _fileManager;

        internal PresetLoader([NotNull] IFileManager fileManager)
        {
            _fileManager = fileManager;
            InitializeAllPresetFiles();
        }

        public Dictionary<string, PresetData> Presets { get; } = new Dictionary<string, PresetData>();

        public IEnumerable<string> FindAllPresetFiles()
        {
            // Need to first make sure the Presets directory actually exists, and if not, then create it
            _fileManager.CreateDirectoryIfNotExists(PresetConstants.PresetsDirectoryName);

            return _fileManager.GetFilesFromDirectory(PresetConstants.PresetsDirectoryName, PresetConstants.JsonFileMask);
        }

        public bool HasPreset([NotNull] string agentName)
        {
            return Presets.ContainsKey(agentName);
        }

        public AgentData LoadPreset([NotNull] string agentName)
        {
            var customizingWindow = CustomizingWindow.CurrentWindow;
            var data = customizingWindow.CurrentData;

            if (!HasPreset(agentName))
            {
                return data;
            }

            var preset = Presets[agentName];

            // Load the preset into the currently customizing agent
            var spriteSet = GetSpriteSet(preset);
            var appearance = new Appearance
            {
                spriteSet = spriteSet,
                FrontHair = spriteSet.FrontHair,
                RearHair = spriteSet.RearHair,
                Eyebrow_Def = spriteSet.EyeBrow,
                Eyebrow_Battle = spriteSet.BattleEyeBrow,
                Eyebrow_Panic = spriteSet.PanicEyeBrow,
                Eye_Def = spriteSet.Eye,
                Eye_Panic = spriteSet.EyePanic,
                Eye_Dead = spriteSet.EyeDead,
                Mouth_Def = spriteSet.Mouth,
                Mouth_Battle = spriteSet.BattleMouth,
                Mouth_Panic = spriteSet.PanicMouth,
                HairColor = spriteSet.HairColor,
                EyeColor = spriteSet.EyeColor,

                // This is an issue with the original game because there is no sprite for Combat Eyes, so the original code does the same thing as this.
                Eye_Battle = spriteSet.BattleEyeBrow,
            };

            data.appearance = appearance;
            customizingWindow.portrait.SetCustomizing(data);

            return data;
        }

        public void InitializeDefaultCustomPresetFile()
        {
            var presetList = LoadPresetListFromFile(_fileManager.GetFile(PresetConstants.CustomFileName));
            foreach (var preset in presetList.Presets)
            {
                Presets[preset.Key] = preset.Value;
            }
        }

        public void ReloadPresetsFromFiles()
        {
            Presets.Clear();
            InitializeAllPresetFiles();
        }

        public bool IsExactPreset([NotNull] string agentName,
            Appearance appearance)
        {
            if (!HasPreset(agentName))
            {
                return false;
            }

            var preset = Presets[agentName];

            if (!CheckAllItemsHaveSameSprite(preset, appearance))
            {
                return false;
            }

            return IsSameSpriteName(preset.HairColor, appearance.HairColor.ToHtmlStringRgb()) && IsSameSpriteName(preset.EyeColor, appearance.EyeColor.ToHtmlStringRgb());
        }

        [NotNull]
        public PresetList LoadPresetsFromCustomFile([CanBeNull] string fileName = null)
        {
            var presetFile = _fileManager.GetFile(!string.IsNullOrEmpty(fileName) ? fileName : PresetConstants.CustomFileName);

            return LoadPresetListFromFile(presetFile);
        }

        private static bool CheckAllItemsHaveSameSprite([NotNull] PresetData preset,
            [NotNull] Appearance appearance)
        {
            var presetComparisonList = CreatePresetComparisonList(preset, appearance);
            var areAllItemsSameSpriteName = presetComparisonList.TrueForAll(presetComparison =>
                IsSameSpriteName(presetComparison.Key, presetComparison.Value == null ? string.Empty : presetComparison.Value.name));

            return areAllItemsSameSpriteName;
        }

        [NotNull]
        private static List<KeyValuePair<string, Sprite>> CreatePresetComparisonList([NotNull] PresetData presetData,
            [NotNull] Appearance appearance)
        {
            return new List<KeyValuePair<string, Sprite>>
            {
                CreatePair(presetData.FrontHair, appearance.FrontHair),
                CreatePair(presetData.RearHair, appearance.RearHair),
                CreatePair(presetData.EyebrowDef, appearance.Eyebrow_Def),
                CreatePair(presetData.EyebrowBattle, appearance.Eyebrow_Battle),
                CreatePair(presetData.EyebrowPanic, appearance.Eyebrow_Panic),
                CreatePair(presetData.EyeDef, appearance.Eye_Def),
                CreatePair(presetData.EyeBattle, appearance.Eye_Battle),
                CreatePair(presetData.EyePanic, appearance.Eye_Panic),
                CreatePair(presetData.EyeDead, appearance.Eye_Dead),
                CreatePair(presetData.MouthDef, appearance.Mouth_Def),
                CreatePair(presetData.MouthBattle, appearance.Mouth_Battle),
            };
        }

        private static KeyValuePair<string, Sprite> CreatePair(string preset,
            Sprite appearance)
        {
            return new KeyValuePair<string, Sprite>(preset, appearance);
        }

        private void InitializeAllPresetFiles()
        {
            var presetFiles = FindAllPresetFiles();

            foreach (var preset in presetFiles.Select(LoadPresetListFromFile).SelectMany(presetList => presetList.Presets))
            {
                Presets[preset.Key] = preset.Value;
            }

            // Reload the default preset file last so that it overrules the presets from other files
            InitializeDefaultCustomPresetFile();
        }


        private static bool IsSameSpriteName([NotNull] string currentValue,
            [CanBeNull] string newValue)
        {
            // If either value is null, it can't be compared anyway, so let's say it's good
            if (string.IsNullOrEmpty(currentValue) || string.IsNullOrEmpty(newValue))
            {
                return true;
            }

            var valuesAreEqual = currentValue.Equals(newValue, StringComparison.OrdinalIgnoreCase);

            return valuesAreEqual;
        }

        [NotNull]
        private static WorkerSprite.WorkerSprite GetSpriteSet([NotNull] PresetData preset)
        {
            if (!ColorUtility.TryParseHtmlString(preset.HairColor, out var hairColor))
            {
                hairColor = Color.white;
            }

            if (!ColorUtility.TryParseHtmlString(preset.EyeColor, out var eyeColor))
            {
                eyeColor = Color.white;
            }

            var workerSprite = new WorkerSprite.WorkerSprite
            {
                FrontHair = GetSpriteFromGameData(BasicSpriteRegion.HAIR_FRONT, preset.FrontHair),
                RearHair = GetSpriteFromGameData(BasicSpriteRegion.HAIR_REAR, preset.RearHair),
                EyeBrow = GetSpriteFromGameData(BasicSpriteRegion.EYEBROW, preset.EyebrowDef),
                BattleEyeBrow = GetSpriteFromGameData(BasicSpriteRegion.EYEBROW_BATTLE, preset.EyebrowBattle),
                PanicEyeBrow = GetSpriteFromGameData(BasicSpriteRegion.EYEBROW_PANIC, preset.EyebrowPanic),
                Eye = GetSpriteFromGameData(BasicSpriteRegion.EYE_DEFAULT, preset.EyeDef),
                EyePanic = GetSpriteFromGameData(BasicSpriteRegion.EYE_PANIC, preset.EyePanic),
                EyeDead = GetSpriteFromGameData(BasicSpriteRegion.EYE_DEAD, preset.EyeDead),
                Mouth = GetSpriteFromGameData(BasicSpriteRegion.MOUTH, preset.MouthDef),
                BattleMouth = GetSpriteFromGameData(BasicSpriteRegion.MOUTH_BATTLE, preset.MouthBattle),
                PanicMouth = GetSpriteFromGameData(BasicSpriteRegion.MOUTH_PANIC, preset.MouthPanic),
                HairColor = hairColor,
                EyeColor = eyeColor,
            };

            return workerSprite;
        }

        private static Sprite GetSpriteFromGameData(BasicSpriteRegion region,
            string spriteName)
        {
            // Get the list of sprites loaded into the game
            var workerBasicSpriteController = WorkerSpriteDataLoader.Loader.basic;

            workerBasicSpriteController.GetData(region, out var workerBasicSprite);
            return workerBasicSprite.GetAllSprites().Find(x => x.name == spriteName);
        }

        [NotNull]
        private PresetList LoadPresetListFromFile(string fileName)
        {
            var jsonData = LoadJsonDataFromFile(fileName);
            if (string.IsNullOrEmpty(jsonData))
            {
                return new SerializablePresetList().ToPresetList();
            }

            var presetData = DecodeJsonToPresetData(jsonData);
            if (presetData == null || presetData.Count == 0)
            {
                return new SerializablePresetList().ToPresetList();
            }

            var loadedPresetData = LoadPresetDataToSerializablePresetList(presetData);
            return loadedPresetData.ToPresetList();
        }

        private string LoadJsonDataFromFile(string fileName)
        {
            var file = _fileManager.GetFile(fileName);
            return _fileManager.ReadAllText(file, false);
        }

        [CanBeNull]
        private static Dictionary<string, object> DecodeJsonToPresetData(string jsonData)
        {
            var decodedText = JsonDecoder.DecodeText(jsonData);
            return decodedText as Dictionary<string, object>;
        }

        [NotNull]
        private static SerializablePresetList LoadPresetDataToSerializablePresetList([NotNull] Dictionary<string, object> presetData)
        {
            var loadedPresetData = new SerializablePresetList();
            foreach (var preset in presetData)
            {
                loadedPresetData.Presets[preset.Key] = preset.Value;
            }

            return loadedPresetData;
        }
    }
}
