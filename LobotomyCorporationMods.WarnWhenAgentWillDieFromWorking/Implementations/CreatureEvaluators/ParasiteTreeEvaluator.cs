// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Interfaces;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    public sealed class ParasiteTreeEvaluator : CreatureEvaluator
    {
        private readonly IYggdrasilAnimTestAdapter _yggdrasilAnimTestAdapter;

        internal ParasiteTreeEvaluator(IAgentData agent,
            ICreatureData creature,
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
