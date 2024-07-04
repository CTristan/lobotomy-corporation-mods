// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations.Facades;
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
            [CanBeNull] IYggdrasilAnimTestAdapter yggdrasilAnimTestAdapter = null) : base(agent, creature, skillType)
        {
            _yggdrasilAnimTestAdapter = yggdrasilAnimTestAdapter;
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            const int MaxNumberOfFlowers = 4;

            var numberOfFlowers = Creature.GetParasiteTreeNumberOfFlowers(_yggdrasilAnimTestAdapter);
            var agentWillDie = numberOfFlowers >= MaxNumberOfFlowers && !Agent.HasParasiteTreeEffect();

            return agentWillDie;
        }
    }
}
