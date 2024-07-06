// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IAgentLayerTestAdapter : IComponentTestAdapter<AgentLayer>
    {
        void AddAgent(AgentModel model);
        void RemoveAgent(AgentModel model);
    }
}
