// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.Common.Implementations.Adapters.BaseClasses;
using Hemocode.Common.Interfaces.Adapters;
using UnityEngine;

namespace Hemocode.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class TransformTestAdapter : ComponentTestAdapter<Transform>, ITransformTestAdapter
    {
        internal TransformTestAdapter([NotNull] Transform gameObject) : base(gameObject)
        {
        }

        [NotNull]
        public ITransformTestAdapter Parent =>
            new TransformTestAdapter(GameObjectInternal.parent);

        [NotNull]
        public ITransformTestAdapter GetChild(int index)
        {
            return new TransformTestAdapter(GameObjectInternal.GetChild(index));
        }

        public Vector3 LocalPosition
        {
            get =>
                GameObjectInternal.localPosition;
            set =>
                GameObjectInternal.localPosition = value;
        }

        public Vector3 LocalScale
        {
            get =>
                GameObjectInternal.localScale;
            set =>
                GameObjectInternal.localScale = value;
        }

        public void SetParent([NotNull] ITransformTestAdapter parent)
        {
            ThrowHelper.ThrowIfNull(parent);
            GameObjectInternal.SetParent(parent.GameObject);
        }
    }
}
