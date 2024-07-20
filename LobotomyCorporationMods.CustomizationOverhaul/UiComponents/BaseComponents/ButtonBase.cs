// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces.UiComponents;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents.BaseComponents
{
    internal abstract class ButtonBase : MonoBehaviour, IUiButton
    {
        private protected ButtonBase()
        {
            Button = UiComponentFactory.CreateUiButton();
        }

        protected IUiButton Button { get; set; }

        public bool IsActive => Button.IsActive;

        public T AddComponent<T>() where T : Component
        {
            return Button.AddComponent<T>();
        }

        public bool AnyComponentIsNull()
        {
            return Button.AnyComponentIsNull();
        }

        public void SetActive(bool value)
        {
            Button.SetActive(value);
        }

        public void SetParent(Transform parent)
        {
            Button.SetParent(parent);
        }

        public void SetPosition(float x,
            float y)
        {
            Button.SetPosition(x, y);
        }

        public Button.ButtonClickedEvent OnClick => Button.OnClick;

        public string Text
        {
            get =>
                Button.Text;
            set =>
                Button.Text = value;
        }

        public TextAnchor TextAlignment
        {
            get =>
                Button.TextAlignment;
            set =>
                Button.TextAlignment = value;
        }

        public Color TextColor
        {
            get =>
                Button.TextColor;
            set =>
                Button.TextColor = value;
        }

        public Font TextFont
        {
            get =>
                Button.TextFont;
            set =>
                Button.TextFont = value;
        }

        public int TextFontSize
        {
            get =>
                Button.TextFontSize;
            set =>
                Button.TextFontSize = value;
        }

        public float Width => Button.Width;
        public float Height => Button.Height;

        public void SetButtonImage(string imagePath)
        {
            Button.SetButtonImage(imagePath);
        }

        public void SetButtonImageColor(Color color)
        {
            Button.SetButtonImageColor(color);
        }

        public void SetScale(float x,
            float y)
        {
            Button.SetScale(x, y);
        }

        public void SetSize(float width,
            float height)
        {
            Button.SetSize(width, height);
        }

        public void SetTextAnchor(float anchorX,
            float anchorY,
            float anchorMinX,
            float anchorMinY,
            float anchorMaxX,
            float anchorMaxY)
        {
            Button.SetTextAnchor(anchorX, anchorY, anchorMinX, anchorMinY, anchorMaxX, anchorMaxY);
        }
    }
}
