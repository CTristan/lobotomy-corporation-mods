// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class VoidDreamEvaluator : CreatureEvaluator
    {
        internal VoidDreamEvaluator(AgentModel agent, CreatureModel creature, RwbpType skillType) : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            const int MinTemperance = 2;

            return Agent.temperanceLevel < MinTemperance;
        }
    }
}
