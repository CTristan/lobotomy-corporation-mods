// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.DebugPanel.Implementations;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class CollectorFactoryTests
    {
        private readonly Mock<IEnvironmentDetector> _mockDetector;
        private readonly Mock<IPatchInspectionSource> _mockHarmony1Source;
        private readonly Mock<IPatchInspectionSource> _mockHarmony2Source;
        private readonly Mock<IAssemblyInspectionSource> _mockAssemblySource;
        private readonly Mock<IPluginInfoSource> _mockPluginSource;
        private readonly Mock<IHarmonyVersionClassifier> _mockClassifier;

        public CollectorFactoryTests()
        {
            _mockDetector = new Mock<IEnvironmentDetector>();
            _mockHarmony1Source = new Mock<IPatchInspectionSource>();
            _mockHarmony2Source = new Mock<IPatchInspectionSource>();
            _mockAssemblySource = new Mock<IAssemblyInspectionSource>();
            _mockPluginSource = new Mock<IPluginInfoSource>();
            _mockClassifier = new Mock<IHarmonyVersionClassifier>();

            _mockHarmony1Source.Setup(s => s.GetPatches()).Returns([]);
            _mockHarmony2Source.Setup(s => s.GetPatches()).Returns([]);
            _mockAssemblySource.Setup(s => s.GetAssemblies()).Returns([]);
            _mockPluginSource.Setup(s => s.GetPlugins()).Returns([]);
        }

        private CollectorFactory CreateFactory()
        {
            return new CollectorFactory(
                _mockDetector.Object,
                _mockHarmony1Source.Object,
                _mockHarmony2Source.Object,
                _mockAssemblySource.Object,
                _mockPluginSource.Object,
                _mockClassifier.Object);
        }

        [Fact]
        public void Constructor_throws_when_environmentDetector_is_null()
        {
            Action act = () => _ = new CollectorFactory(null, _mockHarmony1Source.Object, _mockHarmony2Source.Object, _mockAssemblySource.Object, _mockPluginSource.Object, _mockClassifier.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("environmentDetector");
        }

        [Fact]
        public void Constructor_throws_when_harmony1Source_is_null()
        {
            Action act = () => _ = new CollectorFactory(_mockDetector.Object, null, _mockHarmony2Source.Object, _mockAssemblySource.Object, _mockPluginSource.Object, _mockClassifier.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("harmony1Source");
        }

        [Fact]
        public void Constructor_throws_when_harmony2Source_is_null()
        {
            Action act = () => _ = new CollectorFactory(_mockDetector.Object, _mockHarmony1Source.Object, null, _mockAssemblySource.Object, _mockPluginSource.Object, _mockClassifier.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("harmony2Source");
        }

        [Fact]
        public void Constructor_throws_when_assemblySource_is_null()
        {
            Action act = () => _ = new CollectorFactory(_mockDetector.Object, _mockHarmony1Source.Object, _mockHarmony2Source.Object, null, _mockPluginSource.Object, _mockClassifier.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("assemblySource");
        }

        [Fact]
        public void Constructor_throws_when_pluginInfoSource_is_null()
        {
            Action act = () => _ = new CollectorFactory(_mockDetector.Object, _mockHarmony1Source.Object, _mockHarmony2Source.Object, _mockAssemblySource.Object, null, _mockClassifier.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("pluginInfoSource");
        }

        [Fact]
        public void Constructor_throws_when_classifier_is_null()
        {
            Action act = () => _ = new CollectorFactory(_mockDetector.Object, _mockHarmony1Source.Object, _mockHarmony2Source.Object, _mockAssemblySource.Object, _mockPluginSource.Object, null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("classifier");
        }

        [Fact]
        public void CreateActivePatchCollector_uses_harmony2_source_when_available()
        {
            _mockDetector.Setup(d => d.IsHarmony2Available).Returns(true);
            var factory = CreateFactory();

            var collector = factory.CreateActivePatchCollector();
            _ = collector.Collect();

            _mockHarmony2Source.Verify(s => s.GetPatches(), Times.Once);
            _mockHarmony1Source.Verify(s => s.GetPatches(), Times.Never);
        }

        [Fact]
        public void CreateActivePatchCollector_uses_harmony1_source_when_harmony2_unavailable()
        {
            _mockDetector.Setup(d => d.IsHarmony2Available).Returns(false);
            var factory = CreateFactory();

            var collector = factory.CreateActivePatchCollector();
            _ = collector.Collect();

            _mockHarmony1Source.Verify(s => s.GetPatches(), Times.Once);
            _mockHarmony2Source.Verify(s => s.GetPatches(), Times.Never);
        }

        [Fact]
        public void CreateBaseModCollector_uses_harmony2_source_when_available()
        {
            _mockDetector.Setup(d => d.IsHarmony2Available).Returns(true);
            var factory = CreateFactory();

            var collector = factory.CreateBaseModCollector();
            _ = collector.Collect();

            _mockHarmony2Source.Verify(s => s.GetPatches(), Times.Once);
            _mockHarmony1Source.Verify(s => s.GetPatches(), Times.Never);
        }

        [Fact]
        public void CreateBaseModCollector_uses_harmony1_source_when_harmony2_unavailable()
        {
            _mockDetector.Setup(d => d.IsHarmony2Available).Returns(false);
            var factory = CreateFactory();

            var collector = factory.CreateBaseModCollector();
            _ = collector.Collect();

            _mockHarmony1Source.Verify(s => s.GetPatches(), Times.Once);
            _mockHarmony2Source.Verify(s => s.GetPatches(), Times.Never);
        }

        [Fact]
        public void CreateExpectedPatchSource_returns_source()
        {
            _mockDetector.Setup(d => d.IsHarmony2Available).Returns(false);
            var factory = CreateFactory();

            var source = factory.CreateExpectedPatchSource();

            source.Should().NotBeNull();
        }

        [Fact]
        public void CreateBepInExPluginCollector_returns_collector()
        {
            var factory = CreateFactory();

            var collector = factory.CreateBepInExPluginCollector();

            collector.Should().NotBeNull();
        }

        [Fact]
        public void CreateAssemblyInfoCollector_returns_collector()
        {
            var factory = CreateFactory();

            var collector = factory.CreateAssemblyInfoCollector();

            collector.Should().NotBeNull();
        }

        [Fact]
        public void CreateRetargetHarmonyDetector_returns_collector()
        {
            var factory = CreateFactory();

            var collector = factory.CreateRetargetHarmonyDetector();

            collector.Should().NotBeNull();
        }
    }
}
