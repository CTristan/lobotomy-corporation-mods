// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Extensions;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class NothingThereEvaluator : CreatureEvaluator
    {
        internal NothingThereEvaluator(AgentModel agent,
            CreatureModel creature,
            RwbpType skillType) : base(agent, creature, skillType)
        {
        }

        private bool IsDisguised()
        {
            var nothingThere = (Nothing)Creature.script;
            var isDisguised = nothingThere.copiedWorker.IsNotNull();

            return isDisguised;
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            const int MinFortitudeWhenDisguised = 4;

            return Agent.fortitudeLevel < MinFortitudeWhenDisguised && IsDisguised();
        }
    }
}
