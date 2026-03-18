// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using Customizing;
using JetBrains.Annotations;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.Common.Interfaces.Adapters.BaseClasses;

#endregion

namespace Hemocode.Common.Implementations.Adapters.BaseClasses
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class AgentInfoWindowUiComponentsTestAdapter : TestAdapter<AgentInfoWindow.UIComponent>, IAgentInfoWindowUiComponentsTestAdapter

    {
        internal AgentInfoWindowUiComponentsTestAdapter([NotNull] AgentInfoWindow.UIComponent gameObject) : base(gameObject)
        {
        }

        public void SetData(AgentData agentData)
        {
            GameObjectInternal.SetData(agentData);
        }
    }
}
