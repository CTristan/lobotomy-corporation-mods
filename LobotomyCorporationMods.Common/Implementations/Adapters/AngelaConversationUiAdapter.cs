// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [ExcludeFromCodeCoverage]
    public sealed class AngelaConversationUiAdapter : IAngelaConversationUiAdapter
    {
        private readonly AngelaConversationUI _angelaConversationUI;

        public AngelaConversationUiAdapter(AngelaConversationUI angelaConversationUI)
        {
            _angelaConversationUI = angelaConversationUI;
        }

        public void AddMessage(string message)
        {
            _angelaConversationUI.AddAngelaMessage(message);
        }
    }
}
