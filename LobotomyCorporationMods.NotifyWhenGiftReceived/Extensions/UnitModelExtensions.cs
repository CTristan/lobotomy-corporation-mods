// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace LobotomyCorporationMods.NotifyWhenGiftReceived.Extensions
{
    public static class UnitModelExtensions
    {
        public static bool HasGiftEquipped([NotNull] this UnitModel unitModel, int giftId)
        {
            var equippedGifts = unitModel.GetEquippedGifts();

            return equippedGifts.Any(g => g.metaInfo.id == giftId);
        }

        internal static bool PositionHasLockedGift([NotNull] this UnitModel unitModel, [NotNull] EquipmentModel gift)
        {
            var equippedGifts = unitModel.GetEquippedGifts();
            var matchingGiftAtPosition = equippedGifts.FirstOrDefault(g => g.metaInfo.attachType == gift.metaInfo.attachType && g.metaInfo.attachPos == gift.metaInfo.attachPos);

            if (matchingGiftAtPosition == null)
            {
                return false;
            }

            var lockStateDictionary = unitModel.Equipment.gifts.lockState;
            var matchingGiftLockState = lockStateDictionary.Values.FirstOrDefault(v => v.id == matchingGiftAtPosition.metaInfo.id);

            return matchingGiftLockState != null && matchingGiftLockState.state;
        }

        /// <summary>
        ///     A unit's equipped gifts consists of both added and replaced gifts.
        /// </summary>
        private static IEnumerable<EGOgiftModel> GetEquippedGifts([NotNull] this UnitModel unitModel)
        {
            var giftList = unitModel.Equipment.gifts.addedGifts;
            giftList.AddRange(unitModel.Equipment.gifts.replacedGifts);

            return giftList;
        }
    }
}
