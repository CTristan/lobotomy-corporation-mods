// SPDX-License-Identifier: MIT

using System;
using System.IO;
using AwesomeAssertions;
using Harmony2ForLmm.Interfaces;
using Harmony2ForLmm.Services;
using Xunit;

namespace Harmony2ForLmm.Test.Services
{
    /// <summary>
    /// Tests for <see cref="ManifestService"/>.
    /// </summary>
    public sealed class ManifestServiceTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly string _gameDir;
        private readonly ManifestService _service;

        public ManifestServiceTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            _gameDir = Path.Combine(_tempDir, "game");
            Directory.CreateDirectory(_gameDir);

            _service = new ManifestService("1.2.3");
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }

        [Fact]
        public void ReadManifest_returns_null_when_no_manifest_exists()
        {
            var result = _service.ReadManifest(_gameDir);

            result.Should().BeNull();
        }

        [Fact]
        public void WriteManifest_creates_manifest_directory_and_file()
        {
            string[] files = [Path.Combine(_gameDir, "BepInEx", "core", "0Harmony109.dll")];

            _service.WriteManifest(_gameDir, files);

            var manifestPath = Path.Combine(_gameDir, IManifestService.ManifestDirectory, IManifestService.ManifestFileName);
            File.Exists(manifestPath).Should().BeTrue();
        }

        [Fact]
        public void WriteManifest_stores_correct_version()
        {
            _service.WriteManifest(_gameDir, []);

            var manifest = _service.ReadManifest(_gameDir);

            manifest.Should().NotBeNull();
            manifest!.Version.Should().Be("1.2.3");
        }

        [Fact]
        public void WriteManifest_stores_relative_paths_with_forward_slashes()
        {
            string[] files =
            [
                Path.Combine(_gameDir, "BepInEx", "core", "0Harmony109.dll"),
                Path.Combine(_gameDir, "winhttp.dll"),
            ];

            _service.WriteManifest(_gameDir, files);

            var manifest = _service.ReadManifest(_gameDir);

            manifest.Should().NotBeNull();
            manifest!.Files.Should().Contain("BepInEx/core/0Harmony109.dll");
            manifest.Files.Should().Contain("winhttp.dll");
        }

        [Fact]
        public void WriteManifest_stores_installation_timestamp()
        {
            var before = DateTime.UtcNow;

            _service.WriteManifest(_gameDir, []);

            var manifest = _service.ReadManifest(_gameDir);

            manifest.Should().NotBeNull();
            manifest!.InstalledAt.Should().BeOnOrAfter(before);
            manifest.InstalledAt.Should().BeOnOrBefore(DateTime.UtcNow);
        }

        [Fact]
        public void ReadManifest_round_trips_correctly()
        {
            string[] files =
            [
                Path.Combine(_gameDir, "BepInEx", "patchers", "RetargetHarmony", "RetargetHarmony.dll"),
                Path.Combine(_gameDir, "BepInEx", "core", "0Harmony109.dll"),
                Path.Combine(_gameDir, "winhttp.dll"),
            ];

            _service.WriteManifest(_gameDir, files);

            var manifest = _service.ReadManifest(_gameDir);

            manifest.Should().NotBeNull();
            manifest!.Version.Should().Be("1.2.3");
            manifest.Files.Should().HaveCount(3);
        }

        [Fact]
        public void ReadManifest_returns_null_for_corrupt_json()
        {
            var manifestDir = Path.Combine(_gameDir, IManifestService.ManifestDirectory);
            Directory.CreateDirectory(manifestDir);
            File.WriteAllText(Path.Combine(manifestDir, IManifestService.ManifestFileName), "not valid json {{{");

            var result = _service.ReadManifest(_gameDir);

            result.Should().BeNull();
        }

        [Fact]
        public void DeleteManifest_removes_manifest_directory()
        {
            _service.WriteManifest(_gameDir, []);

            var manifestDir = Path.Combine(_gameDir, IManifestService.ManifestDirectory);
            Directory.Exists(manifestDir).Should().BeTrue();

            _service.DeleteManifest(_gameDir);

            Directory.Exists(manifestDir).Should().BeFalse();
        }

        [Fact]
        public void DeleteManifest_succeeds_when_no_manifest_exists()
        {
            _service.DeleteManifest(_gameDir);

            var manifestDir = Path.Combine(_gameDir, IManifestService.ManifestDirectory);
            Directory.Exists(manifestDir).Should().BeFalse();
        }

        [Fact]
        public void WriteManifest_overwrites_existing_manifest()
        {
            _service.WriteManifest(_gameDir, [Path.Combine(_gameDir, "old.dll")]);

            var newService = new ManifestService("2.0.0");
            newService.WriteManifest(_gameDir, [Path.Combine(_gameDir, "new.dll")]);

            var manifest = newService.ReadManifest(_gameDir);

            manifest.Should().NotBeNull();
            manifest!.Version.Should().Be("2.0.0");
            manifest.Files.Should().HaveCount(1);
            manifest.Files.Should().Contain("new.dll");
        }
    }
}
