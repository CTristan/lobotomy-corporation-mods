// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IAgentLayerAdapter : IAdapter<AgentLayer>, IComponentAdapter
    {
        new AgentLayer GameObject { get; set; }
        new IGameObjectAdapter GameObjectAdapter { get; }
        void AddAgent(AgentModel model);
        void RemoveAgent(AgentModel model);
    }
}
