// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Facades;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(CustomizingWindow), PrivateMethods.CustomizingWindow.ReviseOpenAction)]
    public static class CustomizingWindowPatchReviseOpenAction
    {
        public static void PatchAfterReviseOpenAction([NotNull] this CustomizingWindow instance,
            [NotNull] AgentModel agent)
        {
            ThrowHelper.ThrowIfNull(instance, nameof(instance));
            ThrowHelper.ThrowIfNull(agent, nameof(agent));

            instance.LoadAgentData(agent);
        }

        public static void PostfixWithLogging(Func<CustomizingWindow> getCustomizingWindow, [NotNull] AgentModel agent)
        {
            try
            {
                ThrowHelper.ThrowIfNull(getCustomizingWindow, nameof(getCustomizingWindow));

                getCustomizingWindow().PatchAfterReviseOpenAction(agent);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }

        /// <summary>Runs after opening the Strengthen Agent window to set the appearance data for the customization window.</summary>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix([NotNull] CustomizingWindow __instance,
            [NotNull] AgentModel agent)
        {
            PostfixWithLogging(() => __instance, agent);
        }
        // ReSharper disable InconsistentNaming
    }
}
