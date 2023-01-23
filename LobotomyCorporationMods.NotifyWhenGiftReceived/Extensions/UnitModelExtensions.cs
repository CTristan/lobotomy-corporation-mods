// SPDX-License-Identifier: MIT

using System.Linq;

namespace LobotomyCorporationMods.NotifyWhenGiftReceived.Extensions
{
    internal static class UnitModelExtensions
    {
        public static bool HasGiftEquipped(this UnitModel unitModel, int id)
        {
            var equippedGifts = unitModel.Equipment.gifts.addedGifts;

            return equippedGifts.Any(g => g.metaInfo.id == id);
        }
    }
}
