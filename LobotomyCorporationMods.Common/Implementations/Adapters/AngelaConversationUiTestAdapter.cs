// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal sealed class AngelaConversationUiTestAdapter : ComponentTestAdapter<AngelaConversationUI>, IAngelaConversationUiTestAdapter
    {
        internal AngelaConversationUiTestAdapter([NotNull] AngelaConversationUI gameObject) : base(gameObject)
        {
        }

        public void AddMessage(string message)
        {
            _gameObject.AddAngelaMessage(message);
        }
    }
}
