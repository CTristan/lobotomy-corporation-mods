// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Interfaces.UiComponents;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.Common.Implementations.UiComponents
{
    internal sealed class UiButton : UiComponent, IUiButton
    {
        internal UiButton()
        {
            ButtonObject = GameObject.AddComponent<Button>();
            ImageObject = GameObject.AddComponent<Image>();
            TextObject = UiComponentFactory.CreateUiText();
            InitializeComponents();
        }

        private Button ButtonObject { get; }
        private Image ImageObject { get; }
        private IUiText TextObject { get; }

        public string Text
        {
            get =>
                TextObject.Text;
            set =>
                TextObject.Text = value;
        }

        public TextAnchor TextAlignment
        {
            get =>
                TextObject.Alignment;
            set =>
                TextObject.Alignment = value;
        }

        public Color TextColor
        {
            get =>
                TextObject.Color;
            set =>
                TextObject.Color = value;
        }

        public Font TextFont
        {
            get =>
                TextObject.Font;
            set =>
                TextObject.Font = value;
        }

        public int TextFontSize
        {
            get =>
                TextObject.FontSize;
            set =>
                TextObject.FontSize = value;
        }

        public Button.ButtonClickedEvent OnClick => ButtonObject.onClick;

        public void SetButtonGraphic()
        {
            var texture2D = new Texture2D(2, 2);
            var sprite = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height), new Vector2());
            ImageObject.sprite = sprite;
            ButtonObject.targetGraphic = ImageObject;
        }

        public void SetTextAnchor(float anchorX,
            float anchorY,
            float anchorMinX,
            float anchorMinY,
            float anchorMaxX,
            float anchorMaxY)
        {
            TextObject.SetAnchor(anchorX, anchorY, anchorMinX, anchorMinY, anchorMaxX, anchorMaxY);
        }

        public void SetSize(float width,
            float height)
        {
            ImageObject.rectTransform.sizeDelta = new Vector2(width, height);
        }

        private void InitializeComponents()
        {
            TextObject.SetParent(GameObject.transform);
            TextObject.SetSize(0f, 0f);
            SetTextAnchor(0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f);
            SetButtonGraphic();
        }
    }
}
