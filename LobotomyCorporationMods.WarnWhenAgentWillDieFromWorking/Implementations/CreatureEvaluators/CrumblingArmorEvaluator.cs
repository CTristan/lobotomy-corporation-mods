// SPDX-License-Identifier: MIT

using Hemocode.WarnWhenAgentWillDieFromWorking.Interfaces;

namespace Hemocode.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    public sealed class CrumblingArmorEvaluator : CreatureEvaluator
    {
        internal CrumblingArmorEvaluator(IAgentData agent,
            ICreatureData creature,
            RwbpType skillType) : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            // There is an additional check for the gift in the "Other Abnormalities" check in the base class

            return Agent.fortitudeLevel == 1;
        }
    }
}
