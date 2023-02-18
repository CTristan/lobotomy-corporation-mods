// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using Customizing;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [ExcludeFromCodeCoverage]
    public sealed class AgentInfoWindowUiComponentsAdapter : IAgentInfoWindowUiComponentsAdapter
    {
        private readonly AgentInfoWindow.UIComponent _uiComponent;

        public AgentInfoWindowUiComponentsAdapter(AgentInfoWindow.UIComponent uiComponent)
        {
            _uiComponent = uiComponent;
        }

        public void SetData(AgentData agentData)
        {
            _uiComponent.SetData(agentData);
        }
    }
}
