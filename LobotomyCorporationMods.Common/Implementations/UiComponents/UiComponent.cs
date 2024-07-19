// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Interfaces.UiComponents;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Implementations.UiComponents
{
    internal abstract class UiComponent : IUiComponent
    {
        protected GameObject GameObject { get; } = new GameObject();

        public void SetActive(bool value)
        {
            GameObject.SetActive(value);
        }

        public void SetParent(Transform parent)
        {
            GameObject.transform.SetParent(parent);
        }

        public void SetPosition(float x,
            float y)
        {
            GameObject.transform.localPosition = new Vector2(x, y);
        }
    }
}
