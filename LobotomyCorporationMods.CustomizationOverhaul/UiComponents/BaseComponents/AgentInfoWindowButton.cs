// SPDX-License-Identifier: MIT

using System;
using LobotomyCorporationMods.Common.Implementations.UiComponents;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using UnityEngine;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents.BaseComponents
{
    public class AgentInfoWindowButton : UiButton
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
                Harmony_Patch.Instance.Logger.WriteException(exception);
                throw;
            }
        }
    }
}
