// SPDX-License-Identifier: MIT

using BepInEx;
using UnityEngine;

namespace LobotomyPlaywright
{
    /// <summary>
    /// BepInEx entry point. Discovered by BepInEx when the DLL is in BepInEx/plugins/.
    /// Delegates all lifecycle management to PlaywrightCore/PlaywrightBehaviour.
    /// </summary>
    [BepInPlugin(PluginConstants.PluginGuid, PluginConstants.PluginName, PluginConstants.PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            try
            {
                if (Config == null)
                {
                    Debug.LogWarning("[LobotomyPlaywright] BepInEx Config is null. Using defaults.");
                }

                var configuration = PluginConfiguration.Bind(Config);

                if (!configuration.Enabled)
                {
                    Debug.Log("[LobotomyPlaywright] Disabled via BepInEx configuration.");
                    return;
                }

                PlaywrightCore.Initialize(configuration.Port);
            }
            catch (System.Exception ex)
            {
                PlaywrightCore.HandleFatalException(ex, "Awake");
                Debug.LogError("[LobotomyPlaywright] Initialization error: " + ex.Message);
            }
        }
    }
}
