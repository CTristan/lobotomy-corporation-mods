// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class BeautyAndTheBeastEvaluator : CreatureEvaluator
    {
        private readonly IBeautyBeastAnimAdapter _adapter;

        internal BeautyAndTheBeastEvaluator(AgentModel agent,
            CreatureModel creature,
            RwbpType skillType,
            IBeautyBeastAnimAdapter animationScriptAdapter) : base(agent, creature, skillType)
        {
            _adapter = animationScriptAdapter;
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            const int WeakenedState = 1;

            _adapter.GameObject = (BeautyBeastAnim)Creature.GetAnimScript();
            var animationState = _adapter.State;
            var isWeakened = animationState == WeakenedState;

            var agentWillDie = isWeakened && SkillType == RwbpType.P;

            return agentWillDie;
        }
    }
}
