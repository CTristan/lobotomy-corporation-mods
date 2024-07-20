// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Interfaces.UiComponents
{
    public interface IUiComponent
    {
        bool IsActive { get; }
        T AddComponent<T>() where T : Component;
        bool AnyComponentIsNull();
        void SetActive(bool value);
        void SetParent(Transform parent);

        void SetPosition(float x,
            float y);
    }

    public static class IUiComponentExtensions
    {
        public static bool IsUnityNull([CanBeNull] this IUiComponent uiComponent)
        {
            return uiComponent == null || uiComponent.AnyComponentIsNull();
        }
    }
}
