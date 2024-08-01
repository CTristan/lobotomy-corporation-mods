// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface ITooltipMouseOverTestAdapter : IComponentTestAdapter<TooltipMouseOver>
    {
        void SetDynamicTooltip(string str);
    }
}
