// SPDX-License-Identifier: MIT

#region

using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    public sealed class AgentInfoWindowAdapter : IAgentInfoWindowAdapter
    {
        public void OpenAppearanceWindow([NotNull] AgentInfoWindow agentInfoWindow)
        {
            agentInfoWindow.NotNull(nameof(agentInfoWindow));

            var customizingWindow = CustomizingWindow.CurrentWindow;

            agentInfoWindow.customizingBlock.SetActive(true);
            agentInfoWindow.AppearanceActiveControl.SetActive(true);
            agentInfoWindow.UIComponents.SetData(customizingWindow.CurrentData);
            agentInfoWindow.customizingWindow.OpenAppearanceWindow();
        }
    }
}
