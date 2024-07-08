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
    internal sealed class GameObjectTestAdapter : TestAdapter<GameObject>, IGameObjectTestAdapter
    {
        internal GameObjectTestAdapter([NotNull] GameObject gameObject) : base(gameObject)
        {
        }

        public bool ActiveSelf => _gameObject.activeSelf;

        [NotNull]
        public ITransformTestAdapter Transform => new TransformTestAdapter(_gameObject.transform);

        [NotNull]
        public IImageTestAdapter AddImageComponent()
        {
            return new ImageTestAdapter(_gameObject.AddComponent<Image>());
        }

        [NotNull]
        public IImageTestAdapter ImageComponent => new ImageTestAdapter(_gameObject.GetComponent<Image>());

        public void SetActive(bool value)
        {
            _gameObject.SetActive(value);
        }

        public override GameObject GameObject
        {
            get =>
                !_gameObject.IsUnityNull() ? _gameObject : null;
            set =>
                _gameObject = value;
        }
    }
}
