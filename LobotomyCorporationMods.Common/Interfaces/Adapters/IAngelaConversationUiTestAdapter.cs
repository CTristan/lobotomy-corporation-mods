// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IAngelaConversationUiTestAdapter : IComponentTestAdapter<AngelaConversationUI>
    {
        void AddMessage(string message);
    }
}
