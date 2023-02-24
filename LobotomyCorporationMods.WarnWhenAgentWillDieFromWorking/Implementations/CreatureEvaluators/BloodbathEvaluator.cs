// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class BloodbathEvaluator : CreatureEvaluator
    {
        internal BloodbathEvaluator(AgentModel agent, CreatureModel creature, RwbpType skillType)
            : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            return Agent.fortitudeLevel == 1 || Agent.temperanceLevel == 1;
        }
    }
}
