// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.FreeCustomization.Implementations;
using LobotomyCorporationMods.FreeCustomization.Interfaces;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.FreeCustomization
{
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch : HarmonyPatchBase
    {
        public new static readonly Harmony_Patch Instance = new Harmony_Patch(true);

        public Harmony_Patch() : this(false)
        {
        }

        private Harmony_Patch(bool initialize) : base(typeof(Harmony_Patch), "LobotomyCorporationMods.FreeCustomization.dll", initialize)
        {
            PresetLoader = new PresetLoader(FileManager);
            PresetSaver = new PresetSaver(FileManager, PresetLoader);
        }

        internal IPresetLoader PresetLoader { get; }
        internal IPresetSaver PresetSaver { get; }
        internal Button SavePresetButton { get; set; }
        internal Text SavePresetButtonText { get; set; }
    }
}
