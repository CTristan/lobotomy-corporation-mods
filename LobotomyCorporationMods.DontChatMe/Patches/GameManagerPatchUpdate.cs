// SPDX-License-Identifier: MIT

using System;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.DontChatMe.UiComponents;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LobotomyCorporationMods.DontChatMe.Patches
{
    [HarmonyPatch(typeof(GameManager), nameof(PrivateMethods.GameManager.Start))]
    public static class GameManagerPatchUpdate
    {
        private static void PatchAfterStart([NotNull] this GameManager instance)
        {
            Harmony_Patch.Instance.Logger.Log("Patching GameManager.Start");
            var uiHost = new GameObject("DontChatMeUi");
            Object.DontDestroyOnLoad(uiHost);
            uiHost.AddComponent<WebSocketUi>();
            Harmony_Patch.Instance.Logger.Log("Patched GameManager.Start");
        }

        [EntryPoint]
        // ReSharper disable once InconsistentNaming
        public static void Postfix([NotNull] GameManager __instance)
        {
            try
            {
                Harmony_Patch.Instance.Logger.Log("Running GameManager.Start");
                __instance.PatchAfterStart();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.LogException(ex);

                throw;
            }
        }
    }
}
