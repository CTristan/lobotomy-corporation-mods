// SPDX-License-Identifier:MIT

#region

using LobotomyCorporationMods.Common.Enums;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.UiComponents
{
    public static class UiFactory
    {
        private const int MaxNamePreviewLength = 16;

        private static void AddLayoutElement(GameObject gameObject, Vector2? preferredSize = null, float minHeight = 0)
        {
            var layoutElement = gameObject.AddComponent<LayoutElement>();
            if (preferredSize.HasValue)
            {
                layoutElement.preferredWidth = preferredSize.Value.x;
                layoutElement.preferredHeight = preferredSize.Value.y;
            }

            if (minHeight > 0)
            {
                layoutElement.minHeight = minHeight;
            }

            layoutElement.flexibleHeight = 0;
            layoutElement.flexibleWidth = 0;
        }

        private static void ApplyLayout(RectTransform rectTransform, Vector2 anchoredPosition, Vector2 size)
        {
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;
        }

        private static void ApplyStretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(6, 4);
            rectTransform.offsetMax = new Vector2(-6, -4);
        }

        public static Button CreateButton(
            Transform parent,
            string label,
            UnityAction onClick = null,
            UiLayoutMode layoutMode = UiLayoutMode.Absolute,
            Vector2 anchoredPosition = default,
            Vector2 size = default,
            int fontSize = 14,
            Font customFont = null)
        {
            var sanitizedName = SanitizeName(label);
            var buttonObject = CreateUiElement("Button_" + sanitizedName, parent);

            buttonObject.AddComponent<Image>().color = Color.gray;

            var button = buttonObject.AddComponent<Button>();
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            if (layoutMode == UiLayoutMode.Absolute)
            {
                ApplyLayout(buttonObject.GetComponent<RectTransform>(), anchoredPosition,
                    size == default ? new Vector2(160, 40) : size);
            }
            else
            {
                AddLayoutElement(buttonObject, new Vector2(160, 40));
            }

            var labelObject = CreateUiElement("Label", buttonObject.transform);
            labelObject.AddComponent<Text>();
            var labelText = labelObject.GetComponent<Text>();
            labelText.text = label;
            labelText.font = customFont ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelText.fontSize = fontSize;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = Color.white;
            labelText.horizontalOverflow = HorizontalWrapMode.Wrap;
            labelText.verticalOverflow = VerticalWrapMode.Truncate;

            ApplyStretch(labelObject.GetComponent<RectTransform>());

            return button;
        }

        public static GameObject CreateHorizontalGroup(
            Transform parent,
            string name,
            int spacing = 8,
            RectOffset padding = null,
            TextAnchor childAlignment = TextAnchor.MiddleLeft,
            bool expandWidth = false,
            bool expandHeight = true)
        {
            return CreateLayoutGroup(parent, name, false, spacing, padding, childAlignment, expandWidth, expandHeight);
        }

        public static InputField CreateLabeledInputField(
            Transform parent,
            string labelText,
            string defaultValue = "",
            string placeholderText = "",
            UiLayoutMode layoutMode = UiLayoutMode.Grouped,
            UnityAction<string> onValueChanged = null,
            Font customFont = null)
        {
            var container = CreateUiElement("InputField_" + SanitizeName(labelText), parent);

            if (layoutMode == UiLayoutMode.Grouped)
            {
                AddLayoutElement(container, new Vector2(300, 40)); // You can tweak this size
            }

            // Label
            var labelObject = CreateUiElement("Label", container.transform);
            var label = labelObject.AddComponent<Text>();
            label.text = labelText;
            label.font = customFont ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 14;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = Color.white;

            ApplyStretch(labelObject.GetComponent<RectTransform>());
            labelObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Input Background
            var inputBackground = CreateUiElement("InputBackground", container.transform);
            inputBackground.AddComponent<Image>().color = Color.white;

            var inputField = inputBackground.AddComponent<InputField>();
            var inputTextObj = CreateUiElement("Text", inputBackground.transform);
            var inputText = inputTextObj.AddComponent<Text>();
            inputText.font = customFont ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            inputText.fontSize = 14;
            inputText.color = Color.black;
            inputText.alignment = TextAnchor.MiddleLeft;
            inputText.supportRichText = false;
            inputField.textComponent = inputText;

            ApplyStretch(inputTextObj.GetComponent<RectTransform>());

            // Placeholder
            var placeholderObj = CreateUiElement("Placeholder", inputBackground.transform);
            var placeholder = placeholderObj.AddComponent<Text>();
            placeholder.text = placeholderText;
            placeholder.font = customFont ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            placeholder.fontSize = 14;
            placeholder.fontStyle = FontStyle.Italic;
            placeholder.color = new Color(0.5f, 0.5f, 0.5f);
            placeholder.alignment = TextAnchor.MiddleLeft;
            inputField.placeholder = placeholder;

            ApplyStretch(placeholderObj.GetComponent<RectTransform>());

            inputField.text = defaultValue;

            if (onValueChanged != null)
            {
                inputField.onValueChanged.AddListener(onValueChanged);
            }

            ApplyStretch(inputBackground.GetComponent<RectTransform>());
            inputBackground.AddComponent<LayoutElement>().preferredHeight = 30;

            return inputField;
        }

        private static GameObject CreateLayoutGroup(
            Transform parent,
            string name,
            bool isVertical,
            int spacing,
            RectOffset padding,
            TextAnchor childAlignment,
            bool expandWidth,
            bool expandHeight)
        {
            var layoutGroupObject = CreateUiElement(name, parent);

            var layoutGroup = isVertical
                ? (HorizontalOrVerticalLayoutGroup)layoutGroupObject.AddComponent<VerticalLayoutGroup>()
                : layoutGroupObject.AddComponent<HorizontalLayoutGroup>();

            layoutGroup.spacing = spacing;
            layoutGroup.padding = padding ?? new RectOffset(10, 10, 10, 10);
            layoutGroup.childAlignment = childAlignment;
            layoutGroup.childForceExpandWidth = expandWidth;
            layoutGroup.childForceExpandHeight = expandHeight;

            var contentSizeFitter = layoutGroupObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var rectTransform = layoutGroupObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = Vector2.zero;

            return layoutGroupObject;
        }

        public static GameObject CreateLayoutGroupWithBackground(
            Transform parent,
            string name,
            bool vertical,
            Color? backgroundColor = null)
        {
            // Create background panel
            var panel = CreateUiElement(name, parent);
            var image = panel.AddComponent<Image>();
            image.color = backgroundColor ?? new Color(0f, 0f, 0f, 0.5f); // Default: 50% transparent black

            // Add layout group
            if (vertical)
            {
                var layout = panel.AddComponent<VerticalLayoutGroup>();
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
            }
            else
            {
                var layout = panel.AddComponent<HorizontalLayoutGroup>();
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
            }

            // Add ContentSizeFitter
            var fitter = panel.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return panel;
        }

        public static Canvas CreateOverlayCanvas(string name)
        {
            var canvasObject = new GameObject(name);
            Object.DontDestroyOnLoad(canvasObject);

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            return canvas;
        }

        public static Text CreateText(
            Transform parent,
            string content,
            UiLayoutMode layoutMode = UiLayoutMode.Absolute,
            Vector2 anchoredPosition = default,
            Vector2 size = default,
            int fontSize = 14,
            Font customFont = null)
        {
            var sanitizedName = SanitizeName(content);
            var textObject = CreateUiElement("Text_" + sanitizedName, parent);

            var textComponent = textObject.AddComponent<Text>();
            textComponent.text = content;
            textComponent.font = customFont ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.UpperLeft;
            textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
            textComponent.verticalOverflow = VerticalWrapMode.Overflow;

            ApplyStretch(textObject.GetComponent<RectTransform>());

            if (layoutMode == UiLayoutMode.Absolute)
            {
                ApplyLayout(textObject.GetComponent<RectTransform>(), anchoredPosition,
                    size == default ? new Vector2(400, 100) : size);
            }
            else
            {
                AddLayoutElement(textObject, minHeight: 20);
            }

            return textComponent;
        }

        public static GameObject CreateUiElement(string name, Transform parent)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            gameObject.AddComponent<RectTransform>();
            return gameObject;
        }

        public static GameObject CreateVerticalGroup(
            Transform parent,
            string name,
            int spacing = 8,
            RectOffset padding = null,
            TextAnchor childAlignment = TextAnchor.UpperLeft,
            bool expandWidth = true,
            bool expandHeight = false)
        {
            return CreateLayoutGroup(parent, name, true, spacing, padding, childAlignment, expandWidth, expandHeight);
        }

        private static string SanitizeName(string rawName)
        {
            if (string.IsNullOrEmpty(rawName))
            {
                return "Unnamed";
            }

            var sanitizedName = rawName.Replace("\n", " ").Replace("\r", "").Trim();
            return sanitizedName.Length <= MaxNamePreviewLength
                ? sanitizedName
                : sanitizedName.Substring(0, MaxNamePreviewLength);
        }
    }
}
