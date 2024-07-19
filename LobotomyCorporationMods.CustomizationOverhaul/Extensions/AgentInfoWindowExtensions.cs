// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using LobotomyCorporationMods.CustomizationOverhaul.UiComponents;
using UnityEngine;

namespace LobotomyCorporationMods.CustomizationOverhaul.Extensions
{
    internal static class AgentInfoWindowExtensions
    {
        internal static void CreateLoadPresetButton(this AgentInfoWindow agentInfoWindow)
        {
            if (!GameManager.currentGameManager.ManageStarted)
            {
                if (Harmony_Patch.Instance.LoadPresetButton == null)
                {
                    var uiButton = UiComponentFactory.CreateUiButton();
                    uiButton.Text = LocalizationIds.LoadPresetIconText.GetLocalized();
                    uiButton.SetParent(AgentInfoWindow.currentWindow.gameObject.transform.GetChild(0));
                    uiButton.SetPosition(PresetConstants.LoadPresetButtonPositionX, PresetConstants.LoadPresetButtonPositionY);
                    uiButton.SetSize(PresetConstants.ButtonSizeX, PresetConstants.ButtonSizeY);
                    uiButton.TextFont = DeployUI.instance.ordeal.font;
                    uiButton.TextFontSize = PresetConstants.ButtonTextFontSize;
                    uiButton.TextColor = PresetConstants.PresetTextColor;
                    uiButton.TextAlignment = TextAnchor.MiddleCenter;

                    uiButton.OnClick.AddListener(LoadPresetPanelActions.TogglePanelVisibility);
                    Harmony_Patch.Instance.LoadPresetButton = uiButton;
                }
                else
                {
                    Harmony_Patch.Instance.LoadPresetButton.SetActive(true);
                }
            }
            else
            {
                if (Harmony_Patch.Instance.LoadPresetButton == null)
                {
                    return;
                }

                Harmony_Patch.Instance.LoadPresetButton.SetActive(false);
            }
        }

        internal static void CreateSavePresetButton(this AgentInfoWindow agentInfoWindow)
        {
            if (!GameManager.currentGameManager.ManageStarted)
            {
                if (Harmony_Patch.Instance.SavePresetButton.IsNull())
                {
                    var uiButton = UiComponentFactory.CreateUiButton();
                    uiButton.Text = LocalizationIds.SavePresetIconText.GetLocalized();
                    uiButton.SetParent(AgentInfoWindow.currentWindow.gameObject.transform.GetChild(0));
                    uiButton.SetPosition(PresetConstants.SavePresetButtonPositionX, PresetConstants.SavePresetButtonPositionY);
                    uiButton.SetSize(PresetConstants.ButtonSizeX, PresetConstants.ButtonSizeY);
                    uiButton.TextFont = DeployUI.instance.ordeal.font;
                    uiButton.TextFontSize = PresetConstants.ButtonTextFontSize;
                    uiButton.TextColor = PresetConstants.PresetTextColor;
                    uiButton.TextAlignment = TextAnchor.MiddleCenter;

                    uiButton.OnClick.AddListener(() => Harmony_Patch.Instance.PresetSaver.SavePreset());
                    Harmony_Patch.Instance.SavePresetButton = uiButton;
                }
                else
                {
                    Harmony_Patch.Instance.SavePresetButton.SetActive(true);
                }
            }
            else
            {
                if (Harmony_Patch.Instance.SavePresetButton == null)
                {
                    return;
                }

                Harmony_Patch.Instance.SavePresetButton.SetActive(false);
            }
        }
    }
}
