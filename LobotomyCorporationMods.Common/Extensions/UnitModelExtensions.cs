// SPDX-License-Identifier: MIT

using System.Linq;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class UnitModelExtensions
    {
        public static bool HasBuffOfType<TBuff>([NotNull] this AgentModel agent) where TBuff : UnitBuf
        {
            Guard.Against.Null(agent, nameof(agent));

            var buffs = agent.GetUnitBufList();

            return buffs.OfType<TBuff>().Any();
        }

        public static bool HasCrumblingArmor([NotNull] this AgentModel agent)
        {
            Guard.Against.Null(agent, nameof(agent));

            return agent.HasEquipment((int)EquipmentId.CrumblingArmorGift1) || agent.HasEquipment((int)EquipmentId.CrumblingArmorGift2) || agent.HasEquipment((int)EquipmentId.CrumblingArmorGift3) ||
                   agent.HasEquipment((int)EquipmentId.CrumblingArmorGift4);
        }

        public static bool HasEquipment([NotNull] this UnitModel agent, EquipmentId equipmentId)
        {
            Guard.Against.Null(agent, nameof(agent));
            Guard.Against.Null(equipmentId, nameof(equipmentId));

            return agent.HasEquipment((int)equipmentId);
        }
    }
}
