// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal sealed class TransformTestAdapter : ComponentTestAdapter<Transform>, ITransformTestAdapter
    {
        internal TransformTestAdapter([NotNull] Transform gameObject) : base(gameObject)
        {
        }

        [NotNull]
        public ITransformTestAdapter Parent => new TransformTestAdapter(_gameObject.parent);

        [NotNull]
        public ITransformTestAdapter GetChild(int index)
        {
            return new TransformTestAdapter(_gameObject.GetChild(index));
        }

        public Vector3 LocalPosition
        {
            get =>
                _gameObject.localPosition;
            set =>
                _gameObject.localPosition = value;
        }

        public Vector3 LocalScale
        {
            get =>
                _gameObject.localScale;
            set =>
                _gameObject.localScale = value;
        }

        public void SetParent([NotNull] ITransformTestAdapter parent)
        {
            _gameObject.SetParent(parent.GameObject);
        }
    }
}
