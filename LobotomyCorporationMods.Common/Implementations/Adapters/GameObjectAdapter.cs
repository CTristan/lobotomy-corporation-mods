// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [ExcludeFromCodeCoverage]
    [AdapterClass]
    public class GameObjectAdapter : Adapter<GameObject>, IGameObjectAdapter
    {
        public bool ActiveSelf => GameObject.activeSelf;

        public void SetActive(bool value)
        {
            GameObject.SetActive(value);
        }
    }
}
