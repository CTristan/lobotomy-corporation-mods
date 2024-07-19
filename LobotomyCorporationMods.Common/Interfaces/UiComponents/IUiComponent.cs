// SPDX-License-Identifier: MIT

using UnityEngine;

namespace LobotomyCorporationMods.Common.Interfaces.UiComponents
{
    public interface IUiComponent
    {
        void SetActive(bool value);

        void SetParent(Transform parent);

        void SetPosition(float x,
            float y);
    }
}
