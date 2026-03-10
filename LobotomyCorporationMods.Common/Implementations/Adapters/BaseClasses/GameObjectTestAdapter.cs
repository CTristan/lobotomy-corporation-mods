// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses
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
