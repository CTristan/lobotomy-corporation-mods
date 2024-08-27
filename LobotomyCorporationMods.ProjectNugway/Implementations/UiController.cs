// SPDX-License-Identifier: MIT

using System;
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
        private static Button s_strengthenEmployeeButton;
        private static Text s_strengthenEmployeeButtonText;
        private static Image s_strengthenEmployeeButtonBorderImage;
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
            button.SetPosition(button.transform.localPosition.x, UiComponentConstants.LoadPresetButtonPositionY);
            button.SetText(LocalizationIds.LoadPresetIconText.GetLocalized());
            button.onClick.AddListener(() => LoadButtonOnClick(Harmony_Patch.Instance.UiController));

            LoadPresetButton = button;
        }

        private static void LoadButtonOnClick([NotNull] IUiController uiController)
        {
            // Make sure that are presets are the most current whenever we click the load preset button
            Harmony_Patch.Instance.PresetLoader.ReloadPresetsFromFiles();

            if (uiController.LoadPresetPanel == null)
            {
                uiController.DisplayLoadPresetPanel();

                return;
            }

            uiController.LoadPresetPanel.gameObject.SetActive(!uiController.LoadPresetPanel.isActiveAndEnabled);
        }

        private void InitializeSavePresetButton()
        {
            var button = InitializeButton("SavePresetButton");
            button.SetPosition(button.transform.localPosition.x, UiComponentConstants.SavePresetButtonPositionY);
            button.SetText(LocalizationIds.SavePresetIconText.GetLocalized());
            button.onClick.AddListener(() => SaveButtonOnClick(Harmony_Patch.Instance.UiController));

            SavePresetButton = button;
        }

        private static void SaveButtonOnClick([NotNull] IUiController uiController)
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

        [NotNull]
        private static ButtonWithText InitializeButton(string buttonName)
        {
            GetExistingGameObjectReferences();

            var newButton = new GameObject(buttonName).AddComponent<ButtonWithText>();
            newButton.CopyButton(s_strengthenEmployeeButton);
            newButton.SetSize(newButton.Width, UiComponentConstants.ButtonSizeY);
            newButton.CopyText(s_strengthenEmployeeButtonText);

            var border = new GameObject("BottomBorder").AddComponent<Image>();
            border.CopyImage(s_strengthenEmployeeButtonBorderImage, true);
            border.transform.SetParent(newButton.image.transform);
            border.SetLocalPosition(UiComponentConstants.PresetButtonBorderPositionX, UiComponentConstants.PresetButtonBorderPositionY);

            return newButton;
        }

        private static void GetExistingGameObjectReferences()
        {
            if (s_strengthenEmployeeButton != null)
            {
                return;
            }

            s_strengthenEmployeeButton = AgentInfoWindow.currentWindow.EnforcenButton;
            s_strengthenEmployeeButtonText = s_strengthenEmployeeButton.GetComponentInChildren<Text>();
            s_strengthenEmployeeButtonBorderImage = GetStrengthenEmployeeButtonBorderImage(s_strengthenEmployeeButton);
        }

        /// <summary>Uses the button's transform to find the border image that we want.</summary>
        /// <param name="strengthenEmployeeButton"></param>
        /// <returns></returns>
        private static Image GetStrengthenEmployeeButtonBorderImage([NotNull] Button strengthenEmployeeButton)
        {
            const string BorderGameObjectName = "Line_lower";
            var borderImageTransform = strengthenEmployeeButton.transform.Find(BorderGameObjectName);

            return borderImageTransform.GetComponent<Image>();
        }
    }
}
