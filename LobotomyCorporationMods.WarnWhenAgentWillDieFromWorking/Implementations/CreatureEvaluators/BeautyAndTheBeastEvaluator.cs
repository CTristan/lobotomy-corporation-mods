// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common.Implementations.Facades;
using LobotomyCorporation.Mods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class BeautyAndTheBeastEvaluator : CreatureEvaluator
    {
        private readonly IBeautyBeastAnimTestAdapter _beautyBeastAnimTestAdapter;

        internal BeautyAndTheBeastEvaluator(
            AgentModel agent,
            CreatureModel creature,
            RwbpType skillType,
            [CanBeNull] IBeautyBeastAnimTestAdapter animationScriptBeautyBeastAnimTestAdapter = null
        )
            : base(agent, creature, skillType)
        {
            _beautyBeastAnimTestAdapter = animationScriptBeautyBeastAnimTestAdapter;
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            var isWeakened = Creature.IsBeautyAndTheBeastWeakened(_beautyBeastAnimTestAdapter);
            var agentWillDie = isWeakened && SkillType == RwbpType.P;

            return agentWillDie;
        }
    }
}
