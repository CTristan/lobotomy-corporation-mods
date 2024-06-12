// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class NothingThereEvaluator : CreatureEvaluator
    {
        internal NothingThereEvaluator(AgentModel agent, CreatureModel creature, RwbpType skillType)
            : base(agent, creature, skillType)
        {
        }

        private bool IsDisguised()
        {
            var nothingThere = Creature.script as Nothing;
            var isDisguised = nothingThere?.copiedWorker is object;

            return isDisguised;
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            const int MinFortitudeWhenDisguised = 4;

            return Agent.fortitudeLevel < MinFortitudeWhenDisguised && IsDisguised();
        }
    }
}
