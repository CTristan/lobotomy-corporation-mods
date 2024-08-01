// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Facades;

#endregion

namespace LobotomyCorporationMods.ProjectNugway.Patches
{
    [HarmonyPatch(typeof(CustomizingWindow), PrivateMethods.CustomizingWindow.ReviseOpenAction)]
    public static class CustomizingWindowPatchReviseOpenAction
    {
        public static void PatchAfterReviseOpenAction([NotNull] this CustomizingWindow instance,
            [NotNull] AgentModel agent)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(agent, nameof(agent));

            instance.LoadAgentData(agent);
        }

        /// <summary>Runs after opening the Strengthen Agent window to set the appearance data for the customization window.</summary>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix([NotNull] CustomizingWindow __instance,
            [NotNull] AgentModel agent)
        {
            try
            {
                __instance.PatchAfterReviseOpenAction(agent);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.LogError(ex);

                throw;
            }
        }
        // ReSharper disable InconsistentNaming
    }
}
