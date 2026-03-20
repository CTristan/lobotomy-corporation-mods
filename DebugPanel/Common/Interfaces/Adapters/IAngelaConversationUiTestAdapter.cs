// SPDX-License-Identifier: MIT

using DebugPanel.Common.Interfaces.Adapters.BaseClasses;

namespace DebugPanel.Common.Interfaces.Adapters
{
    public interface IAngelaConversationUiTestAdapter : IComponentTestAdapter<AngelaConversationUI>
    {
        void AddMessage(string message);
    }
}
