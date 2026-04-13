// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class BeautyAndTheBeastEvaluator : CreatureEvaluator
    {
        private readonly IBeautyBeastAnimInternals _beautyBeastAnimInternals;

        internal BeautyAndTheBeastEvaluator(
            AgentModel agent,
            CreatureModel creature,
            RwbpType skillType,
            [CanBeNull] IBeautyBeastAnimInternals animationScriptBeautyBeastAnimInternals = null
        )
            : base(agent, creature, skillType)
        {
            _beautyBeastAnimInternals = animationScriptBeautyBeastAnimInternals;
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            var isWeakened = Creature.IsBeautyAndTheBeastWeakened(_beautyBeastAnimInternals);
            var agentWillDie = isWeakened && SkillType == RwbpType.P;

            return agentWillDie;
        }
    }
}
