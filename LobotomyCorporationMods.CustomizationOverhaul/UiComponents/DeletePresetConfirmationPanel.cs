// SPDX-License-Identifier: MIT

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using LobotomyCorporationMods.CustomizationOverhaul.Implementations;
using LobotomyCorporationMods.CustomizationOverhaul.UiComponents.BaseComponents;
using UnityEngine;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents
{
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public class DeletePresetConfirmationPanel : AgentInfoWindowImage
    {
        private const float ExpandDuration = 0.2f;
        private ButtonWithText _acceptButton;
        private ButtonWithText _cancelButton;
        private PresetSlotButton _parentButton;

        public new void Awake()
        {
            try
            {
                base.Awake();

                Handle.Label.SetStyle(
                    color: Color.white,
                    font: DeployUI.instance.ordeal.font,
                    fontSize: UiComponentConstants.ButtonTextFontSize,
                    alignment: TextAnchor.MiddleCenter
                );
                Handle.Label.GameObject.resizeTextForBestFit = false;

                InitializeAcceptButton();
                InitializeCancelButton();
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);

                throw;
            }
        }

        internal void SetText(string text)
        {
            Handle.Label.Text = text;
        }

        private void InitializeAcceptButton()
        {
            var imagePath = Harmony_Patch.Instance.FileManager.GetFile(
                UiComponentConstants.AcceptDeletePresetIconPath
            );
            _acceptButton = UiFactory.CreateButtonWithText(transform, "AcceptButton", string.Empty);
            _acceptButton.Button.Sprite = SpriteLoader.LoadSpriteFromFile(imagePath);
            _acceptButton.Button.RectTransform.AnchoredPosition = new Vector2(
                UiComponentConstants.AcceptDeletePresetButtonPositionX,
                UiComponentConstants.AcceptDeletePresetButtonPositionY
            );
            _acceptButton.Button.AddClickListener(ProcessDeletion);
        }

        private void ProcessDeletion()
        {
            Handle.Label.Text = string.Empty;
            _parentButton.ProcessDeletion();
        }

        private void InitializeCancelButton()
        {
            var imagePath = Harmony_Patch.Instance.FileManager.GetFile(
                UiComponentConstants.CancelDeletePresetIconPath
            );
            _cancelButton = UiFactory.CreateButtonWithText(transform, "CancelButton", string.Empty);
            _cancelButton.Button.Sprite = SpriteLoader.LoadSpriteFromFile(imagePath);
            _cancelButton.Button.RectTransform.AnchoredPosition = new Vector2(
                UiComponentConstants.CancelDeletePresetButtonPositionX,
                UiComponentConstants.CancelDeletePresetButtonPositionY
            );
            _cancelButton.Button.AddClickListener(CancelDeletion);
        }

        private void CancelDeletion()
        {
            SwipeOut();
            _parentButton.CancelDeletion();
        }

        public void SwipeIn(PresetSlotButton parentButton, string text)
        {
            try
            {
                _parentButton = parentButton;
                Handle.Image.GameObject.fillAmount = 0f;

                StartCoroutine(SlowlyFillImage(text));
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);

                throw;
            }
        }

        private IEnumerator SlowlyFillImage(string text)
        {
            float elapsedTime = 0;
            while (elapsedTime < ExpandDuration)
            {
                elapsedTime += Time.deltaTime;

                Handle.Image.GameObject.fillAmount = Mathf.Lerp(
                    0f,
                    1f,
                    elapsedTime / ExpandDuration
                );

                yield return null;
            }

            Handle.Image.GameObject.fillAmount = 1f;

            Handle.Label.Text = text;
            _acceptButton.Button.SetActive(true);
            _cancelButton.Button.SetActive(true);
        }

        public void SwipeOut()
        {
            try
            {
                Handle.Label.Text = string.Empty;
                _acceptButton.Button.SetActive(false);
                _cancelButton.Button.SetActive(false);

                StartCoroutine(SlowlyEmptyImage());
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);

                throw;
            }
        }

        private IEnumerator SlowlyEmptyImage()
        {
            float elapsedTime = 0;
            while (elapsedTime < ExpandDuration)
            {
                elapsedTime += Time.deltaTime;

                Handle.Image.GameObject.fillAmount = Mathf.Lerp(
                    1f,
                    0f,
                    elapsedTime / ExpandDuration
                );

                yield return null;
            }

            Handle.Image.GameObject.fillAmount = 0f;
            gameObject.SetActive(false);
        }
    }
}
