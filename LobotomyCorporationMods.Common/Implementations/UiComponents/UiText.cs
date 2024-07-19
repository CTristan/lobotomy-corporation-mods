// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Interfaces.UiComponents;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.Common.Implementations.UiComponents
{
    internal sealed class UiText : UiComponent, IUiText
    {
        internal UiText()
        {
            TextObject = GameObject.AddComponent<Text>();
        }

        private Text TextObject { get; }

        public Font Font
        {
            get =>
                TextObject.font;
            set =>
                TextObject.font = value;
        }

        public int FontSize
        {
            get =>
                TextObject.fontSize;
            set =>
                TextObject.fontSize = value;
        }

        public Color Color
        {
            get =>
                TextObject.color;
            set =>
                TextObject.color = value;
        }

        public string Text
        {
            get =>
                TextObject.text;
            set =>
                TextObject.text = value;
        }

        public TextAnchor Alignment
        {
            get =>
                TextObject.alignment;
            set =>
                TextObject.alignment = value;
        }

        public void SetAnchor(float anchorX,
            float anchorY,
            float anchorMinX,
            float anchorMinY,
            float anchorMaxX,
            float anchorMaxY)
        {
            TextObject.rectTransform.anchorMin = new Vector2(anchorMinX, anchorMinY);
            TextObject.rectTransform.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
            TextObject.rectTransform.anchoredPosition = new Vector2(anchorX, anchorY);
        }

        public void SetSize(float width,
            float height)
        {
            TextObject.rectTransform.sizeDelta = new Vector2(width, height);
        }
    }
}
