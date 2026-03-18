// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.Common.Extensions;
using Hemocode.Common.Interfaces.Adapters;
using Hemocode.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;

namespace Hemocode.Common.Implementations.Adapters.BaseClasses
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
