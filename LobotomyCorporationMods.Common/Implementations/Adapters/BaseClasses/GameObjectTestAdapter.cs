// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.Common.Extensions;
using Hemocode.Common.Interfaces.Adapters;
using Hemocode.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Hemocode.Common.Implementations.Adapters.BaseClasses
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class GameObjectTestAdapter : TestAdapter<GameObject>, IGameObjectTestAdapter
    {
        internal GameObjectTestAdapter([NotNull] GameObject gameObject) : base(gameObject)
        {
        }

        public bool ActiveSelf => GameObjectInternal.activeSelf;

        [NotNull]
        public ITransformTestAdapter Transform => new TransformTestAdapter(GameObjectInternal.transform);

        [NotNull]
        public IImageTestAdapter AddImageComponent()
        {
            return new ImageTestAdapter(GameObjectInternal.AddComponent<Image>());
        }

        [NotNull]
        public IImageTestAdapter ImageComponent => new ImageTestAdapter(GameObjectInternal.GetComponent<Image>());

        public void SetActive(bool value)
        {
            GameObjectInternal.SetActive(value);
        }

        public override GameObject GameObject
        {
            get =>
                !GameObjectInternal.IsUnityNull() ? GameObjectInternal : null;
            set =>
                GameObjectInternal = value;
        }
    }
}
