// SPDX-License-Identifier: MIT

#region

using Customizing;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IAgentInfoWindowUiComponentsAdapter : IAdapter<AgentInfoWindow.UIComponent>
    {
        void SetData(AgentData agentData);
    }
}
