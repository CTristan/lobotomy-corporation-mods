// SPDX-License-Identifier: MIT

using UnityEngine;

namespace LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses
{
    public interface IComponentTestAdapter<T> : ITestAdapter<T> where T : Component
    {
        ITransformTestAdapter Transform { get; }
        bool IsUnityNull();
        void SetActive(bool value);
    }
}
