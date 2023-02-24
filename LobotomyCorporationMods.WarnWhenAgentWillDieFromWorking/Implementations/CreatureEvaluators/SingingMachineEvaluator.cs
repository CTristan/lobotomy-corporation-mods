// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class SingingMachineEvaluator : CreatureEvaluator
    {
        internal SingingMachineEvaluator(AgentModel agent, CreatureModel creature, RwbpType skillType)
            : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            /*
             Singing Machine's gift increases the Fortitude stat, and since the kill agent check is at
             the end of the work session it's possible for the agent to get the gift which increases the
             Fortitude level from 3 to 4 which then kills the agent. To account for that we look at the actual stat value instead
             of the level.
             */

            const int GiftIncrease = 8;
            const int FortitudeThreshold = 65;
            const int MinTemperance = 3;
            var fortitudeStatTooHigh = Agent.fortitudeStat >= FortitudeThreshold - GiftIncrease;
            var qliphothCounter = Creature.qliphothCounter;

            return qliphothCounter == 0 || fortitudeStatTooHigh || Agent.temperanceLevel < MinTemperance;
        }
    }
}
