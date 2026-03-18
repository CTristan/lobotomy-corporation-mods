// SPDX-License-Identifier: MIT

using Hemocode.Common.Interfaces.Adapters.BaseClasses;

namespace Hemocode.Common.Interfaces.Adapters
{
    public interface IAgentLayerTestAdapter : IComponentTestAdapter<AgentLayer>
    {
        void AddAgent(AgentModel model);
        void RemoveAgent(AgentModel model);
    }
}
