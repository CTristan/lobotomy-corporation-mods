// SPDX-License-Identifier: MIT

#region

using System;

#endregion

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

            var script = Creature.script as HappyTeddy;
            _ = script ?? throw new InvalidOperationException(nameof(script));

            if (script.lastAgent is not null)
            {
                agentWillDie = Agent.instanceId == script.lastAgent.instanceId;
            }

            return agentWillDie;
        }
    }
}
