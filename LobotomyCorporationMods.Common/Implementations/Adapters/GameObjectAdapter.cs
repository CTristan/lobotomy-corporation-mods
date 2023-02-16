// SPDX-License-Identifier: MIT

#region

using System;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    public sealed class GameObjectAdapter : IGameObjectAdapter
    {
        public bool GameObjectIsActive([NotNull] GameObject gameObject)
        {
            try
            {
                gameObject.NotNull(nameof(gameObject));

                return gameObject.activeSelf;
            }
            catch (Exception exception) when (exception.IsUnityException())
            {
                return false;
            }
        }
    }
}
