// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes.ValidCodeCoverageExceptionAttributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Implementations.UiComponents;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using UnityEngine;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents.BaseComponents
{
    [UiComponent]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public class AgentInfoWindowImage : UiImageWithText
    {
        public new void Awake()
        {
            try
            {
                base.Awake();
                Text.font = DeployUI.instance.ordeal.font;
                Text.fontSize = PresetConstants.ButtonTextFontSize;
                Text.color = PresetConstants.PresetTextColor;
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
