// SPDX-License-Identifier: MIT

using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    public sealed class AgentInfoWindowAdapter : IAgentInfoWindowAdapter
    {
        public void OpenAppearanceWindow([NotNull] AgentInfoWindow agentInfoWindow)
        {
            Guard.Against.Null(agentInfoWindow, nameof(agentInfoWindow));

            var customizingWindow = CustomizingWindow.CurrentWindow;

            agentInfoWindow.customizingBlock.SetActive(true);
            agentInfoWindow.AppearanceActiveControl.SetActive(true);
            agentInfoWindow.UIComponents.SetData(customizingWindow.CurrentData);
            agentInfoWindow.customizingWindow.OpenAppearanceWindow();
        }
    }
}
