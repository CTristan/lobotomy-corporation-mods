// SPDX-License-Identifier: MIT

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes.ValidCodeCoverageExceptionAttributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations.UiComponents;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using LobotomyCorporationMods.CustomizationOverhaul.UiComponents.BaseComponents;
using UnityEngine;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents
{
    [UiComponent]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public class DeletePresetConfirmationPanel : AgentInfoWindowImage
    {
        private const float ExpandDuration = 0.2f; // Duration of the expansion in seconds
        private UiButton _acceptButton;
        private UiButton _cancelButton;
        private PresetSlotButton _parentButton;

        public new void Awake()
        {
            try
            {
                base.Awake();

                Text.font = DeployUI.instance.ordeal.font;
                Text.fontSize = PresetConstants.ButtonTextFontSize;
                Text.color = Color.white;
                Text.alignment = TextAnchor.MiddleCenter;
                Text.PreventTextFromResizing();

                InitializeAcceptButton();
                InitializeCancelButton();
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.LogError(exception);
                throw;
            }
        }


        private void InitializeAcceptButton()
        {
            var imagePath = Harmony_Patch.Instance.FileManager.GetFile(PresetConstants.AcceptDeletePresetIconPath);
            _acceptButton = new GameObject().AddComponent<UiButton>();
            _acceptButton.transform.SetParent(transform);
            _acceptButton.SetButtonImage(imagePath);
            _acceptButton.SetPosition(PresetConstants.AcceptDeletePresetButtonPositionX, PresetConstants.AcceptDeletePresetButtonPositionY);
            _acceptButton.onClick.AddListener(ProcessDeletion);
        }

        private void ProcessDeletion()
        {
            _parentButton.ProcessDeletion();
        }

        private void InitializeCancelButton()
        {
            var imagePath = Harmony_Patch.Instance.FileManager.GetFile(PresetConstants.CancelDeletePresetIconPath);
            _cancelButton = new GameObject().AddComponent<UiButton>();
            _cancelButton.transform.SetParent(transform);
            _cancelButton.SetButtonImage(imagePath);
            _cancelButton.SetPosition(PresetConstants.CancelDeletePresetButtonPositionX, PresetConstants.CancelDeletePresetButtonPositionY);
            _cancelButton.onClick.AddListener(CancelDeletion);
        }

        private void CancelDeletion()
        {
            SwipeOut();
            _parentButton.CancelDeletion();
        }

        public void SwipeIn(PresetSlotButton parentButton,
            string text)
        {
            // Set the reference to the PresetSlotButton we came from
            _parentButton = parentButton;
            fillAmount = 0f;

            // Start the expansion animation
            StartCoroutine(SlowlyFillImage(text));
        }

        private IEnumerator SlowlyFillImage(string text)
        {
            float elapsedTime = 0;
            while (elapsedTime < ExpandDuration)
            {
                elapsedTime += Time.deltaTime;

                fillAmount = Mathf.Lerp(0f, 1f, elapsedTime / ExpandDuration);

                yield return null;
            }

            fillAmount = 1f;

            // Add the text and buttons after animating
            Text.text = text;
            _acceptButton.gameObject.SetActive(true);
            _cancelButton.gameObject.SetActive(true);
        }

        public void SwipeOut()
        {
            // Empty the UI elements so that they don't look weird while trying to resize the text
            _acceptButton.gameObject.SetActive(false);
            _cancelButton.gameObject.SetActive(false);
            Text.text = string.Empty;

            // Start the expansion animation
            StartCoroutine(SlowlyEmptyImage());
        }

        private IEnumerator SlowlyEmptyImage()
        {
            float elapsedTime = 0;
            while (elapsedTime < ExpandDuration)
            {
                elapsedTime += Time.deltaTime;

                fillAmount = Mathf.Lerp(1f, 0f, elapsedTime / ExpandDuration);

                yield return null;
            }

            fillAmount = 0f;
            gameObject.SetActive(false);
        }
    }
}
