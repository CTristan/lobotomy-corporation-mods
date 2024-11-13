// SPDX-License-Identifier: MIT

using System;
using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;

namespace LobotomyCorporationMods.Common.Implementations.Facades
{
    public static class GiftFacade
    {
        public static bool AbnormalityHasGift([NotNull] this ManagementSlot managementSlot)
        {
            return managementSlot.GetAbnormalityGift().IsNotNull();
        }

        /// <summary>Gets the attachment type of the gift for the abnormality in the management slot if one exists.</summary>
        /// <param name="managementSlot">The management slot to check.</param>
        /// <returns>The attachment type of the abnormality gift, or null if the gift doesn't exist.</returns>
        public static EGOgiftAttachType GetAbnormalityGiftAttachmentType([NotNull] this ManagementSlot managementSlot)
        {
            return managementSlot.GetAbnormalityGiftInfo()?.attachType ?? 0;
        }

        public static int? GetAbnormalityGiftId([NotNull] this ManagementSlot managementSlot)
        {
            return managementSlot.GetAbnormalityGiftInfo()?.id;
        }

        [CanBeNull]
        public static string GetAbnormalityGiftName([NotNull] this CreatureEquipmentMakeInfo creatureEquipmentMakeInfo)
        {
            return creatureEquipmentMakeInfo.GetAbnormalityGiftInfo()?.Name;
        }

        [CanBeNull]
        public static string GetAbnormalityGiftName([NotNull] this UseSkill useSkill)
        {
            return useSkill.GetAbnormalityGiftInfo()?.Name;
        }

        [CanBeNull]
        public static string GetAbnormalityGiftPosition([NotNull] this ManagementSlot managementSlot)
        {
            return managementSlot.GetAbnormalityGiftInfo()?.attachPos;
        }

        public static bool HasGift([NotNull] this UnitModel unitModel,
            int? giftId)
        {
            Guard.Against.Null(unitModel, nameof(unitModel));

            var equippedGifts = unitModel.GetEquippedGifts();

            return equippedGifts.Exists(g => g.metaInfo.id == giftId);
        }

        public static bool HasGiftInPosition([NotNull] this UnitModel unitModel,
            string positionName,
            EGOgiftAttachType attachType)
        {
            Guard.Against.Null(unitModel, nameof(unitModel));

            return unitModel.GetEquippedGifts().Exists(model => model.metaInfo.attachPos.Equals(positionName, StringComparison.OrdinalIgnoreCase) && model.metaInfo.attachType.Equals(attachType));
        }

        public static bool PositionHasLockedGift([NotNull] this UnitModel unitModel,
            [NotNull] EquipmentModel gift)
        {
            Guard.Against.Null(gift, nameof(gift));

            var matchingGiftAtPosition = unitModel.FindGiftAtPosition(gift.metaInfo);

            return !matchingGiftAtPosition.IsNull() && unitModel.IsGiftLocked(matchingGiftAtPosition.metaInfo.id);
        }
    }
}
