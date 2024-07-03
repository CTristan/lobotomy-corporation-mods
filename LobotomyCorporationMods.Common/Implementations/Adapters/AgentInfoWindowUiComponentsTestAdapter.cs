// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal sealed class AgentInfoWindowUiComponentsTestAdapter : Adapter<AgentInfoWindow.UIComponent>, IAgentInfoWindowUiComponentsTestAdapter
    {
        internal AgentInfoWindowUiComponentsTestAdapter([NotNull] AgentInfoWindow.UIComponent gameObject) : base(gameObject)
        {
        }

        public void SetData(AgentData agentData)
        {
            GameObject.SetData(agentData);
        }
    }
}
