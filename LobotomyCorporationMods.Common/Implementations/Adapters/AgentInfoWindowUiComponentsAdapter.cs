// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using Customizing;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class AgentInfoWindowUiComponentsAdapter : Adapter<AgentInfoWindow.UIComponent>, IAgentInfoWindowUiComponentsAdapter
    {
        public void SetData(AgentData agentData)
        {
            GameObject.SetData(agentData);
        }
    }
}
