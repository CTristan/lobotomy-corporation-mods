// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    internal sealed class TransformTestAdapter : ComponentTestAdapter<Transform>, ITransformTestAdapter
    {
        internal TransformTestAdapter([NotNull] Transform gameObject) : base(gameObject)
        {
        }

        [NotNull]
        public ITransformTestAdapter Parent => new TransformTestAdapter(GameObject.parent);

        [NotNull]
        public ITransformTestAdapter GetChild(int index)
        {
            return new TransformTestAdapter(GameObject.GetChild(index));
        }

        public Vector3 LocalPosition
        {
            get =>
                GameObject.localPosition;
            set =>
                GameObject.localPosition = value;
        }

        public Vector3 LocalScale
        {
            get =>
                GameObject.localScale;
            set =>
                GameObject.localScale = value;
        }

        public void SetParent([NotNull] ITransformTestAdapter parent)
        {
            GameObject.SetParent(parent.GameObject);
        }
    }
}
