// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class BeautyAndTheBeastEvaluator : CreatureEvaluator
    {
        internal BeautyAndTheBeastEvaluator(AgentModel agent, CreatureModel creature, RwbpType skillType) : base(agent, creature, skillType)
        {
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            var agentWillDie = false;

            if (Creature.GetAnimScript() is BeautyBeastAnim animationScript)
            {
                const int WeakenedState = 1;
                var animationState = animationScript.GetState();
                var isWeakened = animationState == WeakenedState;

                agentWillDie = isWeakened && SkillType == RwbpType.P;
            }

            return agentWillDie;
        }
    }
}
