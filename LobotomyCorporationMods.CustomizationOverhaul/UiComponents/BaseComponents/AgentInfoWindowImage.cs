// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using UnityEngine;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents.BaseComponents
{
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public class AgentInfoWindowImage : MonoBehaviour
    {
        internal ImageWithText Handle { get; private set; }

        public void Awake()
        {
            try
            {
                Handle = UiFactory.CreateImageWithText(transform, gameObject.name, string.Empty);
                Handle.Label.SetStyle(
                    color: UiComponentConstants.PresetTextColor,
                    font: DeployUI.instance.ordeal.font,
                    fontSize: UiComponentConstants.ButtonTextFontSize,
                    alignment: TextAnchor.MiddleCenter
                );
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);

                throw;
            }
        }
    }
}
