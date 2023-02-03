// SPDX-License-Identifier: MIT

using System.Linq;
using LobotomyCorporationMods.Common;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class ParasiteTreeEvaluator : CreatureEvaluator
    {
        internal ParasiteTreeEvaluator(AgentModel agent, CreatureModel creature, RwbpType skillType) : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            var agentWillDie = false;

            if (Creature.GetAnimScript() is YggdrasilAnim animationScript)
            {
                var activeFlowers = animationScript.flowers.Where(flower => flower.activeSelf).ToList();

                agentWillDie = activeFlowers.Count == 4 && !Agent.HasBuffOfType<YggdrasilBlessBuf>();
            }

            return agentWillDie;
        }
    }
}
