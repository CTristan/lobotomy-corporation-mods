// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses
{
    internal class ComponentTestAdapter<T> : TestAdapter<T>, IComponentTestAdapter<T> where T : Component
    {
        internal ComponentTestAdapter([NotNull] T gameObject) : base(gameObject)
        {
        }

        [NotNull]
        public ITransformTestAdapter Transform => new TransformTestAdapter(GameObject.transform);

        public void SetActive(bool value)
        {
            GameObject.gameObject.SetActive(value);
        }
    }
}
