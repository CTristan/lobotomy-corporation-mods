// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Interfaces;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    public sealed class BlueStarEvaluator : CreatureEvaluator
    {
        internal BlueStarEvaluator(IAgentData agent,
            ICreatureData creature,
            RwbpType skillType) : base(agent, creature, skillType)
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
