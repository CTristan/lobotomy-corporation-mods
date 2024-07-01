// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class BeautyAndTheBeastEvaluator : CreatureEvaluator
    {
        private readonly IBeautyBeastAnimTestAdapter _testAdapter;

        internal BeautyAndTheBeastEvaluator(AgentModel agent,
            CreatureModel creature,
            RwbpType skillType,
            IBeautyBeastAnimTestAdapter animationScriptTestAdapter) : base(agent, creature, skillType)
        {
            _testAdapter = animationScriptTestAdapter;
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            const int WeakenedState = 1;

            _testAdapter.GameObject = (BeautyBeastAnim)Creature.GetAnimScript();
            var animationState = _testAdapter.State;
            var isWeakened = animationState == WeakenedState;

            var agentWillDie = isWeakened && SkillType == RwbpType.P;

            return agentWillDie;
        }
    }
}
