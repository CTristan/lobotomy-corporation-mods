// SPDX-License-Identifier: MIT

#region

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IGameObjectAdapter
    {
        bool ActiveSelf { get; }
        void SetActive(bool value);
    }
}
