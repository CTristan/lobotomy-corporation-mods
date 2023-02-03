// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class NothingThereEvaluator : CreatureEvaluator
    {
        internal NothingThereEvaluator(AgentModel agent, CreatureModel creature, RwbpType skillType) : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            const int MinFortitude = 4;

            return Agent.fortitudeLevel < MinFortitude;
        }
    }
}
