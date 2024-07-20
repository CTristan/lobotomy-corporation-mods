// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces.UiComponents;
using LobotomyCorporationMods.CustomizationOverhaul.Implementations;
using LobotomyCorporationMods.CustomizationOverhaul.Interfaces;

#endregion

namespace LobotomyCorporationMods.CustomizationOverhaul
{
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch : HarmonyPatchBase
    {
        public new static readonly Harmony_Patch Instance = new Harmony_Patch(true);

        public Harmony_Patch() : this(false)
        {
        }

        private Harmony_Patch(bool initialize) : base(typeof(Harmony_Patch), "LobotomyCorporationMods.CustomizationOverhaul.dll", initialize)
        {
            PresetLoader = new PresetLoader(FileManager);
            PresetSaver = new PresetSaver(FileManager, PresetLoader);
        }

        internal IUiButton LoadPresetButton { get; set; }

        internal IUiImage LoadPresetPanel { get; set; }
        internal IUiButton SavePresetButton { get; set; }
        internal IPresetLoader PresetLoader { get; }
        internal IPresetSaver PresetSaver { get; }

        internal static void DisableAllCustomUiComponents()
        {
            if (Instance.LoadPresetButton.IsNotNull() && !Instance.LoadPresetButton.IsUnityNull())
            {
                Instance.LoadPresetButton.SetActive(false);
            }

            if (Instance.SavePresetButton.IsNotNull() && !Instance.SavePresetButton.IsUnityNull())
            {
                Instance.SavePresetButton.SetActive(false);
            }

            if (Instance.LoadPresetPanel.IsNotNull() && !Instance.LoadPresetPanel.IsUnityNull())
            {
                Instance.LoadPresetPanel.SetActive(false);
            }
        }
    }
}
