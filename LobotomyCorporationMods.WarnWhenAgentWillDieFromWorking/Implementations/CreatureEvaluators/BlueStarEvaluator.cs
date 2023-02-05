// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class BlueStarEvaluator : CreatureEvaluator
    {
        internal BlueStarEvaluator(AgentModel agent, CreatureModel creature, RwbpType skillType) : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            const int MinPrudence = 5;
            const int MinTemperance = 4;

            return Agent.prudenceLevel < MinPrudence || Agent.temperanceLevel < MinTemperance;
        }
    }
}
