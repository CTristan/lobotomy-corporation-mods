// SPDX-License-Identifier: MIT

#region

using System.Linq;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class ParasiteTreeEvaluator : CreatureEvaluator
    {
        private readonly IYggdrasilAnimAdapter _yggdrasilAnimAdapter;

        internal ParasiteTreeEvaluator(AgentModel agent, CreatureModel creature, RwbpType skillType, IYggdrasilAnimAdapter yggdrasilAnimAdapter)
            : base(agent, creature, skillType)
        {
            _yggdrasilAnimAdapter = yggdrasilAnimAdapter;
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            const int MaxNumberOfFlowers = 4;

            var numberOfFlowers = _yggdrasilAnimAdapter.Flowers.Count(static flower => flower.ActiveSelf);
            var agentWillDie = numberOfFlowers >= MaxNumberOfFlowers && !Agent.HasBuffOfType<YggdrasilBlessBuf>();

            return agentWillDie;
        }
    }
}
