// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Enums;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class SnowQueenEvaluator : CreatureEvaluator
    {
        internal SnowQueenEvaluator(AgentModel agent,
            CreatureModel creature,
            RwbpType skillType) : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            return Agent.HasEquipment((int)EquipmentIds.FirebirdArmor);
        }
    }
}
