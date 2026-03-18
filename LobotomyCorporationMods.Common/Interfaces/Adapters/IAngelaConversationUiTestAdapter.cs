// SPDX-License-Identifier: MIT

using Hemocode.Common.Interfaces.Adapters.BaseClasses;

namespace Hemocode.Common.Interfaces.Adapters
{
    public interface IAngelaConversationUiTestAdapter : IComponentTestAdapter<AngelaConversationUI>
    {
        void AddMessage(string message);
    }
}
