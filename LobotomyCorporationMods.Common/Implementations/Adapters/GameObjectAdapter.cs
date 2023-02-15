// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    public sealed class GameObjectAdapter : IGameObjectAdapter
    {
        public bool GameObjectIsActive([NotNull] GameObject gameObject)
        {
            Guard.Against.Null(gameObject, nameof(gameObject));

            return gameObject.activeSelf;
        }
    }
}
