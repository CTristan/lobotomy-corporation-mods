// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Interfaces;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class SpiderBudEvaluator : CreatureEvaluator
    {
        internal SpiderBudEvaluator(IAgentData agent,
            ICreatureData creature,
            RwbpType skillType) : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            return Agent.prudenceLevel == 1 || SkillType == RwbpType.W;
        }
    }
}
