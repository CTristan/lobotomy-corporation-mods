// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IGameObjectAdapter
    {
        bool ActiveSelf { get; }
        void SetActive(bool value);
    }
}
