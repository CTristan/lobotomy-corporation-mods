// SPDX-License-Identifier: MIT

#region

using System;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    public sealed class AngelaConversationUiAdapter : IAngelaConversationUiAdapter
    {
        public void AddMessage(string message)
        {
            // Asynchronously send the message so we don't hold up anything else
            Action action = () => AngelaConversationUI.instance.AddAngelaMessage(message);
            action.BeginInvoke(ar => action.EndInvoke(ar), null);
        }
    }
}
