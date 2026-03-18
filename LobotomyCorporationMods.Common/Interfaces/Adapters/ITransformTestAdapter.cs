// SPDX-License-Identifier: MIT

using Hemocode.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;

namespace Hemocode.Common.Interfaces.Adapters
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
