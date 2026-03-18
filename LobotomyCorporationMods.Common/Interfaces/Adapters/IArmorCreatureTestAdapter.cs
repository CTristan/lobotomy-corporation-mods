// SPDX-License-Identifier: MIT

using System.Collections;

namespace Hemocode.Common.Interfaces.Adapters
{
    public interface IArmorCreatureTestAdapter
    {
        IList SpecialAgentList { get; }
        void ReloadSpecialAgentList();
    }
}
