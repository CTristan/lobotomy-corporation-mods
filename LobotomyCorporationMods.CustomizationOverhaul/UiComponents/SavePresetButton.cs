// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using LobotomyCorporationMods.CustomizationOverhaul.UiComponents.BaseComponents;
using UnityEngine;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents
{
    internal sealed class SavePresetButton : ButtonBase
    {
        internal SavePresetButton()
        {
            Button = UiComponentFactory.CreateUiButton();
            Button.OnClick.AddListener(ActionsOnClick);
            Button.SetParent(AgentInfoWindow.currentWindow.gameObject.transform.GetChild(0));
            Button.SetPosition(PresetConstants.SavePresetButtonPositionX, PresetConstants.SavePresetButtonPositionY);

            var imagePath = Harmony_Patch.Instance.FileManager.GetFile("Assets/preset-panel.png");
            Button.SetButtonImage(imagePath);

            Button.SetSize(PresetConstants.ButtonSizeX, PresetConstants.ButtonSizeY);
            Button.Text = LocalizationIds.SavePresetIconText.GetLocalized();
            Button.TextFont = DeployUI.instance.ordeal.font;
            Button.TextFontSize = PresetConstants.ButtonTextFontSize;
            Button.TextColor = PresetConstants.PresetTextColor;
            Button.TextAlignment = TextAnchor.MiddleCenter;
        }

        private static void ActionsOnClick()
        {
            Harmony_Patch.Instance.PresetSaver.SavePreset();

            if (!Harmony_Patch.Instance.LoadPresetPanel.IsActive)
            {
                return;
            }

            // Reset the Preset Panel to load in the new preset
            Harmony_Patch.Instance.LoadPresetPanel.SetActive(false);
            Harmony_Patch.Instance.LoadPresetPanel.SetActive(true);
        }
    }
}
