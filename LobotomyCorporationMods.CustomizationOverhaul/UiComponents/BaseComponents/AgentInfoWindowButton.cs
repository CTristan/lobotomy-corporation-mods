// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using UnityEngine;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents.BaseComponents
{
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public class AgentInfoWindowButton : MonoBehaviour
    {
        internal ButtonWithText Handle { get; private set; }

        public void Awake()
        {
            try
            {
                Handle = UiFactory.CreateButtonWithText(transform, gameObject.name, string.Empty);
                Handle.Label.SetStyle(
                    color: UiComponentConstants.PresetTextColor,
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
