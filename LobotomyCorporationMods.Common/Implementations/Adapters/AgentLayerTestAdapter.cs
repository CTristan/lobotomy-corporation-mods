// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.Common.Implementations.Adapters.BaseClasses;
using Hemocode.Common.Interfaces.Adapters;

#endregion

namespace Hemocode.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class AgentLayerTestAdapter : ComponentTestAdapter<AgentLayer>, IAgentLayerTestAdapter
    {
        internal AgentLayerTestAdapter([NotNull] AgentLayer gameObject) : base(gameObject)
        {
        }

        public void AddAgent(AgentModel model)
        {
            GameObjectInternal.AddAgent(model);
        }

        public void RemoveAgent(AgentModel model)
        {
            GameObjectInternal.RemoveAgent(model);
        }
    }
}
