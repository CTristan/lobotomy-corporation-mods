// SPDX-License-Identifier: MIT

using System.Collections.ObjectModel;
using AwesomeAssertions;
using Harmony2ForLmm.Interfaces;
using Harmony2ForLmm.Models;
using Harmony2ForLmm.Services;
using Moq;
using Xunit;

namespace Harmony2ForLmm.Test.Services
{
    /// <summary>
    /// Tests for <see cref="InstallationStateDetector"/>.
    /// </summary>
    public sealed class InstallationStateDetectorTests
    {
        private readonly Mock<IManifestService> _mockManifest;

        public InstallationStateDetectorTests()
        {
            _mockManifest = new Mock<IManifestService>();
        }

        [Fact]
        public void Detect_returns_Fresh_when_no_manifest_exists()
        {
            _mockManifest.Setup(m => m.ReadManifest(It.IsAny<string>())).Returns((InstallationManifest?)null);
            var detector = new InstallationStateDetector(_mockManifest.Object, "1.0.0");

            var result = detector.Detect("/game");

            result.State.Should().Be(InstallationState.Fresh);
            result.InstalledVersion.Should().BeNull();
            result.InstallerVersion.Should().Be("1.0.0");
        }

        [Fact]
        public void Detect_returns_Current_when_versions_match_and_all_files_present()
        {
            var manifest = CreateManifest("1.0.0");
            _mockManifest.Setup(m => m.ReadManifest(It.IsAny<string>())).Returns(manifest);
            var detector = new InstallationStateDetector(_mockManifest.Object, "1.0.0");

            var result = detector.Detect("/game");

            result.State.Should().Be(InstallationState.Current);
            result.InstalledVersion.Should().Be("1.0.0");
            result.InstallerVersion.Should().Be("1.0.0");
        }

        [Fact]
        public void Detect_returns_Outdated_when_installed_version_is_older()
        {
            var manifest = CreateManifest("1.0.0");
            _mockManifest.Setup(m => m.ReadManifest(It.IsAny<string>())).Returns(manifest);
            var detector = new InstallationStateDetector(_mockManifest.Object, "2.0.0");

            var result = detector.Detect("/game");

            result.State.Should().Be(InstallationState.Outdated);
            result.InstalledVersion.Should().Be("1.0.0");
            result.InstallerVersion.Should().Be("2.0.0");
        }

        [Fact]
        public void Detect_returns_Newer_when_installed_version_is_newer()
        {
            var manifest = CreateManifest("3.0.0");
            _mockManifest.Setup(m => m.ReadManifest(It.IsAny<string>())).Returns(manifest);
            var detector = new InstallationStateDetector(_mockManifest.Object, "2.0.0");

            var result = detector.Detect("/game");

            result.State.Should().Be(InstallationState.Newer);
            result.InstalledVersion.Should().Be("3.0.0");
            result.InstallerVersion.Should().Be("2.0.0");
        }

        [Fact]
        public void Detect_returns_Corrupted_when_files_are_missing()
        {
            var manifest = new InstallationManifest
            {
                Version = "1.0.0",
                Files = new Collection<string>(["BepInEx/core/missing.dll"]),
            };

            _mockManifest.Setup(m => m.ReadManifest(It.IsAny<string>())).Returns(manifest);
            var detector = new InstallationStateDetector(_mockManifest.Object, "1.0.0");

            var result = detector.Detect("/game");

            result.State.Should().Be(InstallationState.Corrupted);
            result.MissingFiles.Should().HaveCount(1);
            result.MissingFiles.Should().Contain("BepInEx/core/missing.dll");
        }

        [Fact]
        public void Detect_returns_Corrupted_even_when_version_matches()
        {
            var manifest = new InstallationManifest
            {
                Version = "1.0.0",
                Files = new Collection<string>(["nonexistent.dll"]),
            };

            _mockManifest.Setup(m => m.ReadManifest(It.IsAny<string>())).Returns(manifest);
            var detector = new InstallationStateDetector(_mockManifest.Object, "1.0.0");

            var result = detector.Detect("/game");

            result.State.Should().Be(InstallationState.Corrupted);
        }

        [Fact]
        public void Detect_returns_Current_when_manifest_has_empty_file_list()
        {
            var manifest = CreateManifest("1.0.0");
            _mockManifest.Setup(m => m.ReadManifest(It.IsAny<string>())).Returns(manifest);
            var detector = new InstallationStateDetector(_mockManifest.Object, "1.0.0");

            var result = detector.Detect("/game");

            result.State.Should().Be(InstallationState.Current);
            result.MissingFiles.Should().BeEmpty();
        }

        [Fact]
        public void Detect_handles_minor_version_differences()
        {
            var manifest = CreateManifest("1.0.0");
            _mockManifest.Setup(m => m.ReadManifest(It.IsAny<string>())).Returns(manifest);
            var detector = new InstallationStateDetector(_mockManifest.Object, "1.0.1");

            var result = detector.Detect("/game");

            result.State.Should().Be(InstallationState.Outdated);
        }

        private static InstallationManifest CreateManifest(string version)
        {
            return new InstallationManifest
            {
                Version = version,
                Files = [],
            };
        }
    }
}
