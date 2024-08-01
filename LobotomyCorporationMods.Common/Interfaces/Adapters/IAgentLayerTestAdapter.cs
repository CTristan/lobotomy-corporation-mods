// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IAgentLayerTestAdapter : IComponentTestAdapter<AgentLayer>
    {
        void AddAgent(AgentModel model);
        void RemoveAgent(AgentModel model);
    }
}
