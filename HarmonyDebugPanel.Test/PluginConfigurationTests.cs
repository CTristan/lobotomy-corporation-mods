// SPDX-License-Identifier: MIT

using System;
using System.IO;
using AwesomeAssertions;
using BepInEx.Configuration;
using HarmonyDebugPanel;
using UnityEngine;
using Xunit;

namespace HarmonyDebugPanel.Test;

public sealed class PluginConfigurationTests
{
    [Fact]
    public void Bind_UsesExpectedDefaultValues()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "HarmonyDebugPanel.Test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var configPath = Path.Combine(tempDirectory, "test.cfg");

        try
        {
            var configFile = new ConfigFile(configPath, true);

            var configuration = PluginConfiguration.Bind(configFile);

            configuration.OverlayToggleHotkey.Should().Be(KeyCode.F9);
            configuration.RefreshHotkey.Should().Be(KeyCode.F10);
            configuration.ShowBepInExPlugins.Should().BeTrue();
            configuration.ShowLmmMods.Should().BeTrue();
            configuration.ShowActivePatches.Should().BeTrue();
            configuration.ShowAssemblyInfo.Should().BeTrue();
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
