// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IYggdrasilAnimTestAdapter : ITestAdapter<YggdrasilAnim>
    {
        IEnumerable<IGameObjectTestAdapter> Flowers { get; }
    }
}
