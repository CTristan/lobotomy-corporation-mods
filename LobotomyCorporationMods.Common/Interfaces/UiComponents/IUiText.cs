// SPDX-License-Identifier: MIT

using UnityEngine;

namespace LobotomyCorporationMods.Common.Interfaces.UiComponents
{
    public interface IUiText : IUiComponent
    {
        string Text { get; set; }
        TextAnchor Alignment { get; set; }
        Color Color { get; set; }
        Font Font { get; set; }
        int FontSize { get; set; }

        void SetAnchor(float anchorX,
            float anchorY,
            float anchorMinX,
            float anchorMinY,
            float anchorMaxX,
            float anchorMaxY);

        void SetSize(float width,
            float height);
    }
}
