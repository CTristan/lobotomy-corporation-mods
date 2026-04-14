// SPDX-License-Identifier: MIT

using LobotomyCorporation.Mods.Common;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations
{
    internal sealed class CreatureEvaluatorParameters
    {
        internal CreatureEvaluatorParameters(
            AgentModel agent,
            CreatureModel creature,
            RwbpType skillType,
            IBeautyBeastAnimInternals beautyBeastAnimInternals,
            IYggdrasilAnimInternals yggdrasilAnimInternals
        )
        {
            Agent = agent;
            Creature = creature;
            SkillType = skillType;
            BeautyBeastAnimInternals = beautyBeastAnimInternals;
            YggdrasilAnimInternals = yggdrasilAnimInternals;
        }

        internal AgentModel Agent { get; private set; }
        internal CreatureModel Creature { get; private set; }
        internal RwbpType SkillType { get; private set; }
        internal IBeautyBeastAnimInternals BeautyBeastAnimInternals { get; private set; }
        internal IYggdrasilAnimInternals YggdrasilAnimInternals { get; private set; }
    }
}
