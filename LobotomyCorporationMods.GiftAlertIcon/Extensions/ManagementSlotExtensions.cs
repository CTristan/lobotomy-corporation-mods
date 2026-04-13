// SPDX-License-Identifier: MIT

#region

using System.Text;
using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.GiftAlertIcon.Constants;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.GiftAlertIcon.Extensions
{
    internal static class ManagementSlotExtensions
    {
        internal static void UpdateGiftIcon(
            [NotNull] this ManagementSlot instance,
            UnitModel agent,
            [NotNull] ImageParameters imageParameters,
            [NotNull] IFileManager fileManager,
            OptionalOverrides optionalOverrides
        )
        {
            if (!instance.AbnormalityHasGift())
            {
                instance.HideImageObject(imageParameters, fileManager, optionalOverrides);

                return;
            }

            var giftSlot = instance.GetAbnormalityGiftPosition();
            var giftAttachType = instance.GetAbnormalityGiftAttachmentType();
            var giftsInSameSlot = agent.HasGiftInPosition(giftSlot, giftAttachType);
            if (giftsInSameSlot)
            {
                ProcessGiftInSameSlot(
                    instance,
                    agent,
                    imageParameters,
                    fileManager,
                    optionalOverrides
                );
            }
            else
            {
                instance.ShowAsNewGift(imageParameters, fileManager, optionalOverrides);
            }
        }

        private static void ShowAsGift(
            [NotNull] this ManagementSlot managementSlot,
            [NotNull] ImageParameters imageParameters,
            Color color,
            string tooltipLine1,
            string tooltipLine2,
            [NotNull] IFileManager fileManager,
            [CanBeNull] OptionalOverrides optionalOverrides = null
        )
        {
            var tooltipMessage = new StringBuilder();
            tooltipMessage.AppendLine(tooltipLine1);
            tooltipMessage.AppendLine();
            tooltipMessage.AppendLine(tooltipLine2);

            managementSlot.UpdateImage(
                imageParameters,
                fileManager,
                color,
                tooltipMessage.ToString(),
                optionalOverrides
            );
        }

        private static void ShowAsNewGift(
            [NotNull] this ManagementSlot managementSlot,
            [NotNull] ImageParameters imageParameters,
            [NotNull] IFileManager fileManager,
            [CanBeNull] OptionalOverrides optionalOverrides = null
        )
        {
            var color = Color.green;
            var tooltipLine1 = LocalizationIds.NewGiftTooltip1.GetLocalized();
            var tooltipLine2 = LocalizationIds.NewGiftTooltip2.GetLocalized();

            ShowAsGift(
                managementSlot,
                imageParameters,
                color,
                tooltipLine1,
                tooltipLine2,
                fileManager,
                optionalOverrides
            );
        }

        private static void ShowAsReplacementGift(
            [NotNull] this ManagementSlot managementSlot,
            [NotNull] ImageParameters imageParameters,
            [NotNull] IFileManager fileManager,
            [CanBeNull] OptionalOverrides optionalOverrides
        )
        {
            var color = Color.grey;
            var tooltipLine1 = LocalizationIds.ReplacementGiftTooltip1.GetLocalized();
            var tooltipLine2 = LocalizationIds.ReplacementGiftTooltip2.GetLocalized();

            ShowAsGift(
                managementSlot,
                imageParameters,
                color,
                tooltipLine1,
                tooltipLine2,
                fileManager,
                optionalOverrides
            );
        }

        private static void ProcessGiftInSameSlot(
            [NotNull] this ManagementSlot instance,
            [NotNull] UnitModel agent,
            [NotNull] ImageParameters imageParameters,
            [NotNull] IFileManager fileManager,
            OptionalOverrides optionalOverrides
        )
        {
            var giftId = instance.GetAbnormalityGiftId();
            if (agent.HasGift(giftId))
            {
                instance.HideImageObject(imageParameters, fileManager, optionalOverrides);
            }
            else
            {
                instance.ShowAsReplacementGift(imageParameters, fileManager, optionalOverrides);
            }
        }
    }
}
