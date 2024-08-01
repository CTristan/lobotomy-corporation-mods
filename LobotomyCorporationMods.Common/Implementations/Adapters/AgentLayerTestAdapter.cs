// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes.ValidCodeCoverageExceptionAttributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal sealed class AgentLayerTestAdapter : ComponentTestAdapter<AgentLayer>, IAgentLayerTestAdapter
    {
        internal AgentLayerTestAdapter([NotNull] AgentLayer gameObject) : base(gameObject)
        {
        }

        public void AddAgent(AgentModel model)
        {
            _gameObject.AddAgent(model);
        }

        public void RemoveAgent(AgentModel model)
        {
            _gameObject.RemoveAgent(model);
        }
    }
}
