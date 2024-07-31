// SPDX-License-Identifier: MIT

using System;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using LobotomyCorporationMods.CustomizationOverhaul.Interfaces;
using LobotomyCorporationMods.CustomizationOverhaul.UiComponents.BaseComponents;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents
{
    public sealed class SavePresetButton : AgentInfoWindowButton
    {
        public new void Awake()
        {
            try
            {
                base.Awake();

                transform.SetParent(AgentInfoWindow.currentWindow.gameObject.transform.GetChild(0));
                onClick.AddListener(() => ActionsOnClick(Harmony_Patch.Instance.UiController));

                var imagePath = Harmony_Patch.Instance.FileManager.GetFile(PresetConstants.PresetPanelImagePath);
                SetButtonImage(imagePath);
                image.SetSize(PresetConstants.ButtonSizeX, PresetConstants.ButtonSizeY);
                image.SetPosition(PresetConstants.SavePresetButtonPositionX, PresetConstants.SavePresetButtonPositionY);

                Text.text = LocalizationIds.SavePresetIconText.GetLocalized();
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.LogError(exception);
                throw;
            }
        }

        private static void ActionsOnClick([NotNull] IUiController uiController)
        {
            try
            {
                Harmony_Patch.Instance.PresetWriter.SavePreset();

                if (!uiController.LoadPresetPanel.gameObject.activeSelf)
                {
                    return;
                }

                // Reset the Preset Panel to load in the new preset
                uiController.LoadPresetPanel.gameObject.SetActive(false);
                uiController.LoadPresetPanel.gameObject.SetActive(true);
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.LogError(exception);
                throw;
            }
        }
    }
}
