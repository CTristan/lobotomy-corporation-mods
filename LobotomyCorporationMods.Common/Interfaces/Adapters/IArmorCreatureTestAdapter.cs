// SPDX-License-Identifier: MIT

using System.Collections;

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IArmorCreatureTestAdapter
    {
        IList SpecialAgentList { get; }
        void OnViewInit();
    }
}
