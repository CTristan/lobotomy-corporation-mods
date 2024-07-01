// SPDX-License-Identifier: MIT

#region

using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IGameObjectTestAdapter : ITestAdapter<GameObject>
    {
        bool ActiveSelf { get; }
        void SetActive(bool value);
    }
}
