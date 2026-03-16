// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;

#endregion

namespace LobotomyCorporationMods.Playwright
{
    /// <summary>
    /// LMM (Lobotomy Mod Manager) entry point.
    /// Discovered by Add_On.init() which scans BaseMods/ for classes named "Harmony_Patch".
    /// </summary>
    [ExcludeFromCodeCoverage]
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
