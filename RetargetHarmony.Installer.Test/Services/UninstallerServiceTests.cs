// SPDX-License-Identifier: MIT

using System;
using System.IO;
using AwesomeAssertions;
using Moq;
using RetargetHarmony.Installer.Interfaces;
using RetargetHarmony.Installer.Services;
using Xunit;

namespace RetargetHarmony.Installer.Test.Services
{
    /// <summary>
    /// Tests for <see cref="UninstallerService"/>.
    /// </summary>
    public sealed class UninstallerServiceTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly string _gameDir;
        private readonly Mock<IBaseModsAnalyzer> _mockAnalyzer;
        private readonly UninstallerService _service;

        public UninstallerServiceTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            _gameDir = Path.Combine(_tempDir, "game");
            Directory.CreateDirectory(_gameDir);

            _mockAnalyzer = new Mock<IBaseModsAnalyzer>();
            _mockAnalyzer.Setup(a => a.Analyze(It.IsAny<string>())).Returns([]);
            _service = new UninstallerService(_mockAnalyzer.Object);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }

        [Fact]
        public void Uninstall_removes_RetargetHarmony_patcher_directory()
        {
            var patcherDir = Path.Combine(_gameDir, "BepInEx", "patchers", "RetargetHarmony");
            Directory.CreateDirectory(patcherDir);
            File.WriteAllBytes(Path.Combine(patcherDir, "RetargetHarmony.dll"), [0]);

            var result = _service.Uninstall(_gameDir, removeBaseMods: false);

            result.IsSuccess.Should().BeTrue();
            Directory.Exists(patcherDir).Should().BeFalse();
        }

        [Fact]
        public void Uninstall_removes_harmony_interop_dlls()
        {
            var coreDir = Path.Combine(_gameDir, "BepInEx", "core");
            Directory.CreateDirectory(coreDir);
            File.WriteAllBytes(Path.Combine(coreDir, "0Harmony109.dll"), [0]);
            File.WriteAllBytes(Path.Combine(coreDir, "0Harmony12.dll"), [0]);

            var result = _service.Uninstall(_gameDir, removeBaseMods: false);

            result.IsSuccess.Should().BeTrue();
            File.Exists(Path.Combine(coreDir, "0Harmony109.dll")).Should().BeFalse();
            File.Exists(Path.Combine(coreDir, "0Harmony12.dll")).Should().BeFalse();
        }

        [Fact]
        public void Uninstall_removes_BepInEx_root_files()
        {
            File.WriteAllBytes(Path.Combine(_gameDir, "winhttp.dll"), [0]);
            File.WriteAllBytes(Path.Combine(_gameDir, "doorstop_config.ini"), [0]);
            File.WriteAllBytes(Path.Combine(_gameDir, ".doorstop_version"), [0]);

            var result = _service.Uninstall(_gameDir, removeBaseMods: false);

            result.IsSuccess.Should().BeTrue();
            result.FilesRemoved.Should().Contain(f => f.Contains("winhttp.dll"));
            result.FilesRemoved.Should().Contain(f => f.Contains("doorstop_config.ini"));
            result.FilesRemoved.Should().Contain(f => f.Contains(".doorstop_version"));
        }

        [Fact]
        public void Uninstall_removes_BepInEx_directory()
        {
            var bepInExDir = Path.Combine(_gameDir, "BepInEx");
            Directory.CreateDirectory(Path.Combine(bepInExDir, "core"));
            File.WriteAllBytes(Path.Combine(bepInExDir, "core", "SomeFile.dll"), [0]);

            var result = _service.Uninstall(_gameDir, removeBaseMods: false);

            result.IsSuccess.Should().BeTrue();
            Directory.Exists(bepInExDir).Should().BeFalse();
        }

        [Fact]
        public void Uninstall_with_removeBaseMods_removes_flagged_mods()
        {
            var baseModsDir = Path.Combine(_gameDir, "BaseMods");
            Directory.CreateDirectory(baseModsDir);
            var modPath = Path.Combine(baseModsDir, "TestMod.dll");
            File.WriteAllBytes(modPath, [0]);

            _mockAnalyzer.Setup(a => a.Analyze(_gameDir)).Returns(
            [
                new FlaggedMod(modPath, "TestMod.dll", FlagReason.Harmony2Reference, "0Harmony v2.0.0.0"),
            ]);

            var result = _service.Uninstall(_gameDir, removeBaseMods: true);

            result.IsSuccess.Should().BeTrue();
            File.Exists(modPath).Should().BeFalse();
        }

        [Fact]
        public void Uninstall_without_removeBaseMods_does_not_remove_flagged_mods()
        {
            var baseModsDir = Path.Combine(_gameDir, "BaseMods");
            Directory.CreateDirectory(baseModsDir);
            var modPath = Path.Combine(baseModsDir, "TestMod.dll");
            File.WriteAllBytes(modPath, [0]);

            _mockAnalyzer.Setup(a => a.Analyze(_gameDir)).Returns(
            [
                new FlaggedMod(modPath, "TestMod.dll", FlagReason.BepInExReference, "BepInEx.Core"),
            ]);

            var result = _service.Uninstall(_gameDir, removeBaseMods: false);

            result.IsSuccess.Should().BeTrue();
            File.Exists(modPath).Should().BeTrue();
        }

        [Fact]
        public void Uninstall_succeeds_when_nothing_to_remove()
        {
            var result = _service.Uninstall(_gameDir, removeBaseMods: false);

            result.IsSuccess.Should().BeTrue();
            result.FilesRemoved.Should().BeEmpty();
            result.DirectoriesRemoved.Should().BeEmpty();
        }

        [Fact]
        public void Uninstall_returns_list_of_removed_files_and_directories()
        {
            var patcherDir = Path.Combine(_gameDir, "BepInEx", "patchers", "RetargetHarmony");
            Directory.CreateDirectory(patcherDir);
            File.WriteAllBytes(Path.Combine(patcherDir, "RetargetHarmony.dll"), [0]);
            File.WriteAllBytes(Path.Combine(_gameDir, "winhttp.dll"), [0]);

            var result = _service.Uninstall(_gameDir, removeBaseMods: false);

            result.IsSuccess.Should().BeTrue();
            result.FilesRemoved.Should().HaveCountGreaterThan(0);
            result.DirectoriesRemoved.Should().HaveCountGreaterThan(0);
        }
    }
}
