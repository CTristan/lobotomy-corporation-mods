// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes.ValidCodeCoverageExceptionAttributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.UiComponents
{
    [UiComponent]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
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
