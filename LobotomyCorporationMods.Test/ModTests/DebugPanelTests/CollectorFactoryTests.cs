// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
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
        private readonly Mock<IDllFileInspector> _mockBasicDllInspector;
        private readonly Mock<IDllFileInspector> _mockCecilDllInspector;
        private readonly Mock<IShimArtifactSource> _mockShimArtifactSource;
        private readonly Mock<ILoadedAssemblyReferenceSource> _mockLoadedAssemblySource;
        private readonly Mock<IFileSystemScanner> _mockFileSystemScanner;
        private readonly Mock<IKnownIssuesDatabase> _mockKnownIssuesDatabase;

        public CollectorFactoryTests()
        {
            _mockDetector = new Mock<IEnvironmentDetector>();
            _mockHarmony1Source = new Mock<IPatchInspectionSource>();
            _mockHarmony2Source = new Mock<IPatchInspectionSource>();
            _mockAssemblySource = new Mock<IAssemblyInspectionSource>();
            _mockPluginSource = new Mock<IPluginInfoSource>();
            _mockClassifier = new Mock<IHarmonyVersionClassifier>();
            _mockBasicDllInspector = new Mock<IDllFileInspector>();
            _mockCecilDllInspector = new Mock<IDllFileInspector>();
            _mockShimArtifactSource = new Mock<IShimArtifactSource>();
            _mockLoadedAssemblySource = new Mock<ILoadedAssemblyReferenceSource>();
            _mockFileSystemScanner = new Mock<IFileSystemScanner>();
            _mockKnownIssuesDatabase = new Mock<IKnownIssuesDatabase>();

            _mockHarmony1Source.Setup(s => s.GetPatches()).Returns([]);
            _mockHarmony2Source.Setup(s => s.GetPatches()).Returns([]);
            _mockAssemblySource.Setup(s => s.GetAssemblies()).Returns([]);
            _mockPluginSource.Setup(s => s.GetPlugins()).Returns([]);
            _mockShimArtifactSource.Setup(s => s.GetBackupFileNames()).Returns([]);
            _mockLoadedAssemblySource.Setup(s => s.GetBaseModAssemblies()).Returns([]);
        }

        private CollectorFactory CreateFactory()
        {
            return new CollectorFactory(
                _mockDetector.Object,
                _mockHarmony1Source.Object,
                _mockHarmony2Source.Object,
                _mockAssemblySource.Object,
                _mockPluginSource.Object,
                _mockClassifier.Object,
                _mockBasicDllInspector.Object,
                _mockCecilDllInspector.Object,
                _mockShimArtifactSource.Object,
                _mockLoadedAssemblySource.Object,
                _mockFileSystemScanner.Object,
                _mockKnownIssuesDatabase.Object);
        }

        [Fact]
        public void Constructor_throws_when_environmentDetector_is_null()
        {
            Action act = () => _ = new CollectorFactory(null, _mockHarmony1Source.Object, _mockHarmony2Source.Object, _mockAssemblySource.Object, _mockPluginSource.Object, _mockClassifier.Object, _mockBasicDllInspector.Object, _mockCecilDllInspector.Object, _mockShimArtifactSource.Object, _mockLoadedAssemblySource.Object, _mockFileSystemScanner.Object, _mockKnownIssuesDatabase.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("environmentDetector");
        }

        [Fact]
        public void Constructor_throws_when_harmony1Source_is_null()
        {
            Action act = () => _ = new CollectorFactory(_mockDetector.Object, null, _mockHarmony2Source.Object, _mockAssemblySource.Object, _mockPluginSource.Object, _mockClassifier.Object, _mockBasicDllInspector.Object, _mockCecilDllInspector.Object, _mockShimArtifactSource.Object, _mockLoadedAssemblySource.Object, _mockFileSystemScanner.Object, _mockKnownIssuesDatabase.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("harmony1Source");
        }

        [Fact]
        public void Constructor_throws_when_harmony2Source_is_null()
        {
            Action act = () => _ = new CollectorFactory(_mockDetector.Object, _mockHarmony1Source.Object, null, _mockAssemblySource.Object, _mockPluginSource.Object, _mockClassifier.Object, _mockBasicDllInspector.Object, _mockCecilDllInspector.Object, _mockShimArtifactSource.Object, _mockLoadedAssemblySource.Object, _mockFileSystemScanner.Object, _mockKnownIssuesDatabase.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("harmony2Source");
        }

        [Fact]
        public void Constructor_throws_when_assemblySource_is_null()
        {
            Action act = () => _ = new CollectorFactory(_mockDetector.Object, _mockHarmony1Source.Object, _mockHarmony2Source.Object, null, _mockPluginSource.Object, _mockClassifier.Object, _mockBasicDllInspector.Object, _mockCecilDllInspector.Object, _mockShimArtifactSource.Object, _mockLoadedAssemblySource.Object, _mockFileSystemScanner.Object, _mockKnownIssuesDatabase.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("assemblySource");
        }

        [Fact]
        public void Constructor_throws_when_pluginInfoSource_is_null()
        {
            Action act = () => _ = new CollectorFactory(_mockDetector.Object, _mockHarmony1Source.Object, _mockHarmony2Source.Object, _mockAssemblySource.Object, null, _mockClassifier.Object, _mockBasicDllInspector.Object, _mockCecilDllInspector.Object, _mockShimArtifactSource.Object, _mockLoadedAssemblySource.Object, _mockFileSystemScanner.Object, _mockKnownIssuesDatabase.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("pluginInfoSource");
        }

        [Fact]
        public void Constructor_throws_when_classifier_is_null()
        {
            Action act = () => _ = new CollectorFactory(_mockDetector.Object, _mockHarmony1Source.Object, _mockHarmony2Source.Object, _mockAssemblySource.Object, _mockPluginSource.Object, null, _mockBasicDllInspector.Object, _mockCecilDllInspector.Object, _mockShimArtifactSource.Object, _mockLoadedAssemblySource.Object, _mockFileSystemScanner.Object, _mockKnownIssuesDatabase.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("classifier");
        }

        [Fact]
        public void CreateActivePatchCollector_uses_harmony2_source_first_when_available()
        {
            _mockDetector.Setup(d => d.IsHarmony2Available).Returns(true);
            var factory = CreateFactory();
            var debugInfo = new List<string>();

            var collector = factory.CreateActivePatchCollector(debugInfo);
            _ = collector.Collect();

            _mockHarmony2Source.Verify(s => s.GetPatches(), Times.Once);
        }

        [Fact]
        public void CreateActivePatchCollector_falls_back_to_harmony1_when_harmony2_returns_empty()
        {
            _mockDetector.Setup(d => d.IsHarmony2Available).Returns(true);
            var factory = CreateFactory();
            var debugInfo = new List<string>();

            var collector = factory.CreateActivePatchCollector(debugInfo);
            _ = collector.Collect();

            _mockHarmony2Source.Verify(s => s.GetPatches(), Times.Once);
            _mockHarmony1Source.Verify(s => s.GetPatches(), Times.Once);
        }

        [Fact]
        public void CreateActivePatchCollector_uses_harmony1_source_when_harmony2_unavailable()
        {
            _mockDetector.Setup(d => d.IsHarmony2Available).Returns(false);
            var factory = CreateFactory();
            var debugInfo = new List<string>();

            var collector = factory.CreateActivePatchCollector(debugInfo);
            _ = collector.Collect();

            _mockHarmony1Source.Verify(s => s.GetPatches(), Times.Once);
            _mockHarmony2Source.Verify(s => s.GetPatches(), Times.Never);
        }

        [Fact]
        public void CreateBaseModCollector_uses_harmony2_source_first_when_available()
        {
            _mockDetector.Setup(d => d.IsHarmony2Available).Returns(true);
            var factory = CreateFactory();
            var debugInfo = new List<string>();

            var collector = factory.CreateBaseModCollector(debugInfo);
            _ = collector.Collect();

            _mockHarmony2Source.Verify(s => s.GetPatches(), Times.Once);
        }

        [Fact]
        public void CreateBaseModCollector_uses_harmony1_source_when_harmony2_unavailable()
        {
            _mockDetector.Setup(d => d.IsHarmony2Available).Returns(false);
            var factory = CreateFactory();
            var debugInfo = new List<string>();

            var collector = factory.CreateBaseModCollector(debugInfo);
            _ = collector.Collect();

            _mockHarmony1Source.Verify(s => s.GetPatches(), Times.Once);
            _mockHarmony2Source.Verify(s => s.GetPatches(), Times.Never);
        }

        [Fact]
        public void CreateExpectedPatchSource_returns_source()
        {
            _mockDetector.Setup(d => d.IsHarmony2Available).Returns(false);
            var factory = CreateFactory();
            var debugInfo = new List<string>();

            var source = factory.CreateExpectedPatchSource(debugInfo);

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

        [Fact]
        public void Constructor_throws_when_basicDllInspector_is_null()
        {
            Action act = () => _ = new CollectorFactory(_mockDetector.Object, _mockHarmony1Source.Object, _mockHarmony2Source.Object, _mockAssemblySource.Object, _mockPluginSource.Object, _mockClassifier.Object, null, _mockCecilDllInspector.Object, _mockShimArtifactSource.Object, _mockLoadedAssemblySource.Object, _mockFileSystemScanner.Object, _mockKnownIssuesDatabase.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("basicDllInspector");
        }

        [Fact]
        public void Constructor_throws_when_cecilDllInspector_is_null()
        {
            Action act = () => _ = new CollectorFactory(_mockDetector.Object, _mockHarmony1Source.Object, _mockHarmony2Source.Object, _mockAssemblySource.Object, _mockPluginSource.Object, _mockClassifier.Object, _mockBasicDllInspector.Object, null, _mockShimArtifactSource.Object, _mockLoadedAssemblySource.Object, _mockFileSystemScanner.Object, _mockKnownIssuesDatabase.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("cecilDllInspector");
        }

        [Fact]
        public void Constructor_throws_when_shimArtifactSource_is_null()
        {
            Action act = () => _ = new CollectorFactory(_mockDetector.Object, _mockHarmony1Source.Object, _mockHarmony2Source.Object, _mockAssemblySource.Object, _mockPluginSource.Object, _mockClassifier.Object, _mockBasicDllInspector.Object, _mockCecilDllInspector.Object, null, _mockLoadedAssemblySource.Object, _mockFileSystemScanner.Object, _mockKnownIssuesDatabase.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("shimArtifactSource");
        }

        [Fact]
        public void Constructor_throws_when_loadedAssemblySource_is_null()
        {
            Action act = () => _ = new CollectorFactory(_mockDetector.Object, _mockHarmony1Source.Object, _mockHarmony2Source.Object, _mockAssemblySource.Object, _mockPluginSource.Object, _mockClassifier.Object, _mockBasicDllInspector.Object, _mockCecilDllInspector.Object, _mockShimArtifactSource.Object, null, _mockFileSystemScanner.Object, _mockKnownIssuesDatabase.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("loadedAssemblySource");
        }

        [Fact]
        public void CreateDllIntegrityCollector_uses_cecil_inspector_when_available()
        {
            _mockDetector.Setup(d => d.IsMonoCecilAvailable).Returns(true);
            _mockCecilDllInspector.Setup(i => i.GetAssemblyReferences(It.IsAny<string>())).Returns([]);
            var factory = CreateFactory();

            var collector = factory.CreateDllIntegrityCollector();
            _ = collector.Collect();

            _mockCecilDllInspector.Verify(i => i.IsDeepInspectionAvailable, Times.AtLeastOnce);
        }

        [Fact]
        public void CreateDllIntegrityCollector_uses_basic_inspector_when_cecil_unavailable()
        {
            _mockDetector.Setup(d => d.IsMonoCecilAvailable).Returns(false);
            _mockBasicDllInspector.Setup(i => i.GetAssemblyReferences(It.IsAny<string>())).Returns([]);
            var factory = CreateFactory();

            var collector = factory.CreateDllIntegrityCollector();
            _ = collector.Collect();

            _mockBasicDllInspector.Verify(i => i.IsDeepInspectionAvailable, Times.AtLeastOnce);
        }

        [Fact]
        public void CreateDllIntegrityCollector_returns_collector()
        {
            var factory = CreateFactory();

            var collector = factory.CreateDllIntegrityCollector();

            collector.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_throws_when_fileSystemScanner_is_null()
        {
            Action act = () => _ = new CollectorFactory(_mockDetector.Object, _mockHarmony1Source.Object, _mockHarmony2Source.Object, _mockAssemblySource.Object, _mockPluginSource.Object, _mockClassifier.Object, _mockBasicDllInspector.Object, _mockCecilDllInspector.Object, _mockShimArtifactSource.Object, _mockLoadedAssemblySource.Object, null, _mockKnownIssuesDatabase.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("fileSystemScanner");
        }

        [Fact]
        public void CreateFilesystemValidationCollector_returns_collector()
        {
            var factory = CreateFactory();

            var collector = factory.CreateFilesystemValidationCollector();

            collector.Should().NotBeNull();
        }

        [Fact]
        public void CreateErrorLogCollector_returns_collector()
        {
            var factory = CreateFactory();

            var collector = factory.CreateErrorLogCollector();

            collector.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_throws_when_knownIssuesDatabase_is_null()
        {
            Action act = () => _ = new CollectorFactory(_mockDetector.Object, _mockHarmony1Source.Object, _mockHarmony2Source.Object, _mockAssemblySource.Object, _mockPluginSource.Object, _mockClassifier.Object, _mockBasicDllInspector.Object, _mockCecilDllInspector.Object, _mockShimArtifactSource.Object, _mockLoadedAssemblySource.Object, _mockFileSystemScanner.Object, null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("knownIssuesDatabase");
        }

        [Fact]
        public void CreateKnownIssuesChecker_returns_collector()
        {
            var factory = CreateFactory();

            var collector = factory.CreateKnownIssuesChecker([], []);

            collector.Should().NotBeNull();
        }

        [Fact]
        public void CreateDependencyChecker_returns_collector()
        {
            var factory = CreateFactory();

            var collector = factory.CreateDependencyChecker([], []);

            collector.Should().NotBeNull();
        }

        [Fact]
        public void CreateExternalLogCollector_returns_collector()
        {
            var factory = CreateFactory();

            var collector = factory.CreateExternalLogCollector();

            collector.Should().NotBeNull();
        }

        [Fact]
        public void CreateGameplayLogErrorCollector_returns_collector()
        {
            var factory = CreateFactory();

            var collector = factory.CreateGameplayLogErrorCollector();

            collector.Should().NotBeNull();
        }
    }
}
