// SPDX-License-Identifier:MIT

#region

using System;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.DontChatMe.UiComponents;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Patches
{
    [HarmonyPatch(typeof(GlobalGameManager), nameof(PrivateMethods.GlobalGameManager.Awake))]
    public static class GameManagerPatchAwake
    {
        private static GameObject s_uiHost;

        // ReSharper disable once UnusedParameter.Local
        private static void PatchAfterStart([NotNull] this GlobalGameManager instance)
        {
            if (s_uiHost)
            {
                return;
            }

            Harmony_Patch.Instance.Logger.Log("Initializing WebSocket UI...");
            s_uiHost = new GameObject("DontChatMeUi");
            Object.DontDestroyOnLoad(s_uiHost);
            s_uiHost.AddComponent<WebSocketUi>();
            Harmony_Patch.Instance.Logger.Log("WebSocket UI initialized.");
        }

        [EntryPoint]
        // ReSharper disable once InconsistentNaming
        public static void Postfix([NotNull] GlobalGameManager __instance)
        {
            try
            {
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
