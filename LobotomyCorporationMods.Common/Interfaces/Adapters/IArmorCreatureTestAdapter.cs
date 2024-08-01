// SPDX-License-Identifier: MIT

#region

using System.Collections;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IArmorCreatureTestAdapter
    {
        IList SpecialAgentList { get; }
        void ReloadSpecialAgentList();
    }
}
