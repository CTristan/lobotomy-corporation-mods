// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Interfaces;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class NothingThereEvaluator : CreatureEvaluator
    {
        internal NothingThereEvaluator(IAgentData agent,
            ICreatureData creature,
            RwbpType skillType) : base(agent, creature, skillType)
        {
        }

        private bool IsDisguised()
        {
            Nothing nothingThere = (Nothing)Creature.script;
            bool isDisguised = nothingThere.copiedWorker.IsNotNull();

            return isDisguised;
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            const int MinFortitudeWhenDisguised = 4;

            return Agent.fortitudeLevel < MinFortitudeWhenDisguised && IsDisguised();
        }
    }
}
