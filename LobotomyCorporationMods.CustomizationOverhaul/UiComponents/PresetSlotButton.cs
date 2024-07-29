// SPDX-License-Identifier: MIT

using System;
using System.Globalization;
using Customizing;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations.UiComponents;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using LobotomyCorporationMods.CustomizationOverhaul.UiComponents.BaseComponents;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents
{
    public class PresetSlotButton : AgentInfoWindowButton
    {
        private Button _acceptButton;
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

                SetUpDeleteButton();
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);
                throw;
            }
        }

        private void SetUpDeleteButton()
        {
            try
            {
                if (_deleteButton.IsUnityNull())
                {
                    _deleteButton = new GameObject().AddComponent<UiButton>();
                    _deleteButton.transform.SetParent(transform);
                    var imagePath = Harmony_Patch.Instance.FileManager.GetFile(PresetConstants.PresetDeleteIconPath);
                    _deleteButton.SetButtonImage(imagePath);
                    _deleteButton.SetPosition(PresetConstants.DeletePresetButtonPositionX, PresetConstants.DeletePresetButtonPositionY);
                    _deleteButton.onClick.AddListener(DisplayDeleteConfirmMessage);
                }
                else
                {
                    _deleteButton.gameObject.SetActive(true);
                }
            }
            catch (Exception e)
            {
                Harmony_Patch.Instance.Logger.WriteException(e);
                throw;
            }
        }

        private void EnableAcceptButton()
        {
            try
            {
                if (_acceptButton.IsUnityNull())
                {
                    _acceptButton = new GameObject().AddComponent<Button>();
                    _acceptButton.transform.SetParent(transform);
                    _acceptButton.image = _acceptButton.gameObject.AddComponent<Image>();
                    _acceptButton.image.SetImage(Harmony_Patch.Instance.FileManager.GetFile(PresetConstants.PresetDeleteAcceptIconPath));
                    _acceptButton.image.SetPosition(PresetConstants.AcceptDeletePresetButtonPositionX, PresetConstants.AcceptDeletePresetButtonPositionY);
                    _acceptButton.onClick.AddListener(ProcessDeletion);
                }
                else
                {
                    _acceptButton.gameObject.SetActive(true);
                }
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);
                throw;
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
                Harmony_Patch.Instance.Logger.WriteException(exception);
                throw;
            }
        }

        internal void CancelDeletion()
        {
            SetUpDeleteButton();
        }

        private void DisplayDeleteConfirmMessage()
        {
            if (_deletePresetConfirmationPanel.IsUnityNull())
            {
                _deletePresetConfirmationPanel = new GameObject().AddComponent<DeletePresetConfirmationPanel>();
                _deletePresetConfirmationPanel.transform.SetParent(transform);
                _deletePresetConfirmationPanel.SetImage(Harmony_Patch.Instance.FileManager.GetFile(PresetConstants.PresetPanelImagePath));
                _deletePresetConfirmationPanel.SetPosition(0.0f, 0.0f);
            }
            else
            {
                _deletePresetConfirmationPanel.gameObject.SetActive(true);
            }

            _deletePresetConfirmationPanel.SetText(string.Format(CultureInfo.InvariantCulture, LocalizationIds.DeletePresetConfirmationText.GetLocalized(), _presetName));
            _deletePresetConfirmationPanel.SwipeIn(this);

            _deleteButton.gameObject.SetActive(false);
        }

        public void ClearButton()
        {
            try
            {
                Text.text = string.Empty;
                _deleteButton.gameObject.SetActive(false);
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);
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

                    Harmony_Patch.Instance.PresetWriter.UpdateSavePresetButtonText(_presetName, loadedAgentData.appearance);
                });

                _deleteButton.gameObject.SetActive(true);

                if (!_acceptButton.IsUnityNull())
                {
                    _acceptButton.gameObject.SetActive(false);
                }

                if (!_deletePresetConfirmationPanel.IsUnityNull())
                {
                    _deletePresetConfirmationPanel.gameObject.SetActive(false);
                }

                _uiPresetList = uiPresetList;
            }
            catch (Exception e)
            {
                Harmony_Patch.Instance.Logger.WriteException(e);

                throw;
            }
        }
    }
}
