// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using DebugPanel.Common.Attributes;
using DebugPanel.Common.Constants;
using DebugPanel.Common.Extensions;
using DebugPanel.Common.Interfaces.Adapters;
using DebugPanel.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;

namespace DebugPanel.Common.Implementations.Adapters.BaseClasses
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public class ComponentTestAdapter<T> : TestAdapter<T>, IComponentTestAdapter<T> where T : Component
    {
        internal ComponentTestAdapter([NotNull] T gameObject) : base(gameObject)
        {
        }

        [NotNull]
        public ITransformTestAdapter Transform =>
            new TransformTestAdapter(GameObjectInternal.transform);

        public void SetActive(bool value)
        {
            GameObjectInternal.gameObject.SetActive(value);
        }

        public override T GameObject
        {
            get =>
                !GameObjectInternal.IsUnityNull() ? GameObjectInternal : null;
            set =>
                GameObjectInternal = value;
        }
    }
}
