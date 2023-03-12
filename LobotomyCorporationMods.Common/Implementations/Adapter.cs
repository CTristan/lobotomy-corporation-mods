// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Implementations
{
    [AdapterClass]
    [ExcludeFromCodeCoverage]
    public class Adapter<T> : IAdapter<T>
    {
        private const string UninitializedGameObjectErrorMessage = "Please load the game object into the adapter before trying to use it.";
        private T? _gameObject;

        protected Adapter()
        {
        }

        public T GameObject
        {
            get
            {
                if (_gameObject is null)
                {
                    throw new InvalidOperationException(UninitializedGameObjectErrorMessage);
                }

                return _gameObject;
            }
            set => _gameObject = value;
        }

        public IGameObjectAdapter GameObjectAdapter => GameObject switch
        {
            GameObject gameObject => new GameObjectAdapter { GameObject = gameObject },
            Component component => new GameObjectAdapter { GameObject = component.gameObject },
            _ => throw new InvalidOperationException(UninitializedGameObjectErrorMessage)
        };
    }
}
