// SPDX-License-Identifier: MIT

using System;
using System.IO;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces.UiComponents;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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

        public override bool AnyComponentIsNull()
        {
            var uiElements = new Object[]
            {
                GameObject, ButtonObject, ImageObject,
            };

            var uiElementIsNull = Array.Exists(uiElements, uiElement => uiElement.IsUnityNull());
            return uiElementIsNull || TextObject.AnyComponentIsNull();
        }

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

        public float Width => ImageObject.rectTransform.rect.width;

        public float Height => ImageObject.rectTransform.rect.height;

        public Button.ButtonClickedEvent OnClick => ButtonObject.onClick;

        public void SetButtonImage([NotNull] string imagePath)
        {
            var texture2D = new Texture2D(2, 2);

            if (!string.IsNullOrEmpty(imagePath))
            {
                texture2D.LoadImage(File.ReadAllBytes(imagePath));
            }

            var sprite = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height), new Vector2());
            ImageObject.sprite = sprite;
            ButtonObject.targetGraphic = ImageObject;
            SetSize(texture2D.width, texture2D.height);
        }

        public void SetButtonImageColor(Color color)
        {
            ImageObject.color = color;
        }

        public void SetScale(float x,
            float y)
        {
            ImageObject.rectTransform.localScale = new Vector3(x, y);
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
            SetButtonImage(string.Empty);
        }
    }
}
