// SPDX-License-Identifier: MIT

using System.Linq;
using JetBrains.Annotations;

namespace LobotomyCorporationMods.NotifyWhenGiftReceived.Extensions
{
    internal static class UnitModelExtensions
    {
        internal static bool HasGiftEquipped([NotNull] this UnitModel unitModel, int id)
        {
            var equippedGifts = unitModel.Equipment.gifts.addedGifts;

            return equippedGifts.Any(g => g.metaInfo.id == id);
        }
    }
}
