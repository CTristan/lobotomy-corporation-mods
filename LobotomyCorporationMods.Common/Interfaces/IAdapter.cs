// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces
{
    public interface IAdapter<T>
    {
        T GameObject { get; set; }
        IGameObjectAdapter GameObjectAdapter { get; }
    }
}
