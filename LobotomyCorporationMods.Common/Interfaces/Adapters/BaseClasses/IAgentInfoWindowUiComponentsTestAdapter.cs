// SPDX-License-Identifier: MIT

#region

using Customizing;

#endregion

namespace Hemocode.Common.Interfaces.Adapters.BaseClasses
{
    public interface IAgentInfoWindowUiComponentsTestAdapter : ITestAdapter<AgentInfoWindow.UIComponent>
    {
        void SetData(AgentData agentData);
    }
}
