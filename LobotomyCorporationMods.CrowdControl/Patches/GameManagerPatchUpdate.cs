// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;

namespace LobotomyCorporationMods.CrowdControl.Patches
{
    [HarmonyPatch(typeof(GameManager), nameof(PrivateMethods.GameManager.Update))]
    public static class GameManagerPatchUpdate
    {
        public static void PatchBeforeUpdate([NotNull] this GameManager instance)
        {
            if (Harmony_Patch.ActionQueue.Count <= 0)
            {
                return;
            }

            var action = Harmony_Patch.ActionQueue.Dequeue();
            action.Invoke();
        }

        /// <summary>Runs after opening the Strengthen Agent window to open the appearance window.</summary>
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Global
        public static bool Prefix([NotNull] GameManager __instance)
        {
            try
            {
                __instance.PatchBeforeUpdate();

                return true;
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.LogException(ex);

                throw;
            }
        }
    }
}
