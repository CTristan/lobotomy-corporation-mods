// SPDX-License-Identifier: MIT

using System;
using System.IO;
using AwesomeAssertions;
using RetargetHarmony.Installer.Services;
using Xunit;

namespace RetargetHarmony.Installer.Test.Services
{
    /// <summary>
    /// Tests for <see cref="InstallerService"/>.
    /// </summary>
    public sealed class InstallerServiceTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly string _resourcesDir;
        private readonly string _gameDir;
        private readonly InstallerService _service;

        public InstallerServiceTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);

            _resourcesDir = Path.Combine(_tempDir, "resources");
            Directory.CreateDirectory(_resourcesDir);

            _gameDir = Path.Combine(_tempDir, "game");
            Directory.CreateDirectory(_gameDir);

            _service = new InstallerService(_resourcesDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }

        [Fact]
        public void IsBepInExInstalled_returns_false_when_not_installed()
        {
            _service.IsBepInExInstalled(_gameDir).Should().BeFalse();
        }

        [Fact]
        public void IsBepInExInstalled_returns_true_when_installed()
        {
            Directory.CreateDirectory(Path.Combine(_gameDir, "BepInEx"));
            File.WriteAllBytes(Path.Combine(_gameDir, "winhttp.dll"), [0]);

            _service.IsBepInExInstalled(_gameDir).Should().BeTrue();
        }

        [Fact]
        public void IsRetargetHarmonyInstalled_returns_false_when_not_installed()
        {
            _service.IsRetargetHarmonyInstalled(_gameDir).Should().BeFalse();
        }

        [Fact]
        public void IsRetargetHarmonyInstalled_returns_true_when_installed()
        {
            var patcherDir = Path.Combine(_gameDir, "BepInEx", "patchers", "RetargetHarmony");
            Directory.CreateDirectory(patcherDir);
            File.WriteAllBytes(Path.Combine(patcherDir, "RetargetHarmony.dll"), [0]);

            _service.IsRetargetHarmonyInstalled(_gameDir).Should().BeTrue();
        }

        [Fact]
        public void Install_copies_harmony_interop_dlls_to_BepInEx_core()
        {
            File.WriteAllBytes(Path.Combine(_resourcesDir, "0Harmony109.dll"), [1, 2, 3]);
            File.WriteAllBytes(Path.Combine(_resourcesDir, "0Harmony12.dll"), [4, 5, 6]);

            var result = _service.Install(_gameDir);

            result.IsSuccess.Should().BeTrue();
            File.Exists(Path.Combine(_gameDir, "BepInEx", "core", "0Harmony109.dll")).Should().BeTrue();
            File.Exists(Path.Combine(_gameDir, "BepInEx", "core", "0Harmony12.dll")).Should().BeTrue();
        }

        [Fact]
        public void Install_copies_RetargetHarmony_dll_to_patchers()
        {
            File.WriteAllBytes(Path.Combine(_resourcesDir, "RetargetHarmony.dll"), [7, 8, 9]);

            var result = _service.Install(_gameDir);

            result.IsSuccess.Should().BeTrue();
            File.Exists(Path.Combine(_gameDir, "BepInEx", "patchers", "RetargetHarmony", "RetargetHarmony.dll")).Should().BeTrue();
        }

        [Fact]
        public void Install_copies_BepInEx_distribution_files()
        {
            var bepinexResources = Path.Combine(_resourcesDir, "bepinex");
            Directory.CreateDirectory(bepinexResources);
            File.WriteAllBytes(Path.Combine(bepinexResources, "winhttp.dll"), [0]);
            File.WriteAllBytes(Path.Combine(bepinexResources, "doorstop_config.ini"), [0]);

            var bepinexCore = Path.Combine(bepinexResources, "BepInEx", "core");
            Directory.CreateDirectory(bepinexCore);
            File.WriteAllBytes(Path.Combine(bepinexCore, "BepInEx.dll"), [0]);

            var result = _service.Install(_gameDir);

            result.IsSuccess.Should().BeTrue();
            File.Exists(Path.Combine(_gameDir, "winhttp.dll")).Should().BeTrue();
            File.Exists(Path.Combine(_gameDir, "doorstop_config.ini")).Should().BeTrue();
            File.Exists(Path.Combine(_gameDir, "BepInEx", "core", "BepInEx.dll")).Should().BeTrue();
        }

        [Fact]
        public void Install_returns_list_of_written_files()
        {
            File.WriteAllBytes(Path.Combine(_resourcesDir, "0Harmony109.dll"), [0]);
            File.WriteAllBytes(Path.Combine(_resourcesDir, "RetargetHarmony.dll"), [0]);

            var result = _service.Install(_gameDir);

            result.IsSuccess.Should().BeTrue();
            result.FilesWritten.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public void Install_succeeds_even_without_resource_files()
        {
            var result = _service.Install(_gameDir);

            result.IsSuccess.Should().BeTrue();
            result.FilesWritten.Should().BeEmpty();
        }
    }
}
