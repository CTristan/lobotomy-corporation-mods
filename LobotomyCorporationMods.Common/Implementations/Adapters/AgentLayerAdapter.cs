// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage]
    public sealed class AgentLayerAdapter : Adapter<AgentLayer>, IAgentLayerAdapter
    {
        public void AddAgent(AgentModel model)
        {
            GameObject.AddAgent(model);
        }

        public void RemoveAgent(AgentModel model)
        {
            GameObject.RemoveAgent(model);
        }
    }
}
