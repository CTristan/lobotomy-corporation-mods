// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.CustomizationOverhaul.Implementations;
using LobotomyCorporationMods.CustomizationOverhaul.Interfaces;
using LobotomyCorporationMods.CustomizationOverhaul.UiComponents;
using UnityEngine;

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
            PresetWriter = new PresetWriter(FileManager, PresetLoader);
        }

        internal LoadPresetButton LoadPresetButton { get; set; }

        internal LoadPresetPanel LoadPresetPanel { get; set; }
        internal SavePresetButton SavePresetButton { get; set; }
        internal IPresetLoader PresetLoader { get; }
        internal IPresetWriter PresetWriter { get; }

        internal static void DisplayLoadPresetButton()
        {
            if (!GameManager.currentGameManager.ManageStarted)
            {
                if (Instance.LoadPresetButton.IsUnityNull())
                {
                    Instance.LoadPresetButton = new GameObject().AddComponent<LoadPresetButton>();
                }
                else
                {
                    Instance.LoadPresetButton.gameObject.SetActive(true);
                }
            }
            else
            {
                if (Instance.LoadPresetButton.IsUnityNull())
                {
                    return;
                }

                Instance.LoadPresetButton.gameObject.SetActive(false);
            }
        }

        internal static void DisplaySavePresetButton()
        {
            if (!GameManager.currentGameManager.ManageStarted)
            {
                if (Instance.SavePresetButton.IsUnityNull())
                {
                    Instance.SavePresetButton = new GameObject().AddComponent<SavePresetButton>();
                }
                else
                {
                    Instance.SavePresetButton.gameObject.SetActive(true);
                }
            }
            else
            {
                if (Instance.SavePresetButton.IsUnityNull())
                {
                    return;
                }

                Instance.SavePresetButton.gameObject.SetActive(false);
            }
        }

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
