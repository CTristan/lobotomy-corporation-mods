// SPDX-License-Identifier: MIT

using System;
using System.Security;
using Customizing;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.FreeCustomization.Extensions
{
    public static class CustomizingWindowExtensions
    {
        public static void SetAppearanceData(this CustomizingWindow customizingWindow, AgentModel agentModel, AgentInfoWindow agentInfoWindow)
        {
            try
            {
                Guard.Against.Null(customizingWindow, nameof(customizingWindow));
                Guard.Against.Null(agentModel, nameof(agentModel));
                Guard.Against.Null(agentInfoWindow, nameof(agentInfoWindow));

                customizingWindow.CurrentData.agentName = agentModel._agentName;
                customizingWindow.CurrentData.appearance = agentModel.GetAppearanceData();
                customizingWindow.CurrentData.CustomName = agentModel.name;
                agentInfoWindow.UIComponents.SetData(customizingWindow.CurrentData);
            }
            // Errors that only happen during testing, so we'll ignore them
            catch (SecurityException)
            {
                // Intentionally left blank
            }
            catch (MissingMemberException)
            {
                // Intentionally left blank
            }
        }
    }
}
