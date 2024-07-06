// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IAngelaConversationUiTestAdapter : IComponentTestAdapter<AngelaConversationUI>
    {
        void AddMessage(string message);
    }
}
