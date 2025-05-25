// SPDX-License-Identifier:MIT

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LobotomyCorporationMods.Common.UiComponents
{
    public static class UiFactory
    {
        public static Button CreateButton(Transform parent, string text, Vector2 anchoredPosition, Vector2 size,
            UnityAction onClick)
        {
            var buttonObject = CreateUiElement("Button_" + text, parent);
            var buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = Color.gray;

            var button = buttonObject.AddComponent<Button>();
            button.onClick.AddListener(onClick);

            var buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.sizeDelta = size;
            buttonRect.anchoredPosition = anchoredPosition;

            var textObject = CreateUiElement("Text", buttonObject.transform);
            var buttonText = textObject.AddComponent<Text>();
            buttonText.text = text;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.resizeTextForBestFit = true;

            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        public static Canvas CreateOverlayCanvas(string name)
        {
            var canvasObject = new GameObject(name);
            Object.DontDestroyOnLoad(canvasObject);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // Ensure it's on top

            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static Text CreateText(Transform parent, string content, Vector2 anchoredPosition, int fontSize = 14)
        {
            var textObject = CreateUiElement("Text_" + content, parent);
            var textComponent = textObject.AddComponent<Text>();
            textComponent.text = content;
            textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.UpperLeft;

            var textRect = textObject.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(400, 100);
            textRect.anchoredPosition = anchoredPosition;

            return textComponent;
        }

        public static GameObject CreateUiElement(string name, Transform parent)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }
    }
}
