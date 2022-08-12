// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;

namespace LobotomyCorporationMods.FreeCustomization
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    public sealed class Harmony_Patch
    {
        private const string ModFileName = "LobotomyCorporationMods.FreeCustomization.dll";

        private static IFileManager FileManager;

        /// <summary>
        ///     Do not use for testing as it causes an exception. Use the other constructor instead.
        /// </summary>
        public Harmony_Patch()
        {
            FileManager = new FileManager(ModFileName);
            InitializeHarmonyPatch();
        }

        /// <summary>
        ///     Entry point for testing.
        /// </summary>
        public Harmony_Patch(IFileManager fileManager)
        {
            FileManager = fileManager;
        }

        /// <summary>
        ///     Patches all of the relevant method calls through Harmony.
        /// </summary>
        private static void InitializeHarmonyPatch()
        {
            try
            {
                var harmonyInstance = HarmonyInstance.Create("FreeCustomization");
                if (harmonyInstance == null)
                {
                    throw new InvalidOperationException(nameof(harmonyInstance));
                }

                var harmonyMethod = new HarmonyMethod(typeof(Harmony_Patch).GetMethod("CloseWindowPrefix"));
                harmonyInstance.Patch(typeof(AppearanceUI).GetMethod("CloseWindow", AccessTools.all), harmonyMethod, null);

                harmonyMethod = new HarmonyMethod(typeof(Harmony_Patch).GetMethod("GenerateWindowPostfix"));
                harmonyInstance.Patch(typeof(AgentInfoWindow).GetMethod("GenerateWindow", AccessTools.all), null, harmonyMethod);

                harmonyMethod = new HarmonyMethod(typeof(Harmony_Patch).GetMethod("OpenAppearanceWindowPostfix"));
                harmonyInstance.Patch(typeof(CustomizingWindow).GetMethod("OpenAppearanceWindow"), null, harmonyMethod);
            }
            catch (Exception ex)
            {
                var message = ex.Message + Environment.NewLine + ex.StackTrace;
                FileManager.WriteToLog(message);

                throw;
            }
        }

        /// <summary>
        ///     Runs before the Close Window function of the AppearanceUI runs to verify if we actually want to close the window.
        ///     The only reason we do this is because there's a hardcoded call to a private method (CustomizingWindow.Start()) that
        ///     closes the appearance window after the first agent window is generated.
        /// </summary>
        public static bool CloseWindowPrefix([NotNull] AppearanceUI __instance)
        {
            return __instance.closeAction != null;
        }

        /// <summary>
        ///     Runs after opening the Agent window to automatically open the appearance window, since there's no reason to hide it
        ///     behind a button.
        /// </summary>
        public static void GenerateWindowPostfix()
        {
            try
            {
                AgentInfoWindow.currentWindow.customizingWindow.OpenAppearanceWindow();
            }
            catch (Exception ex)
            {
                var message = ex.Message + Environment.NewLine + ex.StackTrace;
                FileManager.WriteToLog(message);

                throw;
            }
        }

        /// <summary>
        ///     Runs after opening the Appearance Window to make sure the IsCustomAppearance field is false, which is used by all
        ///     of the private methods to check for increasing the cost of custom agents.
        /// </summary>
        public static void OpenAppearanceWindowPostfix([NotNull] CustomizingWindow __instance)
        {
            __instance.CurrentData.isCustomAppearance = false;
        }
    }
}
