// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Extensions;
using UnityEngine.UI;

namespace LobotomyCorporationMods.Common.Implementations.UiComponents
{
    public class UiImageWithText : Image
    {
        public UiImageWithText()
        {
            InitializeText();
        }

        protected Text Text { get; private set; }

        protected void InitializeText()
        {
            Text = gameObject.CreateNewTextObject();
        }
    }
}
