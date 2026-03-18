// SPDX-License-Identifier: MIT

using Hemocode.Common.Interfaces.Adapters.BaseClasses;

namespace Hemocode.Common.Interfaces.Adapters
{
    public interface IBeautyBeastAnimTestAdapter : IComponentTestAdapter<BeautyBeastAnim>
    {
        int State { get; }
    }
}
