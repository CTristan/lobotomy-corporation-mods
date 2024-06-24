// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class RedShoesEvaluator : CreatureEvaluator
    {
        internal RedShoesEvaluator(AgentModel agent, CreatureModel creature, RwbpType skillType)
            : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            const int MinTemperance = 3;

            return Agent.temperanceLevel < MinTemperance;
        }
    }
}
