// SPDX-License-Identifier: MIT

using Hemocode.Common.Implementations;
using Hemocode.Common.Interfaces.Adapters;
using Hemocode.WarnWhenAgentWillDieFromWorking.Interfaces;

namespace Hemocode.WarnWhenAgentWillDieFromWorking.Implementations
{
    public sealed class CreatureEvaluatorParameters
    {
        internal CreatureEvaluatorParameters(IAgentData agent,
            ICreatureData creature,
            RwbpType skillType,
            IBeautyBeastAnimTestAdapter beautyBeastAnimTestAdapter,
            IYggdrasilAnimTestAdapter yggdrasilAnimTestAdapter)
        {
            ThrowHelper.ThrowIfNull(agent, nameof(agent));
            ThrowHelper.ThrowIfNull(creature, nameof(creature));
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
