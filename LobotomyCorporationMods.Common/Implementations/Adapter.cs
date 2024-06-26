// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces;

#endregion

namespace LobotomyCorporationMods.Common.Implementations
{
    [AdapterClass]
    [ExcludeFromCodeCoverage]
    public class Adapter<T> : IAdapter<T>
    {
        private T _gameObject;

        protected Adapter()
        {
        }

        public T GameObject
        {
            get
            {
                if (!(_gameObject is object))
                {
                    throw new InvalidOperationException("Please load the game object into the adapter before trying to use it.");
                }

                return _gameObject;
            }
            set => _gameObject = value;
        }
    }
}
