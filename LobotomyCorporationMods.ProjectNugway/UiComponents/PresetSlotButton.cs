// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Customizing;
using LobotomyCorporationMods.Common.Attributes.ValidCodeCoverageExceptionAttributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations.UiComponents;
using LobotomyCorporationMods.ProjectNugway.Constants;
using LobotomyCorporationMods.ProjectNugway.UiComponents.BaseComponents;
using UnityEngine;

namespace LobotomyCorporationMods.ProjectNugway.UiComponents
{
    [UiComponent]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public class PresetSlotButton : AgentInfoWindowButton
    {
        private UiButton _deleteButton;
        private DeletePresetConfirmationPanel _deletePresetConfirmationPanel;
        private string _presetName;
        private UiPresetList _uiPresetList;

        public new void Awake()
        {
            try
            {
                base.Awake();

                var imagePath = Harmony_Patch.Instance.FileManager.GetFile(PresetConstants.PresetPanelImagePath);
                SetButtonImage(imagePath);

                InitializeDeleteButton();
                InitializeDeletePresetConfirmationPanel();
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.LogError(exception);

                throw;
            }
        }

        private void InitializeDeletePresetConfirmationPanel()
        {
            _deletePresetConfirmationPanel = new GameObject().AddComponent<DeletePresetConfirmationPanel>();
            _deletePresetConfirmationPanel.transform.SetParent(transform);
            _deletePresetConfirmationPanel.SetImage(Harmony_Patch.Instance.FileManager.GetFile(PresetConstants.DeletePresetPanelImagePath));
            _deletePresetConfirmationPanel.SetPosition(0.0f, 0.0f);
            _deletePresetConfirmationPanel.gameObject.SetActive(false);
        }

        private void InitializeDeleteButton()
        {
            if (_deleteButton == null)
            {
                _deleteButton = new GameObject().AddComponent<UiButton>();
                _deleteButton.transform.SetParent(transform);
                var imagePath = Harmony_Patch.Instance.FileManager.GetFile(PresetConstants.DeletePresetIconPath);
                _deleteButton.SetButtonImage(imagePath);
                _deleteButton.SetPosition(PresetConstants.DeletePresetButtonPositionX, PresetConstants.DeletePresetButtonPositionY);
                _deleteButton.onClick.AddListener(DisplayDeleteConfirmMessage);
            }
            else
            {
                _deleteButton.gameObject.SetActive(true);
            }
        }

        internal void ProcessDeletion()
        {
            try
            {
                Harmony_Patch.Instance.PresetWriter.DeletePreset(_presetName);
                Harmony_Patch.Instance.PresetLoader.ReloadPresetsFromFiles();
                _uiPresetList.UpdatePage();
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.LogError(exception);

                throw;
            }
        }

        internal void CancelDeletion()
        {
            try
            {
                InitializeDeleteButton();
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.LogError(exception);

                throw;
            }
        }

        private void DisplayDeleteConfirmMessage()
        {
            _deletePresetConfirmationPanel.gameObject.SetActive(true);

            var confirmationText = string.Format(CultureInfo.InvariantCulture, LocalizationIds.DeletePresetConfirmationText.GetLocalized(), _presetName);
            _deletePresetConfirmationPanel.SwipeIn(this, confirmationText);

            _deleteButton.gameObject.SetActive(false);
        }

        public void ClearButton()
        {
            try
            {
                Text.text = string.Empty;
                _deleteButton.gameObject.SetActive(false);
                _deletePresetConfirmationPanel.gameObject.SetActive(false);
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.LogError(exception);

                throw;
            }
        }

        public void UpdateButton(UiPresetList uiPresetList,
            int buttonNum,
            string presetName)
        {
            try
            {
                _presetName = presetName;
                Text.text = _presetName;

                image.SetPosition(0.0f, PresetConstants.LoadPresetPanelPositionY - buttonNum * Height);

                onClick.AddListener(delegate
                {
                    var loadedAgentData = Harmony_Patch.Instance.PresetLoader.LoadPreset(_presetName);

                    var instance = CustomizingWindow.CurrentWindow.appearanceUI;
                    instance.palette.OnSetColor(loadedAgentData.appearance.HairColor);
                    instance.SetAppearanceSprite(loadedAgentData);
                    instance.SetCreditControl(true);

                    Harmony_Patch.Instance.UiController.UpdateSavePresetButtonText(_presetName, loadedAgentData.appearance);
                });

                _deleteButton.gameObject.SetActive(true);

                if (_deletePresetConfirmationPanel.gameObject.activeSelf)
                {
                    _deletePresetConfirmationPanel.SetText(string.Empty);
                    _deletePresetConfirmationPanel.gameObject.SetActive(false);
                }

                _uiPresetList = uiPresetList;
            }
            catch (Exception e)
            {
                Harmony_Patch.Instance.Logger.LogError(e);

                throw;
            }
        }
    }
}
