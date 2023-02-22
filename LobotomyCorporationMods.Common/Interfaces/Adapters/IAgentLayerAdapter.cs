// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IAgentLayerAdapter : IAdapter<AgentLayer>
    {
        void AddAgent(AgentModel model);
        void RemoveAgent(AgentModel model);
    }
}
