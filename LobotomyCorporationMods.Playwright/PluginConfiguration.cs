// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using BepInEx.Configuration;

#endregion

namespace LobotomyCorporationMods.Playwright
{
    [ExcludeFromCodeCoverage]
    public sealed class PluginConfiguration
    {
        private PluginConfiguration()
        {
        }

        public bool Enabled { get; private set; } = true;

        public int Port { get; private set; } = 8484;

        public static PluginConfiguration Bind(ConfigFile config)
        {
            var settings = new PluginConfiguration();
            if (config == null)
            {
                return settings;
            }

            settings.Enabled = config.Bind(
                "General",
                "Enabled",
                true,
                new ConfigDescription("Whether the LobotomyPlaywright TCP server is enabled.")).Value;
            settings.Port = config.Bind(
                "General",
                "Port",
                8484,
                new ConfigDescription("TCP port for the LobotomyPlaywright server.")).Value;

            return settings;
        }
    }
}
