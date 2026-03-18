// SPDX-License-Identifier: MIT

using Hemocode.WarnWhenAgentWillDieFromWorking.Interfaces;

namespace Hemocode.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    public sealed class VoidDreamEvaluator : CreatureEvaluator
    {
        internal VoidDreamEvaluator(IAgentData agent,
            ICreatureData creature,
            RwbpType skillType) : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            const int MinTemperance = 2;

            return Agent.temperanceLevel < MinTemperance;
        }
    }
}
