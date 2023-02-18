// SPDX-License-Identifier:MIT

#region

using System.Collections.Generic;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IYggdrasilAnimAdapter
    {
        IEnumerable<IGameObjectAdapter> Flowers { get; }
    }
}
