// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IAngelaConversationUiAdapter : IAdapter<AngelaConversationUI>, IComponentAdapter
    {
        new AngelaConversationUI GameObject { get; set; }
        new IGameObjectAdapter GameObjectAdapter { get; }
        void AddMessage(string message);
    }
}
