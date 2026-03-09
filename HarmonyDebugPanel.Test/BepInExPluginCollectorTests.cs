// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using AwesomeAssertions;
using HarmonyDebugPanel.Implementations.Collectors;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;
using Xunit;

namespace HarmonyDebugPanel.Test
{
    public sealed class BepInExPluginCollectorTests
    {
        [Fact]
        public void Collect_SkipsNullPluginEntries()
        {
            StubPluginInfoSource source = new(
            [
                null,
            ]);

            BepInExPluginCollector collector = new(source, new HarmonyVersionClassifier());

            IList<ModInfo> mods = collector.Collect();

            _ = mods.Should().BeEmpty();
        }

        [Fact]
        public void Collect_MapsPluginMetadataToModInfo()
        {
            StubPluginInfoSource source = new(
            [
                new(
                    "plugin.guid",
                    "PluginName",
                    "1.2.3",
                    new AssemblyInspectionInfo("PluginAssembly", "1.2.3", "plugin.dll", [new("0Harmony")])),
            ]);

            BepInExPluginCollector collector = new(source, new HarmonyVersionClassifier());

            IList<ModInfo> mods = collector.Collect();

            _ = mods.Should().HaveCount(1);
            _ = mods[0].Name.Should().Be("PluginName");
            _ = mods[0].Version.Should().Be("1.2.3");
            _ = mods[0].Source.Should().Be(ModSource.BepInExPlugin);
            _ = mods[0].HarmonyVersion.Should().Be(HarmonyVersion.Harmony2);
            _ = mods[0].AssemblyName.Should().Be("PluginAssembly");
            _ = mods[0].Identifier.Should().Be("plugin.guid");
        }

        private sealed class StubPluginInfoSource(IEnumerable<BepInExPluginInspectionInfo> plugins) : IPluginInfoSource
        {
            private readonly IEnumerable<BepInExPluginInspectionInfo> _plugins = plugins;

            public IEnumerable<BepInExPluginInspectionInfo> GetPlugins()
            {
                return _plugins;
            }
        }
    }
}
