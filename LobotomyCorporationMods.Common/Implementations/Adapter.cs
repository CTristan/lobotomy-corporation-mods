// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces;

#endregion

namespace LobotomyCorporationMods.Common.Implementations
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public class Adapter<T> : ITestAdapter<T>
    {
        private T _gameObject;

        protected Adapter()
        {
        }

        [NotNull]
        public T GameObject
        {
            get
            {
                if (!_gameObject.IsNotNull())
                {
                    throw new InvalidOperationException("Please load the game object into the adapter before trying to use it.");
                }

                return _gameObject;
            }
            set =>
                _gameObject = value;
        }
    }
}
