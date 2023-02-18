// SPDX-License-Identifier:MIT

#region

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class BeautyAndTheBeastEvaluator : CreatureEvaluator
    {
        private readonly IBeautyBeastAnimAdapter _adapter;

        internal BeautyAndTheBeastEvaluator([NotNull] AgentModel agent, [NotNull] CreatureModel creature, RwbpType skillType, [CanBeNull] IBeautyBeastAnimAdapter animationScriptAdapter)
            : base(agent, creature, skillType)
        {
            _adapter = animationScriptAdapter ?? new BeautyBeastAnimAdapter(creature.GetAnimScript() as BeautyBeastAnim);
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            const int WeakenedState = 1;
            var animationState = _adapter.GetState();
            var isWeakened = animationState == WeakenedState;

            var agentWillDie = isWeakened && SkillType == RwbpType.P;

            return agentWillDie;
        }
    }
}
