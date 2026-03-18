// SPDX-License-Identifier: MIT

using Hemocode.Common.Interfaces.Adapters.BaseClasses;

namespace Hemocode.Common.Interfaces.Adapters
{
    public interface ITooltipMouseOverTestAdapter : IComponentTestAdapter<TooltipMouseOver>
    {
        void SetDynamicTooltip(string str);
    }
}
