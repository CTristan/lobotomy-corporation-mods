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
    internal class TestAdapter<T> : ITestAdapter<T>
    {
        private const string UninitializedGameObjectErrorMessage = "Please load the game object into the adapter before trying to use it.";

        protected T _gameObject;

        // ReSharper disable once UnusedMember.Global
        protected TestAdapter()
        {
        }

        protected TestAdapter([NotNull] T gameObject)
        {
            _gameObject = gameObject;
        }

        [CanBeNull]
        public virtual T GameObject
        {
            get
            {
                if (!_gameObject.IsNotNull())
                {
                    throw new InvalidOperationException(UninitializedGameObjectErrorMessage);
                }

                return _gameObject;
            }
            set =>
                _gameObject = value;
        }
    }
}
