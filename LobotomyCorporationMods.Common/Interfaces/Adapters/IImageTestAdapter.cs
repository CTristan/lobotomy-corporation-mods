// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IImageTestAdapter : IComponentTestAdapter<Image>
    {
        Color Color { get; set; }
        Sprite Sprite { get; set; }
        ITooltipMouseOverTestAdapter TooltipMouseOverComponent { get; }
        ITooltipMouseOverTestAdapter AddTooltipMouseOverComponent();
    }
}
