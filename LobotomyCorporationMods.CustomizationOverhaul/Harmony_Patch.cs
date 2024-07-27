// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.CustomizationOverhaul.Implementations;
using LobotomyCorporationMods.CustomizationOverhaul.Interfaces;
using LobotomyCorporationMods.CustomizationOverhaul.UiComponents;

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

        internal LoadPresetButton LoadPresetButton { get; set; }

        internal LoadPresetPanel LoadPresetPanel { get; set; }
        internal SavePresetButton SavePresetButton { get; set; }
        internal IPresetLoader PresetLoader { get; }
        internal IPresetSaver PresetSaver { get; }

        internal static void DisableAllCustomUiComponents()
        {
            if (!Instance.LoadPresetButton.IsUnityNull())
            {
                Instance.LoadPresetButton.gameObject.SetActive(false);
            }

            if (!Instance.SavePresetButton.IsUnityNull())
            {
                Instance.SavePresetButton.gameObject.SetActive(false);
            }

            if (!Instance.LoadPresetPanel.IsUnityNull())
            {
                Instance.LoadPresetPanel.gameObject.SetActive(false);
            }
        }
    }
}
