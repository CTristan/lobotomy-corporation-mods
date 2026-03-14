// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.DebugPanel.Implementations;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

// ReSharper disable InconsistentNaming
namespace LobotomyCorporationMods.DebugPanel.Patches
{
    [HarmonyPatch(typeof(GlobalGameManager), PrivateMethods.GlobalGameManager.Awake)]
    public static class GlobalGameManagerPatchAwake
    {
        public static bool PatchAfterAwake([NotNull] this GlobalGameManager instance, bool debugPanelAlreadyExists)
        {
            ThrowHelper.ThrowIfNull(instance, nameof(instance));

            return !debugPanelAlreadyExists;
        }

        public static void PostfixWithLogging(
            Func<GlobalGameManager> getInstance,
            Func<bool> getDebugPanelExists)
        {
            try
            {
                ThrowHelper.ThrowIfNull(getInstance, nameof(getInstance));
                ThrowHelper.ThrowIfNull(getDebugPanelExists, nameof(getDebugPanelExists));

                _ = getInstance().PatchAfterAwake(getDebugPanelExists());
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }

        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix(GlobalGameManager __instance)
        {
            try
            {
                var existing = Object.FindObjectOfType<DebugPanelBehaviour>();
                if (existing != null)
                {
                    return;
                }

                var go = new GameObject("DebugPanelBehaviour");
                _ = go.AddComponent<DebugPanelBehaviour>();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);
            }
        }
    }
}
