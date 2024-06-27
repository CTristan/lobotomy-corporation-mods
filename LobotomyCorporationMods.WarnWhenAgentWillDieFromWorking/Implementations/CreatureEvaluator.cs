// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Interfaces;

#endregion

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

        /// <summary>
        ///     Some abnormalities don't kill from working on them directly but due to other conditions such as gifts or buffs.
        /// </summary>
        private bool WillAgentDieFromOtherCreatures()
        {
            // Crumbling Armor's gift
            if (WillDieFromCrumblingArmorGift())
            {
                return true;
            }

            // Fairy Festival's effect
            if (WillDieFromFairyFestivalEffect())
            {
                return true;
            }

            // Laetitia's effect
            if (WillDieFromLaetitiaEffect())
            {
                return true;
            }

            return false;
        }

        private bool WillDieFromCrumblingArmorGift()
        {
            return Agent.HasCrumblingArmor() && SkillType == RwbpType.B;
        }

        private bool WillDieFromFairyFestivalEffect()
        {
            return Agent.HasBuffOfType<FairyBuf>() && Creature.metadataId != (long)CreatureIds.FairyFestival;
        }

        private bool WillDieFromLaetitiaEffect()
        {
            return Agent.HasBuffOfType<LittleWitchBuf>() && Creature.metadataId != (long)CreatureIds.Laetitia;
        }

        protected abstract bool WillAgentDieFromThisCreature();
    }
}
