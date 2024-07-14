// SPDX-License-Identifier: MIT

#region

using System.Text;
using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations.Facades;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.ParameterObjects;
using LobotomyCorporationMods.GiftAvailabilityIndicator.Constants;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.GiftAvailabilityIndicator.Extensions
{
    internal static class ManagementSlotExtensions
    {
        internal static void UpdateGiftIcon([NotNull] this ManagementSlot instance,
            UnitModel agent,
            [NotNull] string imageName,
            [NotNull] string imagePath,
            [NotNull] IFileManager fileManager,
            OptionalTestAdapterParameters testAdapterParameters)
        {
            if (!instance.AbnormalityHasGift())
            {
                instance.HideImageObject(imageName, imagePath, fileManager, testAdapterParameters);
                return;
            }

            var giftSlot = instance.GetAbnormalityGiftPosition();
            var giftsInSameSlot = agent.HasGiftInPosition(giftSlot);
            if (giftsInSameSlot)
            {
                ProcessGiftInSameSlot(instance, agent, imageName, imagePath, fileManager, testAdapterParameters);
            }
            else
            {
                instance.ShowAsNewGift(imageName, imagePath, fileManager, testAdapterParameters);
            }
        }

        private static void ShowAsNewGift([NotNull] this ManagementSlot managementSlot,
            [NotNull] string imageName,
            [NotNull] string imagePath,
            [NotNull] IFileManager fileManager,
            [CanBeNull] OptionalTestAdapterParameters testAdapterParameters = null)
        {
            var color = Color.green;
            var tooltipLine1 = LocalizationIds.NewGiftTooltip1.GetLocalized();
            var tooltipLine2 = LocalizationIds.NewGiftTooltip2.GetLocalized();

            var tooltipMessage = new StringBuilder();
            tooltipMessage.AppendLine(tooltipLine1);
            tooltipMessage.AppendLine();
            tooltipMessage.AppendLine(tooltipLine2);

            managementSlot.UpdateImage(imageName, imagePath, fileManager, color, tooltipMessage.ToString(), testAdapterParameters);
        }

        private static void ShowAsReplacementGift([NotNull] this ManagementSlot managementSlot,
            [NotNull] string imageName,
            [NotNull] string imagePath,
            [NotNull] IFileManager fileManager,
            [CanBeNull] OptionalTestAdapterParameters testAdapterParameters)
        {
            var color = Color.grey;
            var tooltipLine1 = LocalizationIds.ReplacementGiftTooltip1.GetLocalized();
            var tooltipLine2 = LocalizationIds.ReplacementGiftTooltip2.GetLocalized();

            var tooltipMessage = new StringBuilder();
            tooltipMessage.AppendLine(tooltipLine1);
            tooltipMessage.AppendLine();
            tooltipMessage.AppendLine(tooltipLine2);

            managementSlot.UpdateImage(imageName, imagePath, fileManager, color, tooltipMessage.ToString(), testAdapterParameters);
        }

        private static void ProcessGiftInSameSlot([NotNull] this ManagementSlot instance,
            [NotNull] UnitModel agent,
            [NotNull] string imageName,
            [NotNull] string imagePath,
            [NotNull] IFileManager fileManager,
            OptionalTestAdapterParameters testAdapterParameters)
        {
            var giftId = instance.GetAbnormalityGiftId();
            if (agent.HasGift(giftId))
            {
                instance.HideImageObject(imageName, imagePath, fileManager, testAdapterParameters);
            }
            else
            {
                instance.ShowAsReplacementGift(imageName, imagePath, fileManager, testAdapterParameters);
            }
        }
    }
}
