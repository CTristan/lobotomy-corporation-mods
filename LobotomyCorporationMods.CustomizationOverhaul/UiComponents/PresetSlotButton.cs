// SPDX-License-Identifier: MIT

using System;
using System.Globalization;
using Customizing;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using LobotomyCorporationMods.CustomizationOverhaul.UiComponents.BaseComponents;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents
{
    public class PresetSlotButton : AgentInfoWindowButton
    {
        private Button _acceptButton;
        private DeletePresetButton _deleteButton;
        private bool _isDeleting;
        private string _presetName;
        private UiPresetList _uiPresetList;

        public new void Awake()
        {
            try
            {
                base.Awake();

                var imagePath = Harmony_Patch.Instance.FileManager.GetFile("Assets/preset-panel.png");
                SetButtonImage(imagePath);

                _deleteButton = new GameObject().AddComponent<DeletePresetButton>();
                _deleteButton.transform.SetParent(transform);
                _deleteButton.onClick.AddListener(ProcessDeleteButton);
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);
                throw;
            }
        }

        /// <summary>When clicking the Delete button, we want to display a confirmation message, and then the Delete button becomes a cancel button.</summary>
        /// <remarks>
        ///     This method is called when the Delete button is clicked. It checks the state of the button, and if it's in the "deleting" state, it cancels the deletion. Otherwise, it
        ///     displays a confirmation message to the user.
        /// </remarks>
        private void ProcessDeleteButton()
        {
            try
            {
                if (_isDeleting)
                {
                    CancelDeletion();
                    _acceptButton.gameObject.SetActive(false);
                }
                else
                {
                    DisplayDeleteConfirmMessage();
                    EnableAcceptButton();
                }

                _isDeleting = !_isDeleting;
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);
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
                    _acceptButton.image.SetImage(Harmony_Patch.Instance.FileManager.GetFile(PresetConstants.PresetPanelAcceptIconPath));
                    _acceptButton.image.SetPosition(PresetConstants.AcceptDeletePresetButtonPositionX, PresetConstants.AcceptDeletePresetButtonPositionY);
                    _acceptButton.onClick.AddListener(ProcessAcceptDeletionButton);
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

        private void ProcessAcceptDeletionButton()
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

        private void CancelDeletion()
        {
            Text.text = _presetName;
        }

        private void DisplayDeleteConfirmMessage()
        {
            Text.text = string.Format(CultureInfo.InvariantCulture, LocalizationIds.DeletePresetConfirmationText.GetLocalized(), _presetName);
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
