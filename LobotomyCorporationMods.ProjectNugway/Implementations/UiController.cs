// SPDX-License-Identifier: MIT

using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.UiComponents;
using LobotomyCorporationMods.ProjectNugway.Constants;
using LobotomyCorporationMods.ProjectNugway.Interfaces;
using LobotomyCorporationMods.ProjectNugway.UiComponents;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.ProjectNugway.Implementations
{
    internal sealed class UiController : IUiController
    {
        private readonly IPresetLoader _presetLoader;

        internal UiController(IPresetLoader presetLoader)
        {
            _presetLoader = presetLoader;
        }

        public ButtonWithText LoadPresetButton { get; private set; }
        public LoadPresetPanel LoadPresetPanel { get; private set; }
        public ButtonWithText SavePresetButton { get; private set; }

        public void DisplayLoadPresetButton()
        {
            if (!GameManager.currentGameManager.ManageStarted)
            {
                if (LoadPresetButton == null)
                {
                    InitializeLoadPresetButton();
                }
                else
                {
                    LoadPresetButton.gameObject.SetActive(true);
                }
            }
            else
            {
                if (LoadPresetButton == null)
                {
                    return;
                }

                LoadPresetButton.gameObject.SetActive(false);
            }
        }

        public void DisplaySavePresetButton()
        {
            if (!GameManager.currentGameManager.ManageStarted)
            {
                if (SavePresetButton == null)
                {
                    InitializeSavePresetButton();
                }
                else
                {
                    SavePresetButton.gameObject.SetActive(true);
                }
            }
            else
            {
                if (SavePresetButton == null)
                {
                    return;
                }

                SavePresetButton.gameObject.SetActive(false);
            }
        }

        public void DisableAllCustomUiComponents()
        {
            if (LoadPresetButton)
            {
                LoadPresetButton.gameObject.SetActive(false);
            }

            if (SavePresetButton)
            {
                SavePresetButton.gameObject.SetActive(false);
            }

            if (LoadPresetPanel)
            {
                LoadPresetPanel.gameObject.SetActive(false);
            }
        }

        public void DisplayLoadPresetPanel()
        {
            if (!GameManager.currentGameManager.ManageStarted)
            {
                if (LoadPresetPanel == null)
                {
                    LoadPresetPanel = new GameObject().AddComponent<LoadPresetPanel>();
                }
                else
                {
                    LoadPresetPanel.gameObject.SetActive(true);
                }
            }
            else
            {
                if (LoadPresetPanel == null)
                {
                    return;
                }

                LoadPresetPanel.gameObject.SetActive(false);
            }
        }

        public void UpdateSavePresetButtonText(string agentName,
            Appearance appearance)
        {
            _presetLoader.InitializeDefaultCustomPresetFile();
            SavePresetButton.gameObject.SetActive(true);
            // SavePresetButton.SetTextColor(Harmony_Patch.Instance.PresetLoader.IsExactPreset(agentName, appearance) ? Color.grey : UiComponentConstants.PresetTextColor);
        }

        private void InitializeLoadPresetButton()
        {
            var button = InitializeButton("LoadPresetButton");
            button.SetPosition(button.transform.position.x, UiComponentConstants.LoadPresetButtonPositionY);
            button.SetText(LocalizationIds.LoadPresetIconText.GetLocalized());

            LoadPresetButton = button;
        }

        private void InitializeSavePresetButton()
        {
            var button = InitializeButton("SavePresetButton");
            button.SetPosition(button.transform.position.x, UiComponentConstants.SavePresetButtonPositionY);
            button.SetText(LocalizationIds.SavePresetIconText.GetLocalized());

            SavePresetButton = button;
        }

        [NotNull]
        private static ButtonWithText InitializeButton(string buttonName)
        {
            var strengthenEmployeeButton = AgentInfoWindow.currentWindow.EnforcenButton;
            var strengthenEmployeeButtonText = strengthenEmployeeButton.GetComponentInChildren<Text>();

            var newButton = new GameObject(buttonName).AddComponent<ButtonWithText>();
            newButton.CopyButton(strengthenEmployeeButton);
            newButton.CopyText(strengthenEmployeeButtonText);

            // newButton.SetTextFont(DeployUI.instance.ordeal.font);
            // newButton.SetTextFontSize(UiComponentConstants.ButtonTextFontSize);
            // newButton.SetTextColor(UiComponentConstants.PresetTextColor);
            // newButton.SetTextAlignment(TextAnchor.MiddleCenter);

            var borderImagePath = Harmony_Patch.Instance.FileManager.GetFile(UiComponentConstants.PresetButtonBorderImagePath);
            var border = new GameObject("TopBorder").AddComponent<Image>();
            border.color = UiComponentConstants.PresetTextColor;
            border.transform.SetParent(newButton.image.transform);
            border.SetImage(borderImagePath);
            border.SetScale(UiComponentConstants.PresetButtonBorderScale);
            border.SetSize(UiComponentConstants.PresetButtonBorderSizeX, UiComponentConstants.PresetButtonBorderSizeY);
            border.SetPosition(UiComponentConstants.PresetButtonBorderPositionX, UiComponentConstants.LoadPresetButtonBorderTopPositionY);

            return newButton;
        }
    }
}
