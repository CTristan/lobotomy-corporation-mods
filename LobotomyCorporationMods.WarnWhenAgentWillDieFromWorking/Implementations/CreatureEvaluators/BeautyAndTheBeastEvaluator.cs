// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Interfaces.Adapters;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class BeautyAndTheBeastEvaluator : CreatureEvaluator
    {
        private readonly IAnimationScriptAdapter _animationScriptAdapter;

        internal BeautyAndTheBeastEvaluator(AgentModel agent, CreatureModel creature, RwbpType skillType, IAnimationScriptAdapter animationScriptAdapter) : base(agent, creature, skillType)
        {
            _animationScriptAdapter = animationScriptAdapter;
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            var agentWillDie = false;

            var animationScript = _animationScriptAdapter.GetScript<BeautyBeastAnim>(Creature);
            if (!(animationScript is null))
            {
                const int WeakenedState = 1;
                var animationState = _animationScriptAdapter.BeautyAndTheBeastState;
                var isWeakened = animationState == WeakenedState;

                agentWillDie = isWeakened && SkillType == RwbpType.P;
            }

            return agentWillDie;
        }
    }
}
