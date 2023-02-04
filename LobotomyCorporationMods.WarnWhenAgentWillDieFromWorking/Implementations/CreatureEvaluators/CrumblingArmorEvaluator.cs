// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class CrumblingArmorEvaluator : CreatureEvaluator
    {
        internal CrumblingArmorEvaluator(AgentModel agent, CreatureModel creature, RwbpType skillType) : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            return Agent.fortitudeLevel == 1 || AgentHasGiftAndWillPerformAttachmentWork();
        }

        private bool AgentHasGiftAndWillPerformAttachmentWork()
        {
            return Agent.HasCrumblingArmor() && SkillType == RwbpType.B;
        }
    }
}