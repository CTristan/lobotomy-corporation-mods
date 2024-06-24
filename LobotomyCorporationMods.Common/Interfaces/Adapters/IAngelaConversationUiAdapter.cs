// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IAngelaConversationUiAdapter : IAdapter<AngelaConversationUI>
    {
        void AddMessage(string message);
    }
}
