// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.FreeCustomization.Constants;
using LobotomyCorporationMods.FreeCustomization.Interfaces;
using LobotomyCorporationMods.FreeCustomization.Objects;
using SharpJson;
using UnityEngine;
using WorkerSprite;

namespace LobotomyCorporationMods.FreeCustomization.Implementations
{
    internal sealed class PresetLoader : IPresetLoader
    {
        private const string PresetsDirectoryName = "Presets";
        private const string AllJsonFiles = "*.json";
        private readonly IFileManager _fileManager;

        internal PresetLoader(IFileManager fileManager)
        {
            _fileManager = fileManager;
            // InitializeAllPresetFiles();
        }

        public Dictionary<string, PresetData> Presets { get; } = new Dictionary<string, PresetData>();

        public bool IsExactPreset([NotNull] string agentName,
            Appearance appearance)
        {
            if (!HasPreset(agentName))
            {
                return false;
            }

            var preset = Presets[agentName];

            var presetCheckList = new List<bool>
            {
                Compare(preset.FrontHair, appearance.FrontHair?.name) && Compare(preset.RearHair, appearance.RearHair?.name) && Compare(preset.EyebrowDef, appearance.Eyebrow_Def?.name) &&
                Compare(preset.EyebrowBattle, appearance.Eyebrow_Battle?.name) && Compare(preset.EyebrowPanic, appearance.Eyebrow_Panic?.name) && Compare(preset.EyeDef, appearance.Eye_Def?.name) &&
                Compare(preset.EyeBattle, appearance.Eye_Battle?.name) && Compare(preset.EyePanic, appearance.Eye_Panic?.name) && Compare(preset.EyeDead, appearance.Eye_Dead?.name) &&
                Compare(preset.MouthDef, appearance.Mouth_Def?.name) && Compare(preset.MouthBattle, appearance.Mouth_Battle?.name) &&
                Compare(preset.HairColor, appearance.HairColor.ToHtmlStringRgb()) && Compare(preset.EyeColor, appearance.EyeColor.ToHtmlStringRgb()),
            };

            return presetCheckList.TrueForAll(isEqual => isEqual);
        }

        public bool HasPreset([NotNull] string agentName)
        {
            return Presets.ContainsKey(agentName);
        }

        [NotNull]
        public PresetList LoadPresetsFromDefaultCustomFile()
        {
            var defaultCustomFile = _fileManager.GetFile(PresetDefaults.DefaultCustomFileName);

            return LoadPresetListFromFile(defaultCustomFile);
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

            // Load the preset into the currently-customizing agent
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
            var presetList = LoadPresetListFromFile(_fileManager.GetFile(PresetDefaults.DefaultCustomFileName));
            foreach (var preset in presetList.Presets)
            {
                Presets[preset.Key] = preset.Value;
            }
        }

        private void InitializeAllPresetFiles()
        {
            // Need to first make sure the Presets directory actually exists, and if not then create it
            _fileManager.CreateDirectoryIfNotExists(PresetsDirectoryName);

            var presetFiles = _fileManager.GetFilesFromDirectory(PresetsDirectoryName, AllJsonFiles);

            foreach (var preset in presetFiles.Select(LoadPresetListFromFile).SelectMany(presetList => presetList.Presets))
            {
                Presets[preset.Key] = preset.Value;
            }

            // Reload the default preset file last so that it overrules the presets from other files
            InitializeDefaultCustomPresetFile();
        }

        private static bool Compare(string currentValue,
            string newValue)
        {
            // If either value is null, it can't be compared anyway so just say it's good
            if (currentValue.IsNull() || newValue.IsNull())
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
