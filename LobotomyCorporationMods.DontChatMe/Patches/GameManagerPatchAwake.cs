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
    [HarmonyPatch(typeof(GlobalGameManager), nameof(PrivateMethods.GlobalGameManager.Awake))]
    public static class GameManagerPatchAwake
    {
        private static void PatchAfterStart([NotNull] this GlobalGameManager instance)
        {
            var uiHost = new GameObject("DontChatMeUi");
            Object.DontDestroyOnLoad(uiHost);
            uiHost.AddComponent<WebSocketUi>();
        }

        [EntryPoint]
        // ReSharper disable once InconsistentNaming
        public static void Postfix([NotNull] GlobalGameManager __instance)
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
