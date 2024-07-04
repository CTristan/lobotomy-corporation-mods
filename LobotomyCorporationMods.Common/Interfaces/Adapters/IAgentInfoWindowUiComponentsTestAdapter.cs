// SPDX-License-Identifier: MIT

#region

using Customizing;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IAgentInfoWindowUiComponentsTestAdapter : ITestAdapter<AgentInfoWindow.UIComponent>
    {
        void SetData(AgentData agentData);
    }
}
