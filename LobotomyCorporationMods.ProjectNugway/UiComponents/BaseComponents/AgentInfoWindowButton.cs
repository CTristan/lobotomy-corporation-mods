// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes.ValidCodeCoverageExceptionAttributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.UiComponents;
using LobotomyCorporationMods.ProjectNugway.Constants;
using UnityEngine;

namespace LobotomyCorporationMods.ProjectNugway.UiComponents.BaseComponents
{
    [UiComponent]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public class AgentInfoWindowButton : ButtonWithText
    {
        public new void Awake()
        {
            try
            {
                base.Awake();

                Text.fontSize = UiComponentConstants.ButtonTextFontSize;
                Text.color = UiComponentConstants.PresetTextColor;
                Text.alignment = TextAnchor.MiddleCenter;
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.LogError(exception);

                throw;
            }
        }
    }
}
