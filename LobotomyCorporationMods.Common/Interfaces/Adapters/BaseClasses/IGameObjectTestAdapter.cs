// SPDX-License-Identifier: MIT

#region

using UnityEngine;

#endregion

namespace Hemocode.Common.Interfaces.Adapters.BaseClasses
{
    public interface IGameObjectTestAdapter : ITestAdapter<GameObject>
    {
        bool ActiveSelf { get; }
        ITransformTestAdapter Transform { get; }
        IImageTestAdapter ImageComponent { get; }
        IImageTestAdapter AddImageComponent();
        void SetActive(bool value);
    }
}
