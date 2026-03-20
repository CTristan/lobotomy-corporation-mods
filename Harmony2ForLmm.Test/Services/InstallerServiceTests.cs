// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using AwesomeAssertions;
using Harmony2ForLmm.Interfaces;
using Harmony2ForLmm.Services;
using Moq;
using Xunit;

namespace Harmony2ForLmm.Test.Services
{
    /// <summary>
    /// Tests for <see cref="InstallerService"/>.
    /// </summary>
    public sealed class InstallerServiceTests : IDisposable
    {
        private readonly string _gameDir;
        private readonly Mock<IResourceProvider> _mockResourceProvider;
        private readonly Mock<IManifestService> _mockManifest;
        private readonly InstallerService _service;

        public InstallerServiceTests()
        {
            _gameDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_gameDir);

            _mockResourceProvider = new Mock<IResourceProvider>();
            _mockManifest = new Mock<IManifestService>();
            _service = new InstallerService(_mockResourceProvider.Object, _mockManifest.Object);
        }

        public void Dispose()
        {
            if (Directory.Exists(_gameDir))
            {
                Directory.Delete(_gameDir, recursive: true);
            }
        }

        [Fact]
        public void Install_extracts_BepInEx_distribution_files()
        {
            var result = _service.Install(_gameDir);

            result.IsSuccess.Should().BeTrue();
            _mockResourceProvider.Verify(r => r.ExtractBepInExTo(_gameDir, It.IsAny<ICollection<string>>()), Times.Once);
        }

        [Fact]
        public void Install_copies_RetargetHarmony_dll_to_patchers()
        {
            var result = _service.Install(_gameDir);

            result.IsSuccess.Should().BeTrue();
            var expectedPath = Path.Combine(_gameDir, "BepInEx", "patchers", "RetargetHarmony", "RetargetHarmony.dll");
            _mockResourceProvider.Verify(r => r.CopyDllTo("RetargetHarmony.dll", expectedPath, It.IsAny<ICollection<string>>()), Times.Once);
        }

        [Fact]
        public void Install_copies_harmony_interop_dlls_to_BepInEx_core()
        {
            var result = _service.Install(_gameDir);

            result.IsSuccess.Should().BeTrue();
            var coreDir = Path.Combine(_gameDir, "BepInEx", "core");
            _mockResourceProvider.Verify(r => r.CopyDllTo("0Harmony109.dll", Path.Combine(coreDir, "0Harmony109.dll"), It.IsAny<ICollection<string>>()), Times.Once);
            _mockResourceProvider.Verify(r => r.CopyDllTo("0Harmony12.dll", Path.Combine(coreDir, "0Harmony12.dll"), It.IsAny<ICollection<string>>()), Times.Once);
            _mockResourceProvider.Verify(r => r.CopyDllTo("0Harmony12.dll", Path.Combine(coreDir, "12Harmony.dll"), It.IsAny<ICollection<string>>()), Times.Once);
        }

        [Fact]
        public void Install_installs_documentation_files()
        {
            _mockResourceProvider.Setup(r => r.ReadDocumentText("UsersGuide.md")).Returns("# User Guide");
            _mockResourceProvider.Setup(r => r.ReadDocumentText("ModdersGuide.md")).Returns("# Modder Guide");

            var result = _service.Install(_gameDir);

            result.IsSuccess.Should().BeTrue();
            var docsDir = Path.Combine(_gameDir, IManifestService.ManifestDirectory, "docs");
            File.Exists(Path.Combine(docsDir, "UsersGuide.md")).Should().BeTrue();
            File.Exists(Path.Combine(docsDir, "ModdersGuide.md")).Should().BeTrue();
        }

        [Fact]
        public void Install_succeeds_even_without_resource_files()
        {
            var result = _service.Install(_gameDir);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Install_writes_manifest_on_success()
        {
            _service.Install(_gameDir);

            _mockManifest.Verify(m => m.WriteManifest(_gameDir, It.IsAny<IReadOnlyList<string>>()), Times.Once);
        }
    }
}
