// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Interfaces;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    /// <summary>Does not perform any creature-specific checks but will still run the other-abnormality checks in the base evaluator classes.</summary>
    internal sealed class DefaultEvaluator : CreatureEvaluator
    {
        internal DefaultEvaluator(IAgentData agent,
            ICreatureData creature,
            RwbpType skillType) : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            return false;
        }
    }
}
