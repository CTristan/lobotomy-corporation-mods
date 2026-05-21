// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using LobotomyCorporationMods.CustomizationOverhaul.Interfaces;
using LobotomyCorporationMods.CustomizationOverhaul.UiComponents;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.CustomizationOverhaul.Implementations
{
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
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
                    LoadPresetButton.Button.SetActive(true);
                }
            }
            else
            {
                if (LoadPresetButton == null)
                {
                    return;
                }

                LoadPresetButton.Button.SetActive(false);
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
                    SavePresetButton.Button.SetActive(true);
                }
            }
            else
            {
                if (SavePresetButton == null)
                {
                    return;
                }

                SavePresetButton.Button.SetActive(false);
            }
        }

        public void DisableAllCustomUiComponents()
        {
            if (LoadPresetButton != null)
            {
                LoadPresetButton.Button.SetActive(false);
            }

            if (SavePresetButton != null)
            {
                SavePresetButton.Button.SetActive(false);
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

        public void UpdateSavePresetButtonText(string agentName, Appearance appearance)
        {
            _presetLoader.InitializeDefaultCustomPresetFile();
            SavePresetButton.Button.SetActive(true);
        }

        private void InitializeLoadPresetButton()
        {
            var button = InitializeButton("LoadPresetButton");
            var currentPos = button.Button.Transform.GameObject.localPosition;
            button.Button.RectTransform.AnchoredPosition = new Vector2(
                currentPos.x,
                UiComponentConstants.LoadPresetButtonPositionY
            );
            button.Label.Text = LocalizeTextDataModel.instance.GetText(
                LocalizationIds.LoadPresetIconText
            );
            button.Button.AddClickListener(() =>
                LoadButtonOnClick(Harmony_Patch.Instance.UiController)
            );

            LoadPresetButton = button;
        }

        private static void LoadButtonOnClick([NotNull] IUiController uiController)
        {
            Harmony_Patch.Instance.PresetLoader.ReloadPresetsFromFiles();

            if (uiController.LoadPresetPanel == null)
            {
                uiController.DisplayLoadPresetPanel();

                return;
            }

            uiController.LoadPresetPanel.gameObject.SetActive(
                !uiController.LoadPresetPanel.isActiveAndEnabled
            );
        }

        private void InitializeSavePresetButton()
        {
            var button = InitializeButton("SavePresetButton");
            var currentPos = button.Button.Transform.GameObject.localPosition;
            button.Button.RectTransform.AnchoredPosition = new Vector2(
                currentPos.x,
                UiComponentConstants.SavePresetButtonPositionY
            );
            button.Label.Text = LocalizeTextDataModel.instance.GetText(
                LocalizationIds.SavePresetIconText
            );
            button.Button.AddClickListener(() =>
                SaveButtonOnClick(Harmony_Patch.Instance.UiController)
            );

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

                uiController.LoadPresetPanel.gameObject.SetActive(false);
                uiController.LoadPresetPanel.gameObject.SetActive(true);
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);

                throw;
            }
        }

        [NotNull]
        private static ButtonWithText InitializeButton(string buttonName)
        {
            GetExistingGameObjectReferences();

            var parent = s_strengthenEmployeeButton.transform.parent;
            var composed = UiFactory.CreateButtonWithText(parent, buttonName, string.Empty);

            var srcImage = s_strengthenEmployeeButton.GetComponent<Image>();
            composed.Button.Sprite = srcImage.sprite;
            composed.Button.Interactable = s_strengthenEmployeeButton.interactable;
            composed.Button.GameObject.colors = s_strengthenEmployeeButton.colors;

            composed.Button.RectTransform.SizeDelta = new Vector2(
                composed.Button.RectTransform.Width,
                UiComponentConstants.ButtonSizeY
            );

            composed.Label.SetStyle(
                color: s_strengthenEmployeeButtonText.color,
                font: s_strengthenEmployeeButtonText.font,
                fontSize: s_strengthenEmployeeButtonText.fontSize,
                alignment: s_strengthenEmployeeButtonText.alignment
            );

            var borderGo = new GameObject("BottomBorder", typeof(RectTransform));
            borderGo.transform.SetParent(
                composed.Button.GameObject.transform,
                worldPositionStays: false
            );
            var border = borderGo.AddComponent<Image>();
            border.sprite = s_strengthenEmployeeButtonBorderImage.sprite;
            border.color = s_strengthenEmployeeButtonBorderImage.color;
            border.rectTransform.sizeDelta = s_strengthenEmployeeButtonBorderImage
                .rectTransform
                .sizeDelta;
            border.rectTransform.anchoredPosition = new Vector2(
                UiComponentConstants.PresetButtonBorderPositionX,
                UiComponentConstants.PresetButtonBorderPositionY
            );

            return composed;
        }

        private static void GetExistingGameObjectReferences()
        {
            if (s_strengthenEmployeeButton != null)
            {
                return;
            }

            s_strengthenEmployeeButton = AgentInfoWindow.currentWindow.EnforcenButton;
            s_strengthenEmployeeButtonText =
                s_strengthenEmployeeButton.GetComponentInChildren<Text>();
            s_strengthenEmployeeButtonBorderImage = GetStrengthenEmployeeButtonBorderImage(
                s_strengthenEmployeeButton
            );
        }

        private static Image GetStrengthenEmployeeButtonBorderImage(
            [NotNull] Button strengthenEmployeeButton
        )
        {
            const string BorderGameObjectName = "Line_lower";
            var borderImageTransform = strengthenEmployeeButton.transform.Find(
                BorderGameObjectName
            );

            return borderImageTransform.GetComponent<Image>();
        }
    }
}
