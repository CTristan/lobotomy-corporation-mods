// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.FreeCustomization.Constants;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.FreeCustomization.Extensions
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
                    var fileManager = Harmony_Patch.Instance.FileManager;
                    var imageFile = fileManager.GetFile(PresetDefaults.SaveIcon);
                    image.color = Color.green;
                    texture2D.LoadImage(fileManager.ReadAllBytes(imageFile));
                    var sprite = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
                    image.sprite = sprite;
                    image.rectTransform.sizeDelta = new Vector2(150f, 90f);
                    button.targetGraphic = image;
                    button.onClick.AddListener(() => Harmony_Patch.Instance.PresetSaver.SavePreset());
                    buttonGameObject.SetActive(true);
                    buttonGameObject.transform.localScale = new Vector2(1f, 1f);
                    buttonGameObject.transform.localPosition = new Vector2(595f, -490f);

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
                    text.fontSize = 30;
                    text.color = new Color(239f / 256f, 139f / 256f, 39f / 256f);
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

            if (GameManager.currentGameManager.state == GameState.STOP)
            {
                if (Harmony_Patch.Instance.SavePresetButtonText == null)
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
                    text.fontSize = 30;
                    text.color = new Color(239f / 256f, 139f / 256f, 39f / 256f);
                    text.alignment = TextAnchor.MiddleCenter;
                    textGameObject.transform.localScale = new Vector3(1f, 1f);
                    textGameObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                    textGameObject.SetActive(true);

                    Harmony_Patch.Instance.SavePresetButtonText = text;
                }
                else
                {
                    Harmony_Patch.Instance.SavePresetButtonText.gameObject.SetActive(true);
                }
            }
            else
            {
                if (Harmony_Patch.Instance.SavePresetButtonText == null)
                {
                    return;
                }

                Harmony_Patch.Instance.SavePresetButtonText.gameObject.SetActive(false);
            }
        }
    }
}
