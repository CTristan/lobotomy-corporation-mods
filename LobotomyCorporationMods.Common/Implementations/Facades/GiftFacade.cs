// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;

namespace LobotomyCorporationMods.Common.Implementations.Facades
{
    public static class GiftFacade
    {
        /// <summary>A unit's equipped gifts consists of both added and replaced gifts.</summary>
        [NotNull]
        private static List<EGOgiftModel> GetEquippedGifts([NotNull] this UnitModel unitModel)
        {
            var giftList = new List<EGOgiftModel>();

            var addedGifts = unitModel.Equipment.gifts.addedGifts;
            if (addedGifts.Count > 0)
            {
                giftList.AddRange(addedGifts);
            }

            var replacedGifts = unitModel.Equipment.gifts.replacedGifts;
            if (replacedGifts.Count > 0)
            {
                giftList.AddRange(unitModel.Equipment.gifts.replacedGifts);
            }

            return giftList;
        }

        public static bool HasGiftEquipped([NotNull] this UnitModel unitModel,
            int giftId)
        {
            Guard.Against.Null(unitModel, nameof(unitModel));

            var equippedGifts = unitModel.GetEquippedGifts();

            return equippedGifts.Exists(g => g.metaInfo.id == giftId);
        }

        public static bool PositionHasLockedGift([NotNull] this UnitModel unitModel,
            [NotNull] EquipmentModel gift)
        {
            Guard.Against.Null(gift, nameof(gift));

            var matchingGiftAtPosition = FindGiftAtPosition(unitModel, gift.metaInfo.attachPos);

            return !matchingGiftAtPosition.IsNull() && IsGiftLocked(unitModel, matchingGiftAtPosition.metaInfo.id);
        }

        private static EGOgiftModel FindGiftAtPosition([NotNull] this UnitModel unitModel,
            string position)
        {
            var equippedGifts = unitModel.GetEquippedGifts();

            return equippedGifts.Find(g => g.metaInfo.attachPos == position);
        }

        private static bool IsGiftLocked([NotNull] this UnitModel unitModel,
            int giftId)
        {
            var matchingGiftLockState = unitModel.GetMatchingGiftLockState(giftId);

            return matchingGiftLockState.state;
        }

        [NotNull]
        private static UnitEGOgiftSpace.GiftLockState GetMatchingGiftLockState([NotNull] this UnitModel unitModel,
            int giftId)
        {
            var lockStateDictionary = unitModel.Equipment.gifts.lockState;
            var lockState = lockStateDictionary.Values.FirstOrDefault(v => v.id == giftId);

            return lockState ?? new UnitEGOgiftSpace.GiftLockState();
        }
    }
}
