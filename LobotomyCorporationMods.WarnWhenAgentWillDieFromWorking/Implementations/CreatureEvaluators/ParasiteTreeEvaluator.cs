// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common;
using LobotomyCorporationMods.Common.Interfaces;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class ParasiteTreeEvaluator : CreatureEvaluator
    {
        private readonly IAnimationScriptAdapter _animationScriptAdapter;

        internal ParasiteTreeEvaluator(AgentModel agent, CreatureModel creature, RwbpType skillType, IAnimationScriptAdapter animationScriptAdapter) : base(agent, creature, skillType)
        {
            _animationScriptAdapter = animationScriptAdapter;
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            var agentWillDie = false;

            var animationScript = _animationScriptAdapter.GetScript<YggdrasilAnim>(Creature);
            if (!(animationScript is null))
            {
                var numberOfFlowers = _animationScriptAdapter.ParasiteTreeNumberOfFlowers;
                const int MaxNumberOfFlowers = 4;

                agentWillDie = numberOfFlowers >= MaxNumberOfFlowers && !Agent.HasBuffOfType<YggdrasilBlessBuf>();
            }

            return agentWillDie;
        }
    }
}
