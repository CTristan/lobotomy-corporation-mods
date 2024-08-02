// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.ProjectNugway.Implementations;
using LobotomyCorporationMods.ProjectNugway.Interfaces;

#endregion

namespace LobotomyCorporationMods.ProjectNugway
{
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch : HarmonyPatchBase
    {
        public new static readonly Harmony_Patch Instance = new Harmony_Patch(true);

        public Harmony_Patch() : this(false)
        {
        }

        private Harmony_Patch(bool initialize) : base(typeof(Harmony_Patch), "LobotomyCorporationMods.ProjectNugway.dll", initialize)
        {
            UiController = new UiController();

            PresetLoader = new PresetLoader(FileManager);
            PresetLoader.ReloadPresetsFromFiles();

            PresetWriter = new PresetWriter(FileManager, PresetLoader, UiController);
        }

        internal IPresetLoader PresetLoader { get; }
        internal IPresetWriter PresetWriter { get; }
        internal IUiController UiController { get; }
    }
}
