// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Linq;
using LobotomyCorporationMods.Common.Enums;

#endregion

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class UnitModelExtensions
    {
        /// <summary>
        ///     A unit's equipped gifts consists of both added and replaced gifts.
        /// </summary>
        public static IEnumerable<EGOgiftModel> GetEquippedGifts(this UnitModel unitModel)
        {
            var giftList = new List<EGOgiftModel>();

            var addedGifts = unitModel.Equipment.gifts.addedGifts;
            if (!(addedGifts is null) && addedGifts.Count > 0)
            {
                giftList.AddRange(addedGifts);
            }

            var replacedGifts = unitModel.Equipment.gifts.replacedGifts;
            if (!(replacedGifts is null) && replacedGifts.Count > 0)
            {
                giftList.AddRange(unitModel.Equipment.gifts.replacedGifts);
            }

            return giftList;
        }

        public static bool HasBuffOfType<TBuff>(this UnitModel agent) where TBuff : UnitBuf
        {
            var buffs = agent.GetUnitBufList();

            return buffs.OfType<TBuff>().Any();
        }

        public static bool HasCrumblingArmor(this UnitModel agent)
        {
            return agent.HasEquipment((int)EquipmentId.CrumblingArmorGift1) || agent.HasEquipment((int)EquipmentId.CrumblingArmorGift2) || agent.HasEquipment((int)EquipmentId.CrumblingArmorGift3) ||
                   agent.HasEquipment((int)EquipmentId.CrumblingArmorGift4);
        }
    }
}
