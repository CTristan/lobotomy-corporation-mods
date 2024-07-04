// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal sealed class AgentLayerTestAdapter : Adapter<AgentLayer>, IAgentLayerTestAdapter
    {
        internal AgentLayerTestAdapter([NotNull] AgentLayer gameObject) : base(gameObject)
        {
        }

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
