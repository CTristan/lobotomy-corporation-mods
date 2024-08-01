// SPDX-License-Identifier: MIT

#region

using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses
{
    public interface IComponentTestAdapter<T> : ITestAdapter<T> where T : Component
    {
        ITransformTestAdapter Transform { get; }
        void SetActive(bool value);
    }
}
