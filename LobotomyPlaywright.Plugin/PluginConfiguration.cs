// SPDX-License-Identifier: MIT

using System;
using BepInEx.Configuration;

namespace LobotomyPlaywright
{
    public sealed class PluginConfiguration
    {
        private PluginConfiguration()
        {
        }

        public ConfigEntry<bool> Enabled { get; private set; }

        public ConfigEntry<int> Port { get; private set; }

        public static PluginConfiguration Bind(ConfigFile config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var settings = new PluginConfiguration
            {
                Enabled = config.Bind(
                    "General",
                    "Enabled",
                    true,
                    new ConfigDescription("Whether the LobotomyPlaywright TCP server is enabled.")),
                Port = config.Bind(
                    "General",
                    "Port",
                    8484,
                    new ConfigDescription("TCP port for the LobotomyPlaywright server.")),
            };

            return settings;
        }
    }
}
