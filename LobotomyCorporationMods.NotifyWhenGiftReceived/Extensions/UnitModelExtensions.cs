// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace LobotomyCorporationMods.NotifyWhenGiftReceived.Extensions
{
    internal static class UnitModelExtensions
    {
        internal static bool HasGiftEquipped([NotNull] this UnitModel unitModel, int id)
        {
            var equippedGifts = unitModel.GetEquippedGifts();

            return equippedGifts.Any(g => g.metaInfo.id == id);
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

        private static IEnumerable<EGOgiftModel> GetEquippedGifts([NotNull] this UnitModel unitModel)
        {
            return unitModel.Equipment.gifts.addedGifts;
        }
    }
}
