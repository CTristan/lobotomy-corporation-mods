// SPDX-License-Identifier: MIT

#region Using directives

using System;
using HarmonyLib;

#endregion

namespace DemoMod.Mod.Patches
{
    /// <summary>
    /// Demonstrates: Finalizer patches (§Finalizer).
    /// Runs after AgentManager.OnFixedUpdate even if it threw — logs and suppresses the exception
    /// so other agents still update.
    /// </summary>
    [HarmonyPatch(typeof(AgentManager), "OnFixedUpdate")]
    public static class AgentUpdateFinalizer
    {
        private static bool s_loggedOnce;

        // Guide: §Finalizer — [HarmonyFinalizer] runs after the method even if it threw
        [HarmonyFinalizer]
        public static Exception Finalizer(Exception __exception)
        {
            // Log once on first invocation to confirm the finalizer is active
            if (!s_loggedOnce)
            {
                Plugin.Log.LogInfo("[DemoMod:Finalizer] AgentManager.OnFixedUpdate finalizer is active");
                s_loggedOnce = true;
            }

            if (__exception != null)
            {
                Plugin.Log.LogError($"[DemoMod:Finalizer] AgentManager.OnFixedUpdate threw: {__exception}");
            }

            // Guide: §Finalizer — returning null suppresses the exception
            return null;
        }
    }
}
