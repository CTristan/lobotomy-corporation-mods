// SPDX-License-Identifier: MIT

#region

using System.Text;
using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations.Facades;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.ParameterContainers;
using LobotomyCorporationMods.GiftAlertIcon.Constants;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.GiftAlertIcon.Extensions
{
    internal static class ManagementSlotExtensions
    {
        internal static void UpdateGiftIcon([NotNull] this ManagementSlot instance,
            UnitModel agent,
            [NotNull] ImageParametersContainer imageParameters,
            [NotNull] IFileManager fileManager,
            OptionalTestAdapterParametersContainer testAdapterParameters)
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
            [NotNull] ImageParametersContainer imageParameters,
            Color color,
            string tooltipLine1,
            string tooltipLine2,
            [NotNull] IFileManager fileManager,
            [CanBeNull] OptionalTestAdapterParametersContainer testAdapterParameters = null)
        {
            var tooltipMessage = new StringBuilder();
            tooltipMessage.AppendLine(tooltipLine1);
            tooltipMessage.AppendLine();
            tooltipMessage.AppendLine(tooltipLine2);

            managementSlot.UpdateImage(imageParameters, fileManager, color, tooltipMessage.ToString(), testAdapterParameters);
        }

        private static void ShowAsNewGift([NotNull] this ManagementSlot managementSlot,
            [NotNull] ImageParametersContainer imageParameters,
            [NotNull] IFileManager fileManager,
            [CanBeNull] OptionalTestAdapterParametersContainer testAdapterParameters = null)
        {
            var color = Color.green;
            var tooltipLine1 = LocalizationIds.NewGiftTooltip1.GetLocalized();
            var tooltipLine2 = LocalizationIds.NewGiftTooltip2.GetLocalized();

            ShowAsGift(managementSlot, imageParameters, color, tooltipLine1, tooltipLine2, fileManager, testAdapterParameters);
        }

        private static void ShowAsReplacementGift([NotNull] this ManagementSlot managementSlot,
            [NotNull] ImageParametersContainer imageParameters,
            [NotNull] IFileManager fileManager,
            [CanBeNull] OptionalTestAdapterParametersContainer testAdapterParameters)
        {
            var color = Color.grey;
            var tooltipLine1 = LocalizationIds.ReplacementGiftTooltip1.GetLocalized();
            var tooltipLine2 = LocalizationIds.ReplacementGiftTooltip2.GetLocalized();

            ShowAsGift(managementSlot, imageParameters, color, tooltipLine1, tooltipLine2, fileManager, testAdapterParameters);
        }

        private static void ProcessGiftInSameSlot([NotNull] this ManagementSlot instance,
            [NotNull] UnitModel agent,
            [NotNull] ImageParametersContainer imageParameters,
            [NotNull] IFileManager fileManager,
            OptionalTestAdapterParametersContainer testAdapterParameters)
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
