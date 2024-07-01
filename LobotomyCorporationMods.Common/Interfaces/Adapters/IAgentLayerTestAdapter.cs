// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IAgentLayerTestAdapter : ITestAdapter<AgentLayer>
    {
        void AddAgent(AgentModel model);
        void RemoveAgent(AgentModel model);
    }
}
