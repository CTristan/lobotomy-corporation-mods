// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Customizing;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using LobotomyCorporationMods.CustomizationOverhaul.Implementations;
using LobotomyCorporationMods.CustomizationOverhaul.UiComponents.BaseComponents;
using UnityEngine;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents
{
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public class PresetSlotButton : AgentInfoWindowButton
    {
        private ButtonWithText _deleteButton;
        private DeletePresetConfirmationPanel _deletePresetConfirmationPanel;
        private string _presetName;
        private UiPresetList _uiPresetList;

        public new void Awake()
        {
            try
            {
                base.Awake();

                var imagePath = Harmony_Patch.Instance.FileManager.GetFile(
                    UiComponentConstants.PresetPanelImagePath
                );
                Handle.Button.Sprite = SpriteLoader.LoadSpriteFromFile(imagePath);

                InitializeDeleteButton();
                InitializeDeletePresetConfirmationPanel();
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);

                throw;
            }
        }

        private void InitializeDeletePresetConfirmationPanel()
        {
            _deletePresetConfirmationPanel =
                new GameObject().AddComponent<DeletePresetConfirmationPanel>();
            _deletePresetConfirmationPanel.transform.SetParent(transform);
            var panelPath = Harmony_Patch.Instance.FileManager.GetFile(
                UiComponentConstants.DeletePresetPanelImagePath
            );
            _deletePresetConfirmationPanel.Handle.Image.Sprite = SpriteLoader.LoadSpriteFromFile(
                panelPath
            );
            _deletePresetConfirmationPanel.Handle.Image.RectTransform.AnchoredPosition =
                new Vector2(0.0f, 0.0f);
            _deletePresetConfirmationPanel.gameObject.SetActive(false);
        }

        private void InitializeDeleteButton()
        {
            if (_deleteButton == null)
            {
                _deleteButton = UiFactory.CreateButtonWithText(
                    transform,
                    "DeleteButton",
                    string.Empty
                );
                var imagePath = Harmony_Patch.Instance.FileManager.GetFile(
                    UiComponentConstants.DeletePresetIconPath
                );
                _deleteButton.Button.Sprite = SpriteLoader.LoadSpriteFromFile(imagePath);
                _deleteButton.Button.RectTransform.AnchoredPosition = new Vector2(
                    UiComponentConstants.DeletePresetButtonPositionX,
                    UiComponentConstants.DeletePresetButtonPositionY
                );
                _deleteButton.Button.AddClickListener(DisplayDeleteConfirmMessage);
            }
            else
            {
                _deleteButton.Button.SetActive(true);
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
            try
            {
                InitializeDeleteButton();
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);

                throw;
            }
        }

        private void DisplayDeleteConfirmMessage()
        {
            _deletePresetConfirmationPanel.gameObject.SetActive(true);

            var confirmationText = string.Format(
                CultureInfo.InvariantCulture,
                LocalizeTextDataModel.instance.GetText(
                    LocalizationIds.DeletePresetConfirmationText
                ),
                _presetName
            );
            _deletePresetConfirmationPanel.SwipeIn(this, confirmationText);

            _deleteButton.Button.SetActive(false);
        }

        public void ClearButton()
        {
            try
            {
                Handle.Label.Text = string.Empty;
                _deleteButton.Button.SetActive(false);
                _deletePresetConfirmationPanel.gameObject.SetActive(false);
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);

                throw;
            }
        }

        public void UpdateButton(UiPresetList uiPresetList, int buttonNum, string presetName)
        {
            try
            {
                _presetName = presetName;
                Handle.Label.Text = _presetName;

                Handle.Button.RectTransform.AnchoredPosition = new Vector2(
                    0.0f,
                    UiComponentConstants.LoadPresetPanelPositionY
                        - buttonNum * Handle.Button.RectTransform.Height
                );

                Handle.Button.AddClickListener(
                    delegate
                    {
                        var loadedAgentData = Harmony_Patch.Instance.PresetLoader.LoadPreset(
                            _presetName
                        );

                        var instance = CustomizingWindow.CurrentWindow.appearanceUI;
                        instance.palette.OnSetColor(loadedAgentData.appearance.HairColor);
                        instance.SetAppearanceSprite(loadedAgentData);
                        instance.SetCreditControl(true);

                        Harmony_Patch.Instance.UiController.UpdateSavePresetButtonText(
                            _presetName,
                            loadedAgentData.appearance
                        );
                    }
                );

                _deleteButton.Button.SetActive(true);

                if (_deletePresetConfirmationPanel.gameObject.activeSelf)
                {
                    _deletePresetConfirmationPanel.SetText(string.Empty);
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
