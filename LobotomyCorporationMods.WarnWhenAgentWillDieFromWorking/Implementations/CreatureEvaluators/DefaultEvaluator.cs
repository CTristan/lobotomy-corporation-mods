// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    /// <summary>
    ///     Does not perform any creature-specific checks but will still run the other-abnormality checks in the base evaluator
    ///     class.
    /// </summary>
    internal sealed class DefaultEvaluator : CreatureEvaluator
    {
        internal DefaultEvaluator(AgentModel agent, CreatureModel creature, RwbpType skillType) : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            return false;
        }
    }
}
