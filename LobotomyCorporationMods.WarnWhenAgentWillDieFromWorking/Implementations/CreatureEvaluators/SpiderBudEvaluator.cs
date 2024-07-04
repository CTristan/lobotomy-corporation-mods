// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class SpiderBudEvaluator : CreatureEvaluator
    {
        internal SpiderBudEvaluator(AgentModel agent,
            CreatureModel creature,
            RwbpType skillType) : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            return Agent.prudenceLevel == 1 || SkillType == RwbpType.W;
        }
    }
}
