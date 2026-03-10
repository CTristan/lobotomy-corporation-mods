// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Interfaces;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
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
