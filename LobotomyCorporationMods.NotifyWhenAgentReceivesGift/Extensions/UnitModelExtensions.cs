// SPDX-License-Identifier: MIT

#region

using System.Linq;
using LobotomyCorporationMods.Common.Extensions;

#endregion

namespace LobotomyCorporationMods.NotifyWhenAgentReceivesGift.Extensions
{
    internal static class UnitModelExtensions
    {
        internal static bool HasGiftEquipped(this UnitModel unitModel, int giftId)
        {
            var equippedGifts = unitModel.GetEquippedGifts();

            return equippedGifts.Any(g => g.metaInfo.id == giftId);
        }

        internal static bool PositionHasLockedGift(this UnitModel unitModel, EquipmentModel gift)
        {
            var equippedGifts = unitModel.GetEquippedGifts();
            var matchingGiftAtPosition = equippedGifts.FirstOrDefault(g => g.metaInfo.attachType == gift.metaInfo.attachType && g.metaInfo.attachPos == gift.metaInfo.attachPos);

            if (matchingGiftAtPosition == null)
            {
                return false;
            }

            var lockStateDictionary = unitModel.Equipment.gifts.lockState;
            var matchingGiftLockState = lockStateDictionary.Values.FirstOrDefault(v => v.id == matchingGiftAtPosition.metaInfo.id);

            return matchingGiftLockState is { state: true };
        }
    }
}
