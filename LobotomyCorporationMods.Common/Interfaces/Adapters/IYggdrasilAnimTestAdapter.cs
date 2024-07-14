// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IYggdrasilAnimTestAdapter : IComponentTestAdapter<YggdrasilAnim>
    {
        IEnumerable<IGameObjectTestAdapter> Flowers { get; }
    }
}
