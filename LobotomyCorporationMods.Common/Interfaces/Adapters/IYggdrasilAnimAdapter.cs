// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IYggdrasilAnimAdapter : IAdapter<YggdrasilAnim>
    {
        IEnumerable<IGameObjectAdapter> Flowers { get; }
    }
}
