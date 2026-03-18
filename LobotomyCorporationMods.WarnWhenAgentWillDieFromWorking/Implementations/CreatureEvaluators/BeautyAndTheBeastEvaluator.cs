// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;
using Hemocode.Common.Interfaces.Adapters;
using Hemocode.WarnWhenAgentWillDieFromWorking.Interfaces;

#endregion

namespace Hemocode.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    public sealed class BeautyAndTheBeastEvaluator : CreatureEvaluator
    {
        private readonly IBeautyBeastAnimTestAdapter _beautyBeastAnimTestAdapter;

        internal BeautyAndTheBeastEvaluator(IAgentData agent,
            ICreatureData creature,
            RwbpType skillType,
            [CanBeNull] IBeautyBeastAnimTestAdapter animationScriptBeautyBeastAnimTestAdapter = null) : base(agent, creature, skillType)
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
