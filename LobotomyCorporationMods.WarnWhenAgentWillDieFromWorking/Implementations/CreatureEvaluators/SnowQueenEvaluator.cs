// SPDX-License-Identifier: MIT

using Hemocode.Common.Enums;
using Hemocode.WarnWhenAgentWillDieFromWorking.Interfaces;

namespace Hemocode.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    public sealed class SnowQueenEvaluator : CreatureEvaluator
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
