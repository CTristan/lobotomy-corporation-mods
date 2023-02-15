// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class ParasiteTreeEvaluator : CreatureEvaluator
    {
        private readonly IAnimationScriptAdapter _animationScriptAdapter;

        internal ParasiteTreeEvaluator(AgentModel agent, CreatureModel creature, RwbpType skillType, IAnimationScriptAdapter animationScriptAdapter) : base(agent, creature, skillType)
        {
            _animationScriptAdapter = animationScriptAdapter;
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            var agentWillDie = false;

            var animationScript = _animationScriptAdapter.UnpackScriptAsType<YggdrasilAnim>();
            if (!(animationScript is null))
            {
                var numberOfFlowers = _animationScriptAdapter.ParasiteTreeNumberOfFlowers;
                const int MaxNumberOfFlowers = 4;

                agentWillDie = numberOfFlowers >= MaxNumberOfFlowers && !Agent.HasBuffOfType<YggdrasilBlessBuf>();
            }

            return agentWillDie;
        }
    }
}
