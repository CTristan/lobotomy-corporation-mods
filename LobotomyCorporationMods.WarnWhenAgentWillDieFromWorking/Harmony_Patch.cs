using System;
using System.Diagnostics.CodeAnalysis;
using CommandWindow;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
using Object = UnityEngine.Object;

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

                // if (!creature.observeInfo.IsMaxObserved())
                // {
                //     return;
                // }
                if (state == AgentState.DEAD || state == AgentState.PANIC || state == AgentState.UNCONTROLLABLE)
                {
                    return;
                }

                if (!(bool)(Object)CommandWindow.CommandWindow.CurrentWindow)
                {
                    return;
                }

                if (CommandWindow.CommandWindow.CurrentWindow.CurrentWindowType != CommandType.Management)
                {
                    return;
                }

                var creature = commandWindow.WorkAllocate.CurrentModel;
                var agent = __instance.CurrentAgent;
                var agentWillDie = false;
                var qliphothCounter = creature.qliphothCounter;

                if (creature.metadataId == (long)CreatureIds.SingingMachine)
                {
                    if (qliphothCounter == 0 || agent.fortitudeLevel >= 4 || agent.temperanceLevel <= 2)
                    {
                        agentWillDie = true;
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
