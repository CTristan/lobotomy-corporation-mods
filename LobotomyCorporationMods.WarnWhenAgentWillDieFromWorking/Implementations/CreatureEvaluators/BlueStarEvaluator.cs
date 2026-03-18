// SPDX-License-Identifier: MIT

using Hemocode.WarnWhenAgentWillDieFromWorking.Interfaces;

namespace Hemocode.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
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
