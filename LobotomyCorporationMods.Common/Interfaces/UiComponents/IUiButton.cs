// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.Common.Interfaces.UiComponents
{
    public interface IUiButton : IUiComponent
    {
        Button.ButtonClickedEvent OnClick { get; }

        string Text { get; set; }
        TextAnchor TextAlignment { get; set; }
        Color TextColor { get; set; }
        Font TextFont { get; set; }
        int TextFontSize { get; set; }
        float Width { get; }
        float Height { get; }

        void SetButtonImage(string imagePath);
        void SetButtonImageColor(Color color);

        void SetScale(float x,
            float y);

        void SetSize(float width,
            float height);

        void SetTextAnchor(float anchorX,
            float anchorY,
            float anchorMinX,
            float anchorMinY,
            float anchorMaxX,
            float anchorMaxY);
    }

    public static class IUiButtonExtensions
    {
        public static bool IsUnityNull([CanBeNull] this IUiButton uiButton)
        {
            return uiButton == null || uiButton.AnyComponentIsNull();
        }
    }
}
