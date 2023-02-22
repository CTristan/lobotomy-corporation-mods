// SPDX-License-Identifier: MIT

#region

using System.Linq;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations.CreatureEvaluators
{
    internal sealed class ParasiteTreeEvaluator : CreatureEvaluator
    {
        private readonly IGameObjectAdapter _gameObjectAdapter;

        internal ParasiteTreeEvaluator(AgentModel agent, CreatureModel creature, RwbpType skillType, IGameObjectAdapter gameObjectAdapter)
            : base(agent, creature, skillType)
        {
            _gameObjectAdapter = gameObjectAdapter;
        }

        protected override bool WillAgentDieFromThisCreature()
        {
            const int MaxNumberOfFlowers = 4;

            var animation = (YggdrasilAnim)Creature.GetAnimScript();
            var numberOfFlowers = animation.flowers.Count(flower =>
            {
                _gameObjectAdapter.GameObject = flower;

                return _gameObjectAdapter.ActiveSelf;
            });
            var agentWillDie = numberOfFlowers >= MaxNumberOfFlowers && !Agent.HasBuffOfType<YggdrasilBlessBuf>();

            return agentWillDie;
        }
    }
}
