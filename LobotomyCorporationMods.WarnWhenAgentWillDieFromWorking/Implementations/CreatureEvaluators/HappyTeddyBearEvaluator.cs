// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class HappyTeddyBearEvaluator : CreatureEvaluator
    {
        internal HappyTeddyBearEvaluator(AgentModel agent, CreatureModel creature, RwbpType skillType)
            : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            var agentWillDie = false;

            var script = (HappyTeddy)Creature.script;
            if (script.lastAgent is not null)
            {
                agentWillDie = Agent.instanceId == script.lastAgent.instanceId;
            }

            return agentWillDie;
        }
    }
}
