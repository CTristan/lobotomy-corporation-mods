// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Interfaces.Adapters;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations
{
    internal sealed class CreatureEvaluatorParameters
    {
        internal CreatureEvaluatorParameters(AgentModel agent,
            CreatureModel creature,
            RwbpType skillType,
            IBeautyBeastAnimAdapter beautyBeastAnimAdapter,
            IYggdrasilAnimAdapter yggdrasilAnimAdapter)
        {
            Agent = agent;
            Creature = creature;
            SkillType = skillType;
            BeautyBeastAnimAdapter = beautyBeastAnimAdapter;
            YggdrasilAnimAdapter = yggdrasilAnimAdapter;
        }

        internal AgentModel Agent { get; private set; }
        internal CreatureModel Creature { get; private set; }
        internal RwbpType SkillType { get; private set; }
        internal IBeautyBeastAnimAdapter BeautyBeastAnimAdapter { get; private set; }
        internal IYggdrasilAnimAdapter YggdrasilAnimAdapter { get; private set; }
    }
}
