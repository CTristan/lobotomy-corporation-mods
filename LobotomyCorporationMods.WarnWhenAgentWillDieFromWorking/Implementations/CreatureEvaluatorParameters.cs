// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Interfaces.Adapters;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations
{
    internal sealed class CreatureEvaluatorParameters
    {
        internal CreatureEvaluatorParameters(AgentModel agent,
            CreatureModel creature,
            RwbpType skillType,
            IBeautyBeastAnimTestAdapter beautyBeastAnimTestAdapter,
            IYggdrasilAnimTestAdapter yggdrasilAnimTestAdapter)
        {
            Agent = agent;
            Creature = creature;
            SkillType = skillType;
            BeautyBeastAnimTestAdapter = beautyBeastAnimTestAdapter;
            YggdrasilAnimTestAdapter = yggdrasilAnimTestAdapter;
        }

        internal AgentModel Agent { get; private set; }
        internal CreatureModel Creature { get; private set; }
        internal RwbpType SkillType { get; private set; }
        internal IBeautyBeastAnimTestAdapter BeautyBeastAnimTestAdapter { get; private set; }
        internal IYggdrasilAnimTestAdapter YggdrasilAnimTestAdapter { get; private set; }
    }
}
