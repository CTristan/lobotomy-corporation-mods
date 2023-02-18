// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [ExcludeFromCodeCoverage]
    public class GameObjectAdapter : IGameObjectAdapter
    {
        private readonly GameObject _gameObject;

        public GameObjectAdapter(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public bool ActiveSelf => _gameObject.activeSelf;

        public void SetActive(bool value)
        {
            _gameObject.SetActive(value);
        }
    }
}
