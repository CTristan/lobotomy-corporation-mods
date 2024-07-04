// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;

namespace LobotomyCorporationMods.Common.Implementations.Facades
{
    public static class GiftFacade
    {
        public static bool HasGiftEquipped([NotNull] this UnitModel unitModel,
            int giftId)
        {
            Guard.Against.Null(unitModel, nameof(unitModel));

            var equippedGifts = unitModel.GetEquippedGifts();

            return equippedGifts.Exists(g => g.metaInfo.id == giftId);
        }

        /// <summary>Some gifts are in special slots that don't show up in an agent's gift window and are used for abnormality effect, for example Snow Queen's icicle</summary>
        /// <param name="equipmentModel">The equipment to check.</param>
        /// <returns>True if the equipment is in a valid slot, otherwise false.</returns>
        public static bool IsInValidSlot([NotNull] this EquipmentModel equipmentModel)
        {
            Guard.Against.Null(equipmentModel, nameof(equipmentModel));

            return equipmentModel.metaInfo.attachPos != EGOgiftAttachRegion.BODY_UP.ToString();
        }

        public static bool PositionHasLockedGift([NotNull] this UnitModel unitModel,
            [NotNull] EquipmentModel gift)
        {
            Guard.Against.Null(gift, nameof(gift));

            var matchingGiftAtPosition = unitModel.FindGiftAtPosition(gift.metaInfo.attachPos);

            return !matchingGiftAtPosition.IsNull() && unitModel.IsGiftLocked(matchingGiftAtPosition.metaInfo.id);
        }
    }
}
