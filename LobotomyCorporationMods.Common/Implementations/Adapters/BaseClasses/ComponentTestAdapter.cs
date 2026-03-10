// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses
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
