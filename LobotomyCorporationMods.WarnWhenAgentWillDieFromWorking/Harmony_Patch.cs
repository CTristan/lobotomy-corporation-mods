using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

        // ReSharper disable once InconsistentNaming
        public static void SetFilterPostfix([NotNull] AgentSlot __instance, AgentState state)
        {
            try
            {
                // Some initial Command Window checks to make sure we're in the right state
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
                var currentSkill = commandWindow.CurrentSkill.rwbpType;
                var qliphothCounter = creature.qliphothCounter;
                var agentWillDie = false;

                switch (creature.metadataId)
                {
                    case (long)CreatureIds.BeautyAndTheBeast:
                        {
                            const int WeakenedState = 1;
                            if (!(creature.GetAnimScript() is BeautyBeastAnim animationScript)) { break; }

                            var animationState = animationScript.GetState();
                            var isWeakened = animationState == WeakenedState;

                            agentWillDie = isWeakened && currentSkill == RwbpType.P;

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
                    case (long)CreatureIds.NothingThere:
                        {
                            agentWillDie = agent.fortitudeLevel <= 3;

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

                // Other fatal abnormalities
                if (!agentWillDie)
                {
                    // Crumbling Armor
                    if (HasCrumblingArmor(agent) && currentSkill == RwbpType.B)
                    {
                        agentWillDie = true;
                    }
                    // Fairy Festival
                    else if (HasBuff<FairyBuf>(agent) && creature.metadataId != (long)CreatureIds.FairyFestival)
                    {
                        agentWillDie = true;
                    }
                    // Laetitia
                    else if (HasBuff<LittleWitchBuf>(agent) && creature.metadataId != (long)CreatureIds.Laetitia)
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

        private static bool HasCrumblingArmor([NotNull] AgentModel agent)
        {
            return agent.HasEquipment(4000371) || agent.HasEquipment(4000372) || agent.HasEquipment(4000373) ||
                   agent.HasEquipment(4000374);
        }

        private static bool HasBuff<TBuff>([NotNull] UnitModel unit) where TBuff : UnitBuf
        {
            var buffs = unit.GetUnitBufList();

            return buffs.OfType<TBuff>().Any();
        }
    }
}
