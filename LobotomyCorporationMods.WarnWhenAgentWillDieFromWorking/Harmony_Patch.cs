using System;
using System.Diagnostics.CodeAnalysis;
using CommandWindow;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking
{
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
                harmonyInstance.Patch(typeof(AgentSlot).GetMethod("SetFilter", AccessTools.all), null, harmonyMethod);
            }
            catch (Exception ex)
            {
                s_fileManager.WriteToLog(ex.Message + Environment.NewLine + ex.StackTrace);
                throw;
            }
        }

        public static void SetFilterPostfix([NotNull] AgentSlot __instance, AgentState state)
        {
            try
            {
                var commandWindow = CommandWindow.CommandWindow.CurrentWindow;

                if (state == AgentState.DEAD || state == AgentState.PANIC || state == AgentState.UNCONTROLLABLE)
                {
                    return;
                }

                if (commandWindow.CurrentWindowType != CommandType.Management)
                {
                    return;
                }

                if (!(commandWindow.CurrentTarget is CreatureModel creature)) { return; }

                if (!creature.observeInfo.IsMaxObserved())
                {
                    return;
                }

                var agent = __instance.CurrentAgent;
                var agentWillDie = false;
                var qliphothCounter = creature.qliphothCounter;

                switch (creature.metadataId)
                {
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

                            if (qliphothCounter == 0 || fortitudeStatTooHigh || agent.temperanceLevel <= 2)
                            {
                                agentWillDie = true;
                            }

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

                if (agentWillDie)
                {
                    __instance.WorkFilterFill.color = CommandWindow.CommandWindow.CurrentWindow.DeadColor;
                    __instance.WorkFilterText.text = LocalizeTextDataModel.instance.GetText("AgentState_Dead");
                    __instance.SetColor(CommandWindow.CommandWindow.CurrentWindow.DeadColor);
                }
            }
            catch
            {
            }
        }
    }
}
