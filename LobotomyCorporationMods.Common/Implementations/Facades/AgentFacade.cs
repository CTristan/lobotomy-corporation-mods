// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Extensions;

namespace LobotomyCorporationMods.Common.Implementations.Facades
{
    public static class AgentFacade
    {
        public static long GetAgentId([NotNull] this UseSkill useSkill)
        {
            return useSkill.GetAgent().instanceId;
        }

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

        public static bool HasCrumblingArmor([NotNull] this UnitModel agent)
        {
            Guard.Against.Null(agent, nameof(agent));

            var crumblingArmorGiftsId = new List<int>
            {
                (int)EquipmentId.CrumblingArmorGift1,
                (int)EquipmentId.CrumblingArmorGift2,
                (int)EquipmentId.CrumblingArmorGift3,
                (int)EquipmentId.CrumblingArmorGift4,
            };

            return crumblingArmorGiftsId.Exists(agent.HasEquipment);
        }

        public static bool HasParasiteTreeEffect([NotNull] this UnitModel agent)
        {
            Guard.Against.Null(agent, nameof(agent));
            var effects = agent.GetUnitBufList();
            return effects.OfType<YggdrasilBlessBuf>().Any();
        }
    }
}
