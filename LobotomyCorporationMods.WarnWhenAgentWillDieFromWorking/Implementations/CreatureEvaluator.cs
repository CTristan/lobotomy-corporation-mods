// SPDX-License-Identifier: MIT

#region

using LobotomyCorporation.Mods.Common.Enums;
using Hemocode.WarnWhenAgentWillDieFromWorking.Interfaces;

#endregion

namespace Hemocode.WarnWhenAgentWillDieFromWorking.Implementations
{
    public abstract class CreatureEvaluator : ICreatureEvaluator
    {
        protected CreatureEvaluator(IAgentData agent,
            ICreatureData creature,
            RwbpType skillType)
        {
            Agent = agent;
            Creature = creature;
            SkillType = skillType;
        }

        protected IAgentData Agent { get; }
        protected ICreatureData Creature { get; }
        protected RwbpType SkillType { get; }

        public bool WillAgentDie()
        {
            var agentWillDie = false;

            // Make sure we have completed observation so that we can't cheat
            if (Creature.IsMaxObserved())
            {
                agentWillDie = WillAgentDieFromThisCreature() || WillAgentDieFromOtherCreatures();
            }

            return agentWillDie;
        }

        /// <summary>Some abnormalities don't kill from working on them directly but due to other conditions such as gifts or buffs.</summary>
        private bool WillAgentDieFromOtherCreatures()
        {
            return
                // Crumbling Armor's gift
                WillDieFromCrumblingArmorGift() ||
                // Fairy Festival's effect
                WillDieFromFairyFestivalEffect() ||
                // Laetitia's effect
                WillDieFromLaetitiaEffect();
        }

        private bool WillDieFromCrumblingArmorGift()
        {
            return Agent.HasCrumblingArmor() && SkillType == RwbpType.B;
        }

        private bool WillDieFromFairyFestivalEffect()
        {
            return Agent.HasFairyFestivalEffect() && Creature.metadataId != (long)CreatureIds.FairyFestival;
        }

        private bool WillDieFromLaetitiaEffect()
        {
            return Agent.HasLaetitiaEffect() && Creature.metadataId != (long)CreatureIds.Laetitia;
        }

        protected abstract bool WillAgentDieFromThisCreature();
    }
}
