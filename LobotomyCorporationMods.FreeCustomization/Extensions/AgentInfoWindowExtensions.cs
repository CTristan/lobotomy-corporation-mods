// SPDX-License-Identifier: MIT

using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.FreeCustomization.Extensions
{
    public static class AgentInfoWindowExtensions
    {
        public static void CreateSavePresetButton(this AgentInfoWindow agentInfoWindow)
        {
            if (GameManager.currentGameManager.state == GameState.STOP)
            {
                if (Harmony_Patch.Instance.SavePresetButton == null)
                {
                    var gameObject1 = new GameObject("ModButton");
                    var image = gameObject1.AddComponent<Image>();
                    image.transform.SetParent(AgentInfoWindow.currentWindow.gameObject.transform.GetChild(0));
                    var button = gameObject1.AddComponent<Button>();
                    var texture2D = new Texture2D(2, 2);

                    var fileManager = Harmony_Patch.Instance.FileManager;
                    var imageFile = fileManager.GetFile("Assets/gift.png");
                    image.color = Color.green;

                    texture2D.LoadImage(fileManager.ReadAllBytes(imageFile));
                    var sprite = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
                    image.sprite = sprite;
                    image.rectTransform.sizeDelta = new Vector2(150f, 90f);
                    button.targetGraphic = image;
                    button.onClick.AddListener(() => Harmony_Patch.Instance.PresetSaver.SavePreset());
                    gameObject1.SetActive(true);
                    gameObject1.transform.localScale = new Vector2(1f, 1f);
                    gameObject1.transform.localPosition = new Vector2(595f, -490f);
                    var gameObject2 = new GameObject("InfoText");
                    var text = gameObject2.AddComponent<Text>();
                    gameObject2.transform.SetParent(gameObject1.transform);
                    text.rectTransform.sizeDelta = Vector2.zero;
                    text.rectTransform.anchorMin = new Vector2(0.02f, 0.0f);
                    text.rectTransform.anchorMax = new Vector2(0.98f, 1f);
                    text.rectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
                    text.text = LocalizeTextDataModel.instance.GetText("AgentDelete");
                    text.font = DeployUI.instance.ordeal.font;
                    text.fontSize = 30;
                    text.color = new Color(239f / 256f, 139f / 256f, 39f / 256f);
                    text.alignment = TextAnchor.MiddleCenter;
                    gameObject2.transform.localScale = new Vector3(1f, 1f);
                    gameObject2.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                    gameObject2.SetActive(true);
                    Harmony_Patch.Instance.SavePresetButton = button;
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
    }
}
