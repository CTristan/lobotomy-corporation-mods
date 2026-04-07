// SPDX-License-Identifier: MIT

#region

using System.Text;
using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common.Extensions;
using LobotomyCorporation.Mods.Common.Implementations.Facades;
using LobotomyCorporation.Mods.Common.Interfaces;
using LobotomyCorporation.Mods.Common.ParameterObjects;
using Hemocode.GiftAlertIcon.Constants;
using UnityEngine;

#endregion

namespace Hemocode.GiftAlertIcon.Extensions
{
    public static class ManagementSlotExtensions
    {
        internal static void UpdateGiftIcon([NotNull] this ManagementSlot instance,
            UnitModel agent,
            [NotNull] ImageParameters imageParameters,
            [NotNull] IFileManager fileManager,
            OptionalTestAdapterParameters testAdapterParameters)
        {
            if (!instance.AbnormalityHasGift())
            {
                instance.HideImageObject(imageParameters, fileManager, testAdapterParameters);

                return;
            }

            var giftSlot = instance.GetAbnormalityGiftPosition();
            var giftAttachType = instance.GetAbnormalityGiftAttachmentType();
            var giftsInSameSlot = agent.HasGiftInPosition(giftSlot, giftAttachType);
            if (giftsInSameSlot)
            {
                ProcessGiftInSameSlot(instance, agent, imageParameters, fileManager, testAdapterParameters);
            }
            else
            {
                instance.ShowAsNewGift(imageParameters, fileManager, testAdapterParameters);
            }
        }

        private static void ShowAsGift([NotNull] this ManagementSlot managementSlot,
            [NotNull] ImageParameters imageParameters,
            Color color,
            string tooltipLine1,
            string tooltipLine2,
            [NotNull] IFileManager fileManager,
            [CanBeNull] OptionalTestAdapterParameters testAdapterParameters = null)
        {
            StringBuilder tooltipMessage = new StringBuilder();
            _ = tooltipMessage.AppendLine(tooltipLine1);
            _ = tooltipMessage.AppendLine();
            _ = tooltipMessage.AppendLine(tooltipLine2);

            managementSlot.UpdateImage(imageParameters, fileManager, color, tooltipMessage.ToString(), testAdapterParameters);
        }

        private static void ShowAsNewGift([NotNull] this ManagementSlot managementSlot,
            [NotNull] ImageParameters imageParameters,
            [NotNull] IFileManager fileManager,
            [CanBeNull] OptionalTestAdapterParameters testAdapterParameters = null)
        {
            var color = Color.green;
            var tooltipLine1 = LocalizationIds.NewGiftTooltip1.GetLocalized();
            var tooltipLine2 = LocalizationIds.NewGiftTooltip2.GetLocalized();

            ShowAsGift(managementSlot, imageParameters, color, tooltipLine1, tooltipLine2, fileManager, testAdapterParameters);
        }

        private static void ShowAsReplacementGift([NotNull] this ManagementSlot managementSlot,
            [NotNull] ImageParameters imageParameters,
            [NotNull] IFileManager fileManager,
            [CanBeNull] OptionalTestAdapterParameters testAdapterParameters)
        {
            var color = Color.grey;
            var tooltipLine1 = LocalizationIds.ReplacementGiftTooltip1.GetLocalized();
            var tooltipLine2 = LocalizationIds.ReplacementGiftTooltip2.GetLocalized();

            ShowAsGift(managementSlot, imageParameters, color, tooltipLine1, tooltipLine2, fileManager, testAdapterParameters);
        }

        private static void ProcessGiftInSameSlot([NotNull] this ManagementSlot instance,
            [NotNull] UnitModel agent,
            [NotNull] ImageParameters imageParameters,
            [NotNull] IFileManager fileManager,
            OptionalTestAdapterParameters testAdapterParameters)
        {
            var giftId = instance.GetAbnormalityGiftId();
            if (agent.HasGift(giftId))
            {
                instance.HideImageObject(imageParameters, fileManager, testAdapterParameters);
            }
            else
            {
                instance.ShowAsReplacementGift(imageParameters, fileManager, testAdapterParameters);
            }
        }
    }
}
