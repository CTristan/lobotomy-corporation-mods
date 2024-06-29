// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Linq;
using LobotomyCorporationMods.Common.Extensions;

#endregion

namespace LobotomyCorporationMods.NotifyWhenAgentReceivesGift.Extensions
{
    internal static class UnitModelExtensions
    {
        /// <summary>A unit's equipped gifts consists of both added and replaced gifts.</summary>
        private static List<EGOgiftModel> GetEquippedGifts(this UnitModel unitModel)
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

        internal static bool HasGiftEquipped(this UnitModel unitModel,
            int giftId)
        {
            var equippedGifts = unitModel.GetEquippedGifts();

            return equippedGifts.Exists(g => g.metaInfo.id == giftId);
        }

        internal static bool PositionHasLockedGift(this UnitModel unitModel,
            EquipmentModel gift)
        {
            var matchingGiftAtPosition = FindGiftAtPosition(unitModel, gift.metaInfo.attachPos);

            return !matchingGiftAtPosition.IsNull() && IsGiftLocked(unitModel, matchingGiftAtPosition.metaInfo.id);
        }

        private static EGOgiftModel FindGiftAtPosition(this UnitModel unitModel,
            string position)
        {
            var equippedGifts = unitModel.GetEquippedGifts();
            return equippedGifts.Find(g => g.metaInfo.attachPos == position);
        }

        private static bool IsGiftLocked(this UnitModel unitModel,
            int giftId)
        {
            var matchingGiftLockState = unitModel.GetMatchingGiftLockState(giftId);

            return matchingGiftLockState.state;
        }

        private static UnitEGOgiftSpace.GiftLockState GetMatchingGiftLockState(this UnitModel unitModel,
            int giftId)
        {
            var lockStateDictionary = unitModel.Equipment.gifts.lockState;

            var lockState = lockStateDictionary.Values.FirstOrDefault(v => v.id == giftId);

            return lockState ?? new UnitEGOgiftSpace.GiftLockState();
        }
    }
}
