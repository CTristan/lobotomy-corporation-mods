// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes.ValidCodeCoverageExceptionAttributes;
using LobotomyCorporationMods.Common.Constants;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.UiComponents
{
    [UiComponent]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public class ImageWithText : Image
    {
        protected Text Text { get; private set; }

        public new void Awake()
        {
            base.Awake();

            Text = new GameObject("Text").AddComponent<Text>();
            Text.transform.SetParent(transform);
        }

        public void SetText(string text)
        {
            Text.text = text;
        }
    }
}
