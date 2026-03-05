// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using HarmonyDebugPanel.Implementations.Collectors;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;
using Xunit;

#pragma warning disable CA1515 // Test classes must be public for xUnit

namespace HarmonyDebugPanel.Test;

public sealed class BepInExPluginCollectorTests
{
    [Fact]
    public void Collect_SkipsNullPluginEntries()
    {
        var source = new StubPluginInfoSource(new List<BepInExPluginInspectionInfo>
        {
            null,
        });

        var collector = new BepInExPluginCollector(source, new HarmonyVersionClassifier());

        var mods = collector.Collect();

        mods.Should().BeEmpty();
    }

    [Fact]
    public void Collect_MapsPluginMetadataToModInfo()
    {
        var source = new StubPluginInfoSource(new List<BepInExPluginInspectionInfo>
        {
            new(
                "plugin.guid",
                "PluginName",
                "1.2.3",
                new AssemblyInspectionInfo("PluginAssembly", "1.2.3", "plugin.dll", new List<AssemblyName> { new("0Harmony") })),
        });

        var collector = new BepInExPluginCollector(source, new HarmonyVersionClassifier());

        var mods = collector.Collect();

        mods.Should().HaveCount(1);
        mods[0].Name.Should().Be("PluginName");
        mods[0].Version.Should().Be("1.2.3");
        mods[0].Source.Should().Be(ModSource.BepInExPlugin);
        mods[0].HarmonyVersion.Should().Be(HarmonyVersion.Harmony2);
        mods[0].AssemblyName.Should().Be("PluginAssembly");
        mods[0].Identifier.Should().Be("plugin.guid");
    }

    private sealed class StubPluginInfoSource : IPluginInfoSource
    {
        private readonly IEnumerable<BepInExPluginInspectionInfo> _plugins;

        public StubPluginInfoSource(IEnumerable<BepInExPluginInspectionInfo> plugins)
        {
            _plugins = plugins;
        }

        public IEnumerable<BepInExPluginInspectionInfo> GetPlugins()
        {
            return _plugins;
        }
    }
}
