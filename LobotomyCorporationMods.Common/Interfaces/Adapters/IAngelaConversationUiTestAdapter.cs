// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IAngelaConversationUiTestAdapter : ITestAdapter<AngelaConversationUI>
    {
        void AddMessage(string message);
    }
}
