// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [ExcludeFromCodeCoverage]
    public sealed class AgentLayerAdapter : IAgentLayerAdapter
    {
        private readonly AgentLayer _agentLayer;

        public AgentLayerAdapter(AgentLayer agentLayer)
        {
            _agentLayer = agentLayer;
        }

        public void AddAgent(AgentModel model)
        {
            _agentLayer.AddAgent(model);
        }

        public void RemoveAgent(AgentModel model)
        {
            _agentLayer.RemoveAgent(model);
        }
    }
}
