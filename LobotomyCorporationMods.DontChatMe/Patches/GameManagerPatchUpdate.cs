// SPDX-License-Identifier: MIT

using System;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LobotomyCorporationMods.DontChatMe.Patches
{
    [HarmonyPatch(typeof(GameManager), nameof(PrivateMethods.GameManager.Update))]
    public static class GameManagerPatchUpdate
    {
        private static void PatchAfterUpdate([NotNull] this GameManager instance)
        {
            var uiHost = new GameObject("DontChatMeUi");
            Object.DontDestroyOnLoad(uiHost);
            uiHost.AddComponent<>();
        }

        [EntryPoint]
        // ReSharper disable once InconsistentNaming
        public static void Postfix([NotNull] GameManager __instance)
        {
            try
            {
                __instance.PatchAfterUpdate();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.LogException(ex);

                throw;
            }
        }
    }
}
