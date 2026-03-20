// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Reflection;
using AutoFixture;
using AutoFixture.AutoMoq;
using AwesomeAssertions;
using DebugPanel.Implementations;
using DebugPanel.Interfaces;
using DebugPanel.Common.Enums.Diagnostics;
using LobotomyCorporationMods.Test.Attributes;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class BepInExPluginCollectorTests
    {
        private readonly Mock<IPluginInfoSource> _mockSource;
        private readonly Mock<IHarmonyVersionClassifier> _mockClassifier;

        public BepInExPluginCollectorTests()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _mockSource = fixture.Freeze<Mock<IPluginInfoSource>>();
            _mockClassifier = fixture.Freeze<Mock<IHarmonyVersionClassifier>>();
            _mockSource.Setup(s => s.GetPlugins()).Returns([]);
        }

        private static BepInExPluginInspectionInfo CreatePlugin(string pluginId, string name, string version, IList<AssemblyName>? references = null)
        {
            var assembly = new AssemblyInspectionInfo(name + "Assembly", "1.0.0", "/path/" + name + ".dll", references ?? []);

            return new BepInExPluginInspectionInfo(pluginId, name, version, assembly);
        }

        [Theory, LobotomyAutoData]
        public void Constructor_throws_when_pluginInfoSource_is_null(Mock<IHarmonyVersionClassifier> mockClassifier)
        {
            Action act = () => _ = new BepInExPluginCollector(null, mockClassifier.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("pluginInfoSource");
        }

        [Theory, LobotomyAutoData]
        public void Constructor_throws_when_harmonyVersionClassifier_is_null(Mock<IPluginInfoSource> mockSource)
        {
            Action act = () => _ = new BepInExPluginCollector(mockSource.Object, null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("harmonyVersionClassifier");
        }

        [Fact]
        public void Collect_returns_empty_list_when_no_plugins_exist()
        {
            var collector = new BepInExPluginCollector(_mockSource.Object, _mockClassifier.Object);

            var result = collector.Collect();

            result.Should().BeEmpty();
        }

        [Fact]
        public void Collect_maps_plugin_info_to_detected_mod_info()
        {
            var plugin = CreatePlugin("com.test.plugin", "TestPlugin", "2.0.0");
            _mockSource.Setup(s => s.GetPlugins()).Returns([plugin]);
            _mockClassifier.Setup(c => c.Classify(It.IsAny<IList<AssemblyName>>())).Returns(HarmonyVersion.Harmony2);

            var collector = new BepInExPluginCollector(_mockSource.Object, _mockClassifier.Object);
            var result = collector.Collect();

            result.Should().HaveCount(1);
            result[0].Name.Should().Be("TestPlugin");
            result[0].Version.Should().Be("2.0.0");
            result[0].Source.Should().Be(ModSource.BepInExPlugin);
            result[0].HarmonyVersion.Should().Be(HarmonyVersion.Harmony2);
            result[0].Identifier.Should().Be("com.test.plugin");
            result[0].AssemblyName.Should().Be("TestPluginAssembly");
        }

        [Fact]
        public void Collect_skips_null_plugins()
        {
            var plugin = CreatePlugin("com.test.plugin", "TestPlugin", "1.0.0");
            _mockSource.Setup(s => s.GetPlugins()).Returns([null, plugin]);
            _mockClassifier.Setup(c => c.Classify(It.IsAny<IList<AssemblyName>>())).Returns(HarmonyVersion.Harmony2);

            var collector = new BepInExPluginCollector(_mockSource.Object, _mockClassifier.Object);
            var result = collector.Collect();

            result.Should().HaveCount(1);
        }
    }
}
