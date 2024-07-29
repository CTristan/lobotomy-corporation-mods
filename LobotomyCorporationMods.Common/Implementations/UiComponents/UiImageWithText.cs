// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Extensions;
using UnityEngine.UI;

namespace LobotomyCorporationMods.Common.Implementations.UiComponents
{
    public class UiImageWithText : Image
    {
        protected Text Text { get; private set; }

        public new void Awake()
        {
            base.Awake();

            Text = gameObject.CreateNewTextObject();
        }

        public void SetText(string text)
        {
            Text.text = text;
        }
    }
}
