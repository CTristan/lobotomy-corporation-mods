// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.Common.Implementations.UiComponents
{
    /// <inheritdoc />
    public class UiButtonWithText : UiButton
    {
        protected Text Text { get; private set; }

        public new void Awake()
        {
            base.Awake();

            Text = gameObject.CreateNewTextObject();
        }

        public void SetTextColor(Color color)
        {
            Text.color = color;
        }
    }
}
