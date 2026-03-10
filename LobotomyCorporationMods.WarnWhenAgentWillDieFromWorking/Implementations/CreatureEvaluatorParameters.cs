// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Interfaces;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations
{
    public sealed class CreatureEvaluatorParameters
    {
        internal CreatureEvaluatorParameters(IAgentData agent,
            ICreatureData creature,
            RwbpType skillType,
            IBeautyBeastAnimTestAdapter beautyBeastAnimTestAdapter,
            IYggdrasilAnimTestAdapter yggdrasilAnimTestAdapter)
        {
            _ = Guard.Against.Null(agent, nameof(agent));
            _ = Guard.Against.Null(creature, nameof(creature));
            Agent = agent;
            Creature = creature;
            SkillType = skillType;
            BeautyBeastAnimTestAdapter = beautyBeastAnimTestAdapter;
            YggdrasilAnimTestAdapter = yggdrasilAnimTestAdapter;
        }

        internal IAgentData Agent { get; private set; }
        internal ICreatureData Creature { get; private set; }
        internal RwbpType SkillType { get; private set; }
        internal IBeautyBeastAnimTestAdapter BeautyBeastAnimTestAdapter { get; private set; }
        internal IYggdrasilAnimTestAdapter YggdrasilAnimTestAdapter { get; private set; }
    }
}
