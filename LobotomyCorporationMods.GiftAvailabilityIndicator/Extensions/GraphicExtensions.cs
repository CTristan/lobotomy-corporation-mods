// SPDX-License-Identifier: MIT

#region

using System.Text;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.GiftAvailabilityIndicator.Extensions
{
    internal static class GraphicExtensions
    {
        internal static void Hide(this Graphic image)
        {
            image.color = Color.clear;

            var tooltip = image.GetComponent<TooltipMouseOver>();
            tooltip.gameObject.SetActive(false);
        }

        internal static void ShowAsNewGift(this Graphic image)
        {
            image.color = Color.green;

            var tooltipMessage = new StringBuilder();
            tooltipMessage.AppendLine("Gift available in new slot");
            tooltipMessage.AppendLine("Using this agent may provide the agent with a gift in a slot that is currently empty");

            var tooltip = image.GetComponent<TooltipMouseOver>();
            tooltip.gameObject.SetActive(true);
            tooltip.SetDynamicTooltip(tooltipMessage.ToString());
        }

        internal static void ShowAsReplacementGift(this Graphic image)
        {
            image.color = Color.grey;

            var tooltipMessage = new StringBuilder();
            tooltipMessage.AppendLine("Gift may replace another gift");
            tooltipMessage.AppendLine("This agent already has a gift in this slot");

            var tooltip = image.GetComponent<TooltipMouseOver>();
            tooltip.gameObject.SetActive(true);
            tooltip.SetDynamicTooltip(tooltipMessage.ToString());
        }
    }
}
