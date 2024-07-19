// SPDX-License-Identifier: MIT

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

        void SetButtonGraphic();

        void SetSize(float width,
            float height);

        void SetTextAnchor(float anchorX,
            float anchorY,
            float anchorMinX,
            float anchorMinY,
            float anchorMaxX,
            float anchorMaxY);
    }
}
