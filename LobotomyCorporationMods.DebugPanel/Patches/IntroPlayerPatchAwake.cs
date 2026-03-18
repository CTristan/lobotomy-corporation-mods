// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.Common.Implementations;
using Hemocode.DebugPanel.Implementations;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

// ReSharper disable InconsistentNaming
namespace Hemocode.DebugPanel.Patches
{
    [HarmonyPatch(typeof(IntroPlayer), PrivateMethods.IntroPlayer.Awake)]
    public static class IntroPlayerPatchAwake
    {
        public static bool PatchAfterAwake([NotNull] this IntroPlayer instance, bool debugPanelAlreadyExists)
        {
            ThrowHelper.ThrowIfNull(instance, nameof(instance));

            return !debugPanelAlreadyExists;
        }

        public static void PostfixWithLogging(
            Func<IntroPlayer> getInstance,
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
        public static void Postfix(IntroPlayer __instance)
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
