// SPDX-License-Identifier: MIT

using System;
using System.IO;
using AwesomeAssertions;
using BepInEx.Configuration;
using UnityEngine;
using Xunit;

namespace HarmonyDebugPanel.Test.Tests
{
    public sealed class PluginConfigurationTests
    {
        [Fact]
        public void Bind_UsesExpectedDefaultValues()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), "HarmonyDebugPanel.Test", Guid.NewGuid().ToString("N"));
            _ = Directory.CreateDirectory(tempDirectory);
            string configPath = Path.Combine(tempDirectory, "test.cfg");

            try
            {
                ConfigFile configFile = new(configPath, true);

                PluginConfiguration configuration = PluginConfiguration.Bind(configFile);

                _ = configuration.OverlayToggleHotkey.Should().Be(KeyCode.F9);
                _ = configuration.RefreshHotkey.Should().Be(KeyCode.F10);
                _ = configuration.ShowBepInExPlugins.Should().BeTrue();
                _ = configuration.ShowLmmMods.Should().BeTrue();
                _ = configuration.ShowActivePatches.Should().BeTrue();
                _ = configuration.ShowAssemblyInfo.Should().BeTrue();
            }
            finally
            {
                if (Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, recursive: true);
                }
            }
        }
    }
}
