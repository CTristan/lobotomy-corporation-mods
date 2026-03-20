// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using DebugPanel.Common.Attributes;
using DebugPanel.Common.Constants;
using DebugPanel.Common.Extensions;
using DebugPanel.Common.Interfaces;

#endregion

namespace DebugPanel.Common.Implementations
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public class TestAdapter<T> : ITestAdapter<T>
    {
        private const string UninitializedGameObjectErrorMessage = "Please load the game object into the adapter before trying to use it.";

        protected T GameObjectInternal { get; set; }

        // ReSharper disable once UnusedMember.Global
        protected TestAdapter()
        {
        }

        protected TestAdapter([NotNull] T gameObject)
        {
            GameObjectInternal = gameObject;
        }

        [CanBeNull]
        public virtual T GameObject
        {
            get => !GameObjectInternal.IsNotNull() ? throw new InvalidOperationException(UninitializedGameObjectErrorMessage) : GameObjectInternal;
            set =>
                GameObjectInternal = value;
        }
    }
}
