// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage]
    public sealed class AngelaConversationUiAdapter : ComponentAdapter, IAngelaConversationUiAdapter
    {
        private AngelaConversationUI? _angelaConversationUI;

        public void AddMessage(string message)
        {
            GameObject.AddAngelaMessage(message);
        }

        public new AngelaConversationUI GameObject
        {
            get
            {
                if (_angelaConversationUI is null)
                {
                    throw new InvalidOperationException(UninitializedGameObjectErrorMessage);
                }

                return _angelaConversationUI;
            }
            set => _angelaConversationUI = value;
        }
    }
}
