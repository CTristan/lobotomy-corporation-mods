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
            bool isDisguised;

            if (Creature.script is Nothing nothing)
            {
                var notDisguised = nothing.copiedWorker is null;
                isDisguised = !notDisguised;
            }
            else
            {
                isDisguised = false;
            }

            return isDisguised;
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            const int MinFortitude = 4;

            return Agent.fortitudeLevel < MinFortitude || IsDisguised();
        }
    }
}
