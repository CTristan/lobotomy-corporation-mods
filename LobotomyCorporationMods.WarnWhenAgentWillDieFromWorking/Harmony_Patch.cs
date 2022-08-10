using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommandWindow;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking
{
    // ReSharper disable once InconsistentNaming
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    public class Harmony_Patch
    {
        private const string ModFileName = "LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.dll";
        private static IFileManager s_fileManager;

        /// <summary>
        ///     Do not use for testing as it causes an exception. Use the other constructor instead.
        /// </summary>
        public Harmony_Patch()
        {
            s_fileManager = new FileManager(ModFileName);
            InitializeHarmonyPatch();
        }

        /// <summary>
        ///     Entry point for testing.
        /// </summary>
        public Harmony_Patch(IFileManager fileManager)
        {
            s_fileManager = fileManager;
        }

        /// <summary>
        ///     Patches all of the relevant method calls through Harmony.
        /// </summary>
        private static void InitializeHarmonyPatch()
        {
            try
            {
                var harmonyInstance = HarmonyInstance.Create("ColorWorkOrderBySuccessChance");
                if (harmonyInstance == null)
                {
                    throw new InvalidOperationException(nameof(harmonyInstance));
                }

                var harmonyMethod = new HarmonyMethod(typeof(Harmony_Patch).GetMethod("SetFilterPostfix"));
                harmonyInstance.Patch(typeof(AgentSlot).GetMethod("SetFilter", new[] { typeof(AgentState) }), null,
                    harmonyMethod);

                // Fix a bug with Crumbling Armor
                harmonyMethod = new HarmonyMethod(typeof(Harmony_Patch).GetMethod("OnNoticePrefix"));
                harmonyInstance.Patch(
                    typeof(ArmorCreature).GetMethod("OnNotice", new[] { typeof(string), typeof(object[]) }),
                    harmonyMethod, null);
            }
            catch (Exception ex)
            {
                s_fileManager.WriteToLog(ex);
                throw;
            }
        }

        // ReSharper disable once InconsistentNaming
        public static void SetFilterPostfix([NotNull] AgentSlot __instance, AgentState state)
        {
            try
            {
                if (__instance == null || IsInvalidState(state)) { return; }

                // Some initial Command Window checks to make sure we're in the right state
                var commandWindow = GetCommandWindowIfValid();
                if (commandWindow == null) { return; }

                // Make sure we actually have an abnormality in our work window
                var creature = GetCreatureIfValid(commandWindow);
                if (creature == null) { return; }

                var agent = __instance.CurrentAgent;
                if (agent == null) { return; }

                var currentSkill = commandWindow.CurrentSkill.rwbpType;
                var agentWillDie = CheckIfWorkWillKillAgent(commandWindow, creature, currentSkill, agent);

                if (!agentWillDie)
                {
                    return;
                }

                __instance.WorkFilterFill.color = commandWindow.DeadColor;
                __instance.WorkFilterText.text = LocalizeTextDataModel.instance.GetText("AgentState_Dead");
                __instance.SetColor(commandWindow.DeadColor);
            }
            catch (Exception ex)
            {
                s_fileManager.WriteToLog(ex);
                throw;
            }
        }

        /// <summary>
        ///     Fixes a bug with Crumbling Armor where an agent that started the day with the gift but later replaced the gift with
        ///     another one would still die when performing an Attachment work.
        ///     The root cause of the bug is that the trigger for killing the agent is separate from whether the agent actually has
        ///     the gift or not. The armor keeps its own private list of agents that have either started the day with the gift or
        ///     have acquired the gift during the day. Unfortunately it doesn't correctly remove the agents from the list when they
        ///     replace the gift, so the only way to avoid the bug normally is to wait until the next day to perform Attachment
        ///     work.
        ///     This fix will force the trigger to check if the agent actually has the gift, and if they do then we stop the armor
        ///     from checking its private list for the agent.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="notice"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public static bool OnNoticePrefix(ArmorCreature __instance, string notice, params object[] param)
        {
            try
            {
                if (notice != NoticeName.OnWorkStart)
                {
                    return true;
                }

                if (!(param[0] is CreatureModel creatureModel)) { return true; }

                // We only care if we're doing Attachment work
                var skillId = creatureModel.currentSkill.skillTypeInfo.id;
                if (skillId != SkillTypeInfo.Consensus)
                {
                    return true;
                }

                var agent = creatureModel.currentSkill.agent;

                // If the agent doesn't actually have Crumbling Armor's gift then we won't continue.
                return agent.HasCrumblingArmor();
            }
            catch (Exception ex)
            {
                s_fileManager.WriteToLog(ex);
                throw;
            }
        }

        [CanBeNull]
        private static CreatureModel GetCreatureIfValid([NotNull] CommandWindow.CommandWindow commandWindow)
        {
            if (!(commandWindow.CurrentTarget is CreatureModel creature))
            {
                return null;
            }

            // Make sure we have completed observation so we can't cheat
            return !creature.observeInfo.IsMaxObserved() ? null : creature;
        }

        private static bool CheckIfWorkWillKillAgent([NotNull] CommandWindow.CommandWindow commandWindow,
            [NotNull] CreatureModel creature, RwbpType skillType, AgentModel agent)
        {
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

        private static bool IsInvalidState(AgentState state)
        {
            return state == AgentState.DEAD || state == AgentState.PANIC || state == AgentState.UNCONTROLLABLE;
        }

        [CanBeNull]
        private static CommandWindow.CommandWindow GetCommandWindowIfValid()
        {
            var commandWindow = CommandWindow.CommandWindow.CurrentWindow;
            if (commandWindow == null) { return null; }

            // Validation checks to confirm we have everything we need
            if (commandWindow.CurrentSkill?.rwbpType == null) { return null; }

            {
                return commandWindow.CurrentWindowType != CommandType.Management ? null : commandWindow;
            }
        }
    }
}
