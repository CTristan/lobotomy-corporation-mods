// SPDX-License-Identifier: MIT

using LobotomyCorporation.Mods.Common.Extensions;
using Hemocode.WarnWhenAgentWillDieFromWorking.Interfaces;

namespace Hemocode.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    public sealed class HappyTeddyBearEvaluator : CreatureEvaluator
    {
        internal HappyTeddyBearEvaluator(IAgentData agent,
            ICreatureData creature,
            RwbpType skillType) : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            var agentWillDie = false;

            HappyTeddy script = (HappyTeddy)Creature.script;
            if (script.lastAgent.IsNotNull())
            {
                agentWillDie = Agent.instanceId == script.lastAgent.instanceId;
            }

            return agentWillDie;
        }
    }
}
