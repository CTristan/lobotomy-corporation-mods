// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface ITransformTestAdapter : IComponentTestAdapter<Transform>
    {
        Vector3 LocalPosition { get; set; }
        Vector3 LocalScale { get; set; }
        ITransformTestAdapter Parent { get; }
        ITransformTestAdapter GetChild(int index);
        void SetParent(ITransformTestAdapter parent);
    }
}
