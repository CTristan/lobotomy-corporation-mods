// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class ParasiteTreeEvaluator : CreatureEvaluator
    {
        private readonly IYggdrasilAnimInternals _yggdrasilAnimInternals;

        internal ParasiteTreeEvaluator(
            AgentModel agent,
            CreatureModel creature,
            RwbpType skillType,
            [CanBeNull] IYggdrasilAnimInternals yggdrasilAnimInternals = null
        )
            : base(agent, creature, skillType)
        {
            _yggdrasilAnimInternals = yggdrasilAnimInternals;
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            const int MaxNumberOfFlowers = 4;

            var numberOfFlowers = Creature.GetParasiteTreeNumberOfFlowers(_yggdrasilAnimInternals);
            var agentWillDie =
                numberOfFlowers >= MaxNumberOfFlowers && !Agent.HasParasiteTreeEffect();

            return agentWillDie;
        }
    }
}
