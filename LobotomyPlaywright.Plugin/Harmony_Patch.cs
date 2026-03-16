// SPDX-License-Identifier: MIT

namespace LobotomyPlaywright
{
    /// <summary>
    /// LMM (Lobotomy Mod Manager) entry point.
    /// Discovered by Add_On.init() which scans BaseMods/ for classes named "Harmony_Patch".
    /// This class must not reference any BepInEx types to ensure type isolation.
    /// </summary>
    public sealed class Harmony_Patch
    {
        public Harmony_Patch()
        {
            if (PlaywrightCore.IsInitialized)
            {
                return;
            }

            PlaywrightCore.Initialize(PluginConstants.DefaultPort);
        }
    }
}
