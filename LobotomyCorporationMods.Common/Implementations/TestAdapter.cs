// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes.ValidCodeCoverageExceptionAttributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Interfaces;

#endregion

namespace LobotomyCorporationMods.Common.Implementations
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal class TestAdapter<T> : ITestAdapter<T> where T : class
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
        public T GameObject
        {
            get
            {
                if (_gameObject == null)
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
