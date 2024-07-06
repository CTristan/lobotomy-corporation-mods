// SPDX-License-Identifier: MIT

#region

using System.Text;
using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations.Facades;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.GiftAvailabilityIndicator.Extensions
{
    internal static class ManagementSlotExtensions
    {
        internal static void ShowAsNewGift([NotNull] this ManagementSlot managementSlot,
            [NotNull] string imageName,
            [NotNull] IFileManager fileManager,
            [CanBeNull] IManagementSlotTestAdapter testAdapter = null,
            [CanBeNull] IGameObjectTestAdapter imageGameObjectTestAdapter = null,
            [CanBeNull] ITexture2dTestAdapter texture2dTestAdapter = null,
            [CanBeNull] ISpriteTestAdapter spriteTestAdapter = null)
        {
            var color = Color.green;
            var tooltipMessage = new StringBuilder();
            tooltipMessage.AppendLine("Gift available in new slot");
            tooltipMessage.AppendLine("Using this agent may provide the agent with a gift in a slot that is currently empty");

            managementSlot.UpdateImage(imageName, fileManager, color, tooltipMessage.ToString(), testAdapter, imageGameObjectTestAdapter, texture2dTestAdapter, spriteTestAdapter);
        }

        internal static void ShowAsReplacementGift([NotNull] this ManagementSlot managementSlot,
            [NotNull] string imageName,
            [NotNull] IFileManager fileManager,
            [CanBeNull] IManagementSlotTestAdapter testAdapter = null,
            [CanBeNull] IGameObjectTestAdapter imageGameObjectTestAdapter = null,
            [CanBeNull] ITexture2dTestAdapter texture2dTestAdapter = null,
            [CanBeNull] ISpriteTestAdapter spriteTestAdapter = null)
        {
            var color = Color.grey;
            var tooltipMessage = new StringBuilder();
            tooltipMessage.AppendLine("Gift may replace another gift");
            tooltipMessage.AppendLine("This agent already has a gift in this slot");

            managementSlot.UpdateImage(imageName, fileManager, color, tooltipMessage.ToString(), testAdapter, imageGameObjectTestAdapter, texture2dTestAdapter, spriteTestAdapter);
        }
    }
}
