// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.CustomizationOverhaul.Extensions
{
    internal static class AgentInfoWindowExtensions
    {
        internal static void CreateSavePresetButton(this AgentInfoWindow agentInfoWindow)
        {
            if (GameManager.currentGameManager.state == GameState.STOP)
            {
                if (Harmony_Patch.Instance.SavePresetButton == null)
                {
                    var buttonGameObject = new GameObject("ModButton");
                    var image = buttonGameObject.AddComponent<Image>();
                    image.transform.SetParent(AgentInfoWindow.currentWindow.gameObject.transform.GetChild(0));
                    var button = buttonGameObject.AddComponent<Button>();
                    var texture2D = new Texture2D(2, 2);
                    var sprite = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
                    image.sprite = sprite;
                    image.rectTransform.sizeDelta = new Vector2(150f, 90f);
                    button.targetGraphic = image;
                    button.onClick.AddListener(() => Harmony_Patch.Instance.PresetSaver.SavePreset());
                    buttonGameObject.SetActive(true);
                    buttonGameObject.transform.localScale = new Vector2(1f, 1f);
                    buttonGameObject.transform.localPosition = new Vector2(PresetConstants.PresetButtonPositionX, PresetConstants.PresetButtonPositionY);

                    Harmony_Patch.Instance.SavePresetButton = button;

                    var textGameObject = new GameObject("SavePresetButtonText");
                    var text = textGameObject.AddComponent<Text>();
                    textGameObject.transform.SetParent(buttonGameObject.transform);
                    text.rectTransform.sizeDelta = Vector2.zero;
                    text.rectTransform.anchorMin = new Vector2(0.02f, 0.0f);
                    text.rectTransform.anchorMax = new Vector2(0.98f, 1f);
                    text.rectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
                    text.text = LocalizationIds.SavePresetIconText.GetLocalized();
                    text.font = DeployUI.instance.ordeal.font;
                    text.fontSize = PresetConstants.PresetButtonTextFontSize;
                    text.color = PresetConstants.PresetTextColor;
                    text.alignment = TextAnchor.MiddleCenter;
                    textGameObject.transform.localScale = new Vector3(1f, 1f);
                    textGameObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                    textGameObject.SetActive(true);

                    Harmony_Patch.Instance.SavePresetButtonText = text;
                }
                else
                {
                    Harmony_Patch.Instance.SavePresetButton.gameObject.SetActive(true);
                }
            }
            else
            {
                if (Harmony_Patch.Instance.SavePresetButton == null)
                {
                    return;
                }

                Harmony_Patch.Instance.SavePresetButton.gameObject.SetActive(false);
            }
        }

        internal static void CreateSavePresetButtonText(this AgentInfoWindow agentInfoWindow)
        {
            if (Harmony_Patch.Instance.SavePresetButton == null)
            {
                agentInfoWindow.CreateSavePresetButton();
            }

            var shouldActivateText = GameManager.currentGameManager.state == GameState.STOP;
            var textObjectExists = Harmony_Patch.Instance.SavePresetButtonText != null;

            if (!textObjectExists)
            {
                Harmony_Patch.Instance.SavePresetButtonText = shouldActivateText ? CreateTextGameObject() : null;
            }
            else
            {
                Harmony_Patch.Instance.SavePresetButtonText.gameObject.SetActive(shouldActivateText);
            }
        }

        [NotNull]
        private static Text CreateTextGameObject()
        {
            var textGameObject = new GameObject("SavePresetButtonText");
            var text = textGameObject.AddComponent<Text>();
            textGameObject.transform.SetParent(Harmony_Patch.Instance.SavePresetButton.transform);
            text.rectTransform.sizeDelta = Vector2.zero;
            text.rectTransform.anchorMin = new Vector2(0.02f, 0.0f);
            text.rectTransform.anchorMax = new Vector2(0.98f, 1f);
            text.rectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
            text.text = LocalizationIds.SavePresetIconText.GetLocalized();
            text.font = DeployUI.instance.ordeal.font;
            text.fontSize = PresetConstants.PresetButtonTextFontSize;
            text.color = PresetConstants.PresetTextColor;
            text.alignment = TextAnchor.MiddleCenter;
            textGameObject.transform.localScale = new Vector3(1f, 1f);
            textGameObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            textGameObject.SetActive(true);

            return text;
        }
    }
}
