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
        private readonly IYggdrasilAnimTestAdapter _yggdrasilAnimTestAdapter;

        internal ParasiteTreeEvaluator(AgentModel agent,
            CreatureModel creature,
            RwbpType skillType,
            IYggdrasilAnimTestAdapter yggdrasilAnimTestAdapter) : base(agent, creature, skillType)
        {
            _yggdrasilAnimTestAdapter = yggdrasilAnimTestAdapter;
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            const int MaxNumberOfFlowers = 4;

            var numberOfFlowers = _yggdrasilAnimTestAdapter.Flowers.Count(flower => flower.ActiveSelf);
            var agentWillDie = numberOfFlowers >= MaxNumberOfFlowers && !Agent.HasParasiteTreeEffect();

            return agentWillDie;
        }
    }
}
