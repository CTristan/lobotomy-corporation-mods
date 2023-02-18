// SPDX-License-Identifier: MIT

#region

using System.Linq;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Implementations;

#endregion

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
    }
}
