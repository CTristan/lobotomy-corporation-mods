// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Interfaces;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class CrumblingArmorEvaluator : CreatureEvaluator
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
