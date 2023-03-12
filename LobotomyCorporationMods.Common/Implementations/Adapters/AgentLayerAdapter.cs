// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage]
    public sealed class AgentLayerAdapter : ComponentAdapter, IAgentLayerAdapter
    {
        private AgentLayer? _agentLayer;

        public void AddAgent(AgentModel model)
        {
            GameObject.AddAgent(model);
        }

        public new AgentLayer GameObject
        {
            get
            {
                if (_agentLayer is null)
                {
                    throw new InvalidOperationException(UninitializedGameObjectErrorMessage);
                }

                return _agentLayer;
            }
            set => _agentLayer = value;
        }

        public void RemoveAgent(AgentModel model)
        {
            GameObject.RemoveAgent(model);
        }
    }
}
