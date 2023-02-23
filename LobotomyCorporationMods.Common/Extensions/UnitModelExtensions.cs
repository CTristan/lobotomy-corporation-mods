// SPDX-License-Identifier: MIT

#region

using System.Linq;
using LobotomyCorporationMods.Common.Enums;

#endregion

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class UnitModelExtensions
    {
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
