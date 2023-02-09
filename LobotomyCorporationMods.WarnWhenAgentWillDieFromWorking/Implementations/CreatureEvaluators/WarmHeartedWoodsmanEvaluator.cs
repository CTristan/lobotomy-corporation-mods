// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class WarmHeartedWoodsmanEvaluator : CreatureEvaluator
    {
        internal WarmHeartedWoodsmanEvaluator(AgentModel agent, CreatureModel creature, RwbpType skillType) : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            return Creature.qliphothCounter == 0;
        }
    }
}
