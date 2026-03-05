// SPDX-License-Identifier: MIT

using System;
using System.IO;
using BepInEx.Configuration;
using FluentAssertions;
using HarmonyDebugPanel;
using UnityEngine;
using Xunit;

#pragma warning disable CA1515 // Test classes must be public for xUnit

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

            configuration.OverlayToggleHotkey.Value.Should().Be(KeyCode.F9);
            configuration.RefreshHotkey.Value.Should().Be(KeyCode.F10);
            configuration.ShowBepInExPlugins.Value.Should().BeTrue();
            configuration.ShowLmmMods.Value.Should().BeTrue();
            configuration.ShowActivePatches.Value.Should().BeTrue();
            configuration.ShowAssemblyInfo.Value.Should().BeTrue();
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
