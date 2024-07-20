// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using LobotomyCorporationMods.CustomizationOverhaul.UiComponents.BaseComponents;
using UnityEngine;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents
{
    internal sealed class LoadPresetButton : ButtonBase
    {
        internal LoadPresetButton()
        {
            Button = UiComponentFactory.CreateUiButton();
            Button.OnClick.AddListener(ActionsOnClick);
            Button.SetParent(AgentInfoWindow.currentWindow.gameObject.transform.GetChild(0));
            Button.SetPosition(PresetConstants.LoadPresetButtonPositionX, PresetConstants.LoadPresetButtonPositionY);

            var imagePath = Harmony_Patch.Instance.FileManager.GetFile("Assets/preset-panel.png");
            Button.SetButtonImage(imagePath);

            Button.SetSize(PresetConstants.ButtonSizeX, PresetConstants.ButtonSizeY);
            Button.Text = LocalizationIds.LoadPresetIconText.GetLocalized();
            Button.TextFont = DeployUI.instance.ordeal.font;
            Button.TextFontSize = PresetConstants.ButtonTextFontSize;
            Button.TextColor = PresetConstants.PresetTextColor;
            Button.TextAlignment = TextAnchor.MiddleCenter;
        }

        private static void ActionsOnClick()
        {
            if (Harmony_Patch.Instance.LoadPresetPanel.IsNull())
            {
                Harmony_Patch.Instance.LoadPresetPanel = new LoadPresetPanel();
                return;
            }

            Harmony_Patch.Instance.LoadPresetPanel.SetActive(!Harmony_Patch.Instance.LoadPresetPanel.IsActive);
        }
    }
}
