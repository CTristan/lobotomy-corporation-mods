using System;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;

#pragma warning disable CA1707
namespace LobotomyCorporationMods.ForceDayEndAfterMaxMeltdownLevel
{
    public sealed class Harmony_Patch
    {
        private const string ModFileName = "LobotomyCorporationMods.ForceDayEndAfterMaxMeltdownLevel.dll";
        private static int s_currentQliphothCounter;
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
                var harmonyInstance = HarmonyInstance.Create("ForceDayEndAfterMaxMeltdownLevel");
                if (harmonyInstance == null)
                {
                    throw new InvalidOperationException(nameof(harmonyInstance));
                }

                var harmonyMethod = new HarmonyMethod(typeof(Harmony_Patch).GetMethod("AddOverloadGaguePrefix"));
                harmonyInstance.Patch(typeof(CreatureOverloadManager).GetMethod("AddOverloadGague", new Type[] { }),
                    harmonyMethod, null);
            }
            catch (Exception ex)
            {
                s_fileManager.WriteToLog(ex.Message + Environment.NewLine + ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        ///     Runs before the Meltdown Counter is incremented to see if we're already at the highest meltdown level. If
        ///     we are and we aren't in an emergency then forcefully end the day.
        /// </summary>
        // ReSharper disable once IdentifierTypo
        public static bool AddOverloadGaguePrefix([NotNull] CreatureOverloadManager __instance)
        {
            const int MaxMeltdownLevel = 10;
            var meltdownLevel = __instance.GetQliphothOverloadLevel();

            // If we're not at the max level then go through the normal method
            if (meltdownLevel < MaxMeltdownLevel) { return true; }

            // We only want to force the day to end on a full Qliphoth counter
            var maxQliphothCounter = __instance.qliphothOverloadMax;
            if (s_currentQliphothCounter < maxQliphothCounter)
            {
                s_currentQliphothCounter++;
                return true;
            }

            var gameManager = GameManager.currentGameManager;

            // In the interest of fairness, we'll allow the player to clear any emergency before ending the day
            if (!gameManager.emergency)
            {
                gameManager.ClearStage();
            }

            return false;
        }
    }
}
#pragma warning restore CA1707
