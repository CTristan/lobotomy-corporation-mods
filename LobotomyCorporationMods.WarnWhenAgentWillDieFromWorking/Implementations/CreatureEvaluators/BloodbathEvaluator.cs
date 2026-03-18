// SPDX-License-Identifier: MIT

using Hemocode.WarnWhenAgentWillDieFromWorking.Interfaces;

namespace Hemocode.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    public sealed class BloodbathEvaluator : CreatureEvaluator
    {
        internal BloodbathEvaluator(IAgentData agent,
            ICreatureData creature,
            RwbpType skillType) : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            return Agent.fortitudeLevel == 1 || Agent.temperanceLevel == 1;
        }
    }
}
