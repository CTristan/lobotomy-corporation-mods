// SPDX-License-Identifier: MIT

using Hemocode.Common.Extensions;
using Hemocode.WarnWhenAgentWillDieFromWorking.Interfaces;

namespace Hemocode.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    public sealed class NothingThereEvaluator : CreatureEvaluator
    {
        internal NothingThereEvaluator(IAgentData agent,
            ICreatureData creature,
            RwbpType skillType) : base(agent, creature, skillType)
        {
        }

        private bool IsDisguised()
        {
            Nothing nothingThere = (Nothing)Creature.script;
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
