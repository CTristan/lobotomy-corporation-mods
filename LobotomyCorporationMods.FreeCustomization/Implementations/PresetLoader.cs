// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Linq;
using Customizing;
using JetBrains.Annotations;
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
            InitializeAllPresetFiles();
        }

        public Dictionary<string, PresetData> Presets { get; } = new Dictionary<string, PresetData>();

        public bool IsPreset([NotNull] string agentName)
        {
            return Presets.ContainsKey(agentName);
        }

        public void LoadPreset([NotNull] string agentName)
        {
            InitializeAllPresetFiles();

            if (!IsPreset(agentName))
            {
                return;
            }

            var preset = Presets[agentName];

            // Load the preset into the currently-customizing agent
            var customizingWindow = CustomizingWindow.CurrentWindow;
            var data = customizingWindow.CurrentData;
            var spriteSet = GetSpriteSet(preset);
            data.appearance = new Appearance
            {
                spriteSet = spriteSet,
                FrontHair = spriteSet.FrontHair,
                RearHair = spriteSet.RearHair,
                Eyebrow_Def = spriteSet.EyeBrow,
                Eyebrow_Battle = spriteSet.BattleEyeBrow,
                Eyebrow_Panic = spriteSet.PanicEyeBrow,
                Eye_Def = spriteSet.Eye,
                Eye_Battle = spriteSet.BattleEyeBrow,
                Eye_Panic = spriteSet.PanicEyeBrow,
                Eye_Dead = spriteSet.EyeDead,
                Mouth_Def = spriteSet.Mouth,
                Mouth_Battle = spriteSet.BattleMouth,
                Mouth_Panic = spriteSet.PanicMouth,
                HairColor = spriteSet.HairColor,
                EyeColor = spriteSet.EyeColor,
            };


            customizingWindow.portrait.SetCustomizing(data);
        }

        [NotNull]
        public PresetList LoadSerializablePresetsFromDefaultCustomFile()
        {
            var defaultCustomFile = _fileManager.GetFile(PresetDefaults.DefaultCustomFileName);

            return LoadPresetListFromFile(defaultCustomFile);
        }

        private static WorkerSprite.WorkerSprite GetSpriteSet(PresetData preset)
        {
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
                HairColor = preset.HairColor.ToColor(),
                EyeColor = preset.EyeColor.ToColor(),
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

        private void InitializeAllPresetFiles()
        {
            // Need to first make sure the Presets directory actually exists, and if not then create it
            _fileManager.CreateDirectoryIfNotExists(PresetsDirectoryName);

            var presetFiles = _fileManager.GetFilesFromDirectory(PresetsDirectoryName, AllJsonFiles);

            foreach (var preset in presetFiles.Select(LoadPresetListFromFile).SelectMany(presetList => presetList.Presets))
            {
                Presets[preset.Key] = preset.Value;
            }
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
