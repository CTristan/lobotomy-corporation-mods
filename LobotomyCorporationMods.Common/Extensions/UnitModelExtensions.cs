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
        public static bool HasFairyFestivalEffect([NotNull] this UnitModel agent)
        {
            Guard.Against.Null(agent, nameof(agent));
            var effects = agent.GetUnitBufList();
            return effects.OfType<FairyBuf>().Any();
        }

        public static bool HasLaetitiaEffect([NotNull] this UnitModel agent)
        {
            Guard.Against.Null(agent, nameof(agent));
            var effects = agent.GetUnitBufList();
            return effects.OfType<LittleWitchBuf>().Any();
        }

        public static bool HasParasiteTreeEffect([NotNull] this UnitModel agent)
        {
            Guard.Against.Null(agent, nameof(agent));
            var effects = agent.GetUnitBufList();
            return effects.OfType<YggdrasilBlessBuf>().Any();
        }

        public static bool HasCrumblingArmor([NotNull] this UnitModel agent)
        {
            Guard.Against.Null(agent, nameof(agent));

            return agent.HasEquipment((int)EquipmentId.CrumblingArmorGift1) || agent.HasEquipment((int)EquipmentId.CrumblingArmorGift2) || agent.HasEquipment((int)EquipmentId.CrumblingArmorGift3) ||
                   agent.HasEquipment((int)EquipmentId.CrumblingArmorGift4);
        }
    }
}
