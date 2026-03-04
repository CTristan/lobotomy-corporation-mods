// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace LobotomyCorporationMods.Common.Extensions
{
    internal static class UnitModelExtensions
    {
        /// <summary>A unit's equipped gifts consists of both added and replaced gifts.</summary>
        [NotNull]
        internal static List<EGOgiftModel> GetEquippedGifts([NotNull] this UnitModel unitModel)
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

        internal static EGOgiftModel FindGiftAtPosition([NotNull] this UnitModel unitModel,
            string position)
        {
            var equippedGifts = unitModel.GetEquippedGifts();

            return equippedGifts.Find(g => g.metaInfo.attachPos == position);
        }

        internal static bool IsGiftLocked([NotNull] this UnitModel unitModel,
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
