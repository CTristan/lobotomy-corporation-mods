// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    internal sealed class GameObjectAdapter : IGameObjectAdapter
    {
        [ExcludeFromCodeCoverage(Justification = "Will always throw a Unity exception when calling this method, so no lines of code are ever covered.")]
        public bool GameObjectIsActive([NotNull] GameObject gameObject)
        {
            Guard.Against.Null(gameObject, nameof(gameObject));

            return gameObject.activeSelf;
        }
    }
}
