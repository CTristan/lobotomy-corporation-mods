// SPDX-License-Identifier: MIT

using System;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
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
                onClick.AddListener(ActionsOnClick);

                var imagePath = Harmony_Patch.Instance.FileManager.GetFile(PresetConstants.PresetPanelImagePath);
                SetButtonImage(imagePath);
                image.SetSize(PresetConstants.ButtonSizeX, PresetConstants.ButtonSizeY);
                image.SetPosition(PresetConstants.SavePresetButtonPositionX, PresetConstants.SavePresetButtonPositionY);

                Text.text = LocalizationIds.SavePresetIconText.GetLocalized();
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);
                throw;
            }
        }

        private static void ActionsOnClick()
        {
            try
            {
                Harmony_Patch.Instance.PresetWriter.SavePreset();

                if (!Harmony_Patch.Instance.LoadPresetPanel.isActiveAndEnabled)
                {
                    return;
                }

                // Reset the Preset Panel to load in the new preset
                Harmony_Patch.Instance.LoadPresetPanel.gameObject.SetActive(false);
                Harmony_Patch.Instance.LoadPresetPanel.gameObject.SetActive(true);
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);
                throw;
            }
        }
    }
}
