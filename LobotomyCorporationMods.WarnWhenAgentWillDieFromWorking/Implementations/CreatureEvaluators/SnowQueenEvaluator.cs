// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Interfaces;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class SnowQueenEvaluator : CreatureEvaluator
    {
        internal SnowQueenEvaluator(IAgentData agent,
            ICreatureData creature,
            RwbpType skillType) : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            return Agent.HasEquipment((int)EquipmentIds.FirebirdArmor);
        }
    }
}
