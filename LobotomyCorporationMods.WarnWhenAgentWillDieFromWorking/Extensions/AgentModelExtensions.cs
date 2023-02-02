// SPDX-License-Identifier: MIT

using System.Linq;
using LobotomyCorporationMods.Common;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions
{
    internal static class AgentModelExtensions
    {
        internal static bool CheckIfWorkWillKillAgent(this AgentModel agent, CommandWindow.CommandWindow commandWindow)
        {
            // Make sure we actually have an abnormality in our work window
            var creature = commandWindow.GetCreatureIfValid();
            if (creature == null) { return false; }

            var skillType = commandWindow.CurrentSkill.rwbpType;

            var agentWillDie = false;
            var qliphothCounter = creature.qliphothCounter;

            switch (creature.metadataId)
            {
                case (long)CreatureIds.BeautyAndTheBeast:
                    {
                        if (creature.GetAnimScript() is BeautyBeastAnim animationScript)
                        {
                            const int WeakenedState = 1;
                            var animationState = animationScript.GetState();
                            var isWeakened = animationState == WeakenedState;

                            agentWillDie = isWeakened && skillType == RwbpType.P;
                        }

                        break;
                    }
                case (long)CreatureIds.Bloodbath:
                    {
                        agentWillDie = agent.fortitudeLevel == 1 || agent.temperanceLevel == 1;

                        break;
                    }
                case (long)CreatureIds.CrumblingArmor:
                    {
                        agentWillDie = agent.fortitudeLevel == 1;

                        break;
                    }
                case (long)CreatureIds.HappyTeddyBear:
                    {
                        if (creature.script is HappyTeddy script)
                        {
                            agentWillDie = agent.instanceId == script.lastAgent.instanceId;
                        }

                        break;
                    }
                case (long)CreatureIds.NothingThere:
                    {
                        agentWillDie = agent.fortitudeLevel <= 3;

                        break;
                    }
                case (long)CreatureIds.ParasiteTree:
                    {
                        if (!(creature.GetAnimScript() is YggdrasilAnim animationScript)) { break; }

                        var activeFlowers = animationScript.flowers.Where(flower => flower.activeSelf).ToList();

                        agentWillDie = activeFlowers.Count == 4 && !agent.HasBuffOfType<YggdrasilBlessBuf>();

                        break;
                    }
                case (long)CreatureIds.RedShoes:
                    {
                        agentWillDie = agent.temperanceLevel <= 2;

                        break;
                    }
                case (long)CreatureIds.SingingMachine:
                    {
                        /*
                             Singing Machine's gift increases the Fortitude stat, and since the kill agent check is at
                             the end of the work session it's possible for the agent to get the gift which increases the
                             Fortitude level from 3 to 4. To account for that we look at the actual stat value instead
                             of the level.
                             */

                        const int GiftIncrease = 8;
                        const int FortitudeThreshold = 65;
                        var fortitudeStatTooHigh = agent.fortitudeStat >= FortitudeThreshold - GiftIncrease;

                        agentWillDie = qliphothCounter == 0 || fortitudeStatTooHigh || agent.temperanceLevel <= 2;

                        break;
                    }
                case (long)CreatureIds.SpiderBud:
                    {
                        agentWillDie = agent.prudenceLevel == 1;

                        break;
                    }
                case (long)CreatureIds.VoidMachine:
                    {
                        agentWillDie = agent.temperanceLevel < 2;

                        break;
                    }
                case (long)CreatureIds.WarmHeartedWoodsman:
                    {
                        agentWillDie = qliphothCounter == 0;

                        break;
                    }
            }

            // Other fatal abnormalities
            if (!agentWillDie)
            {
                // Crumbling Armor
                if (agent.HasCrumblingArmor() && skillType == RwbpType.B)
                {
                    agentWillDie = true;
                }
                // Fairy Festival
                else if (agent.HasBuffOfType<FairyBuf>() && creature.metadataId != (long)CreatureIds.FairyFestival)
                {
                    agentWillDie = true;
                }
                // Laetitia
                else if (agent.HasBuffOfType<LittleWitchBuf>() && creature.metadataId != (long)CreatureIds.Laetitia)
                {
                    agentWillDie = true;
                }
            }

            return agentWillDie;
        }
    }
}
