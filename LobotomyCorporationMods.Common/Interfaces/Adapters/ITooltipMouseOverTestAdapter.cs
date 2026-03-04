// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface ITooltipMouseOverTestAdapter : IComponentTestAdapter<TooltipMouseOver>
    {
        void SetDynamicTooltip(string str);
    }
}
