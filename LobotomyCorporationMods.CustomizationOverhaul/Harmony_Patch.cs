// SPDX-License-Identifier: MIT

#region
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.CustomizationOverhaul.Implementations;
using LobotomyCorporationMods.CustomizationOverhaul.Interfaces;

#endregion

namespace LobotomyCorporationMods.CustomizationOverhaul
{
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch : HarmonyPatchBase<Harmony_Patch>
    {
        public static readonly Harmony_Patch Instance = new Harmony_Patch(true);

        public Harmony_Patch() { }

        private Harmony_Patch(bool initialize)
            : base(initialize)
        {
            PresetLoader = new PresetLoader(FileManager);
            UiController = new UiController(PresetLoader);
            PresetWriter = new PresetWriter(FileManager, PresetLoader, UiController);
        }

        internal IPresetLoader PresetLoader { get; }
        internal IPresetWriter PresetWriter { get; }
        internal IUiController UiController { get; }
    }
}
