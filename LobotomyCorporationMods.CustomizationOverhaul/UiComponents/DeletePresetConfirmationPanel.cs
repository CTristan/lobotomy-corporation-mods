// SPDX-License-Identifier: MIT

using System;
using System.Collections;
using LobotomyCorporationMods.Common.Implementations.UiComponents;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using LobotomyCorporationMods.CustomizationOverhaul.UiComponents.BaseComponents;
using UnityEngine;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents
{
    public class DeletePresetConfirmationPanel : AgentInfoWindowImage
    {
        private const float ExpandDuration = 0.1f; // Duration of the expansion in seconds

        private PresetSlotButton _callingSlotButton;
        private Vector2 _initialSizeDelta;

        public new void Awake()
        {
            try
            {
                base.Awake();

                Text.font = DeployUI.instance.ordeal.font;
                Text.fontSize = PresetConstants.ButtonTextFontSize;
                Text.color = PresetConstants.PresetTextColor;
                Text.alignment = TextAnchor.MiddleCenter;

                SetUpAcceptButton();
                SetUpCancelButton();
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);
                throw;
            }
        }

        private void SetUpAcceptButton()
        {
            var acceptButton = new GameObject().AddComponent<UiButton>();
            acceptButton.transform.SetParent(transform);
            var imagePath = Harmony_Patch.Instance.FileManager.GetFile(PresetConstants.PresetDeleteAcceptIconPath);
            acceptButton.SetButtonImage(imagePath);
            acceptButton.SetPosition(PresetConstants.AcceptDeletePresetButtonPositionX, PresetConstants.AcceptDeletePresetButtonPositionY);
            acceptButton.onClick.AddListener(ProcessDeletion);
        }

        private void ProcessDeletion()
        {
            _callingSlotButton.ProcessDeletion();
        }

        private void SetUpCancelButton()
        {
            var cancelButton = new GameObject().AddComponent<UiButton>();
            cancelButton.transform.SetParent(transform);
            var imagePath = Harmony_Patch.Instance.FileManager.GetFile(PresetConstants.PresetDeleteCancelIconPath);
            cancelButton.SetButtonImage(imagePath);
            cancelButton.SetPosition(PresetConstants.DeletePresetButtonPositionX, PresetConstants.DeletePresetButtonPositionY);
            cancelButton.onClick.AddListener(CancelDeletion);
        }

        private void CancelDeletion()
        {
            SwipeOut();
            _callingSlotButton.CancelDeletion();
        }

        public void SwipeIn(PresetSlotButton callingSlotButton)
        {
            // Set the reference to the PresetSlotButton we came from
            _callingSlotButton = callingSlotButton;

            // Get the parent RectTransform
            var parentRect = rectTransform.parent.GetComponent<RectTransform>();

            // Store the button's initial size and position
            _initialSizeDelta = rectTransform.sizeDelta;

            // Set the button's initial width to zero
            rectTransform.sizeDelta = new Vector2(0, _initialSizeDelta.y);

            // Calculate the desired width of the button
            var desiredWidth = parentRect.rect.width - rectTransform.anchoredPosition.x;

            // Start the expansion animation
            StartCoroutine(ExpandButton(desiredWidth));
        }

        private IEnumerator ExpandButton(float desiredWidth)
        {
            float elapsedTime = 0;
            while (elapsedTime < ExpandDuration)
            {
                elapsedTime += Time.deltaTime;

                // Calculate the current width by interpolating between the initial width and the desired width
                var currentWidth = Mathf.Lerp(0, desiredWidth, elapsedTime / ExpandDuration);

                // Update the button's width
                rectTransform.sizeDelta = new Vector2(currentWidth, _initialSizeDelta.y);

                // Adjust the button's position to keep the right edge anchored
                // No need to adjust the position here since we're expanding towards the left

                yield return null;
            }

            // Ensure the button reaches the final width
            rectTransform.sizeDelta = new Vector2(desiredWidth, _initialSizeDelta.y);
        }

        public void SwipeOut()
        {
            Text.text = string.Empty;

            // Get the parent RectTransform
            var parentRect = rectTransform.parent.GetComponent<RectTransform>();

            // Store the button's initial size and position
            _initialSizeDelta = rectTransform.sizeDelta;

            // Calculate the desired width of the button
            var desiredWidth = parentRect.rect.width - _initialSizeDelta.x;

            // Start the expansion animation
            StartCoroutine(ShrinkButton(desiredWidth));
        }

        private IEnumerator ShrinkButton(float desiredWidth)
        {
            float elapsedTime = 0;
            while (elapsedTime < ExpandDuration)
            {
                elapsedTime += Time.deltaTime;

                // Calculate the current width by interpolating between the initial width and the desired width
                var currentWidth = Mathf.Lerp(_initialSizeDelta.x, desiredWidth, elapsedTime / ExpandDuration);

                // Update the button's width
                rectTransform.sizeDelta = new Vector2(currentWidth, _initialSizeDelta.y);

                // Adjust the button's position to keep the right edge anchored
                // No need to adjust the position here since we're expanding towards the left

                yield return null;
            }

            // Ensure the button reaches the final width
            rectTransform.sizeDelta = new Vector2(desiredWidth, _initialSizeDelta.y);
            gameObject.SetActive(false);
        }
    }
}
