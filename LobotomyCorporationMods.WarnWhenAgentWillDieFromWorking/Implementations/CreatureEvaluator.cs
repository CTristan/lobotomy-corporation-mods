// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Interfaces;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations
{
    internal abstract class CreatureEvaluator : ICreatureEvaluator
    {
        protected CreatureEvaluator(AgentModel agent, CreatureModel creature, RwbpType skillType)
        {
            Agent = agent;
            Creature = creature;
            SkillType = skillType;
        }

        protected AgentModel Agent { get; }
        protected CreatureModel Creature { get; }
        protected RwbpType SkillType { get; }

        public bool WillAgentDie()
        {
            var agentWillDie = false;

            // Make sure we have completed observation so we can't cheat
            if (Creature.observeInfo.IsMaxObserved())
            {
                agentWillDie = WillAgentDieFromThisCreature() || WillAgentDieFromOtherCreatures();
            }

            return agentWillDie;
        }

        protected abstract bool WillAgentDieFromThisCreature();

        /// <summary>
        ///     Some abnormalities don't kill from working on them directly but due to other conditions such as gifts or buffs.
        /// </summary>
        private bool WillAgentDieFromOtherCreatures()
        {
            bool agentWillDie;

            // Crumbling Armor
            if (Agent.HasCrumblingArmor() && SkillType == RwbpType.B)
            {
                agentWillDie = true;
            }
            // Fairy Festival
            else if (Agent.HasBuffOfType<FairyBuf>() && Creature.metadataId != (long)CreatureIds.FairyFestival)
            {
                agentWillDie = true;
            }
            // Laetitia
            else if (Agent.HasBuffOfType<LittleWitchBuf>() && Creature.metadataId != (long)CreatureIds.Laetitia)
            {
                agentWillDie = true;
            }
            else
            {
                agentWillDie = false;
            }

            return agentWillDie;
        }
    }
}
