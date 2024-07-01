// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class AngelaConversationUiTestAdapter : Adapter<AngelaConversationUI>, IAngelaConversationUiTestAdapter
    {
        public void AddMessage(string message)
        {
            GameObject.AddAngelaMessage(message);
        }
    }
}
