// SPDX-License-Identifier: MIT

#region

using System.Text;
using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations.Facades;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using LobotomyCorporationMods.GiftAvailabilityIndicator.Constants;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.GiftAvailabilityIndicator.Extensions
{
    internal static class ManagementSlotExtensions
    {
        internal static void ShowAsNewGift([NotNull] this ManagementSlot managementSlot,
            [NotNull] string imageName,
            [NotNull] string imagePath,
            [NotNull] IFileManager fileManager,
            [CanBeNull] IManagementSlotTestAdapter testAdapter = null,
            [CanBeNull] IGameObjectTestAdapter imageGameObjectTestAdapter = null,
            [CanBeNull] ITexture2dTestAdapter texture2dTestAdapter = null,
            [CanBeNull] ISpriteTestAdapter spriteTestAdapter = null)
        {
            var color = Color.green;
            var tooltipLine1 = LocalizationIds.NewGiftTooltip1.GetLocalized();
            var tooltipLine2 = LocalizationIds.NewGiftTooltip2.GetLocalized();

            var tooltipMessage = new StringBuilder();
            tooltipMessage.AppendLine(tooltipLine1);
            tooltipMessage.AppendLine();
            tooltipMessage.AppendLine(tooltipLine2);

            managementSlot.UpdateImage(imageName, imagePath, fileManager, color, tooltipMessage.ToString(), testAdapter, imageGameObjectTestAdapter, texture2dTestAdapter, spriteTestAdapter);
        }

        internal static void ShowAsReplacementGift([NotNull] this ManagementSlot managementSlot,
            [NotNull] string imageName,
            [NotNull] string imagePath,
            [NotNull] IFileManager fileManager,
            [CanBeNull] IManagementSlotTestAdapter testAdapter = null,
            [CanBeNull] IGameObjectTestAdapter imageGameObjectTestAdapter = null,
            [CanBeNull] ITexture2dTestAdapter texture2dTestAdapter = null,
            [CanBeNull] ISpriteTestAdapter spriteTestAdapter = null)
        {
            var color = Color.grey;
            var tooltipLine1 = LocalizationIds.ReplacementGiftTooltip1.GetLocalized();
            var tooltipLine2 = LocalizationIds.ReplacementGiftTooltip2.GetLocalized();

            var tooltipMessage = new StringBuilder();
            tooltipMessage.AppendLine(tooltipLine1);
            tooltipMessage.AppendLine();
            tooltipMessage.AppendLine(tooltipLine2);

            managementSlot.UpdateImage(imageName, imagePath, fileManager, color, tooltipMessage.ToString(), testAdapter, imageGameObjectTestAdapter, texture2dTestAdapter, spriteTestAdapter);
        }
    }
}
