// SPDX-License-Identifier: MIT

using System.IO;
using LobotomyPlaywright.Implementations.Deployment;
using LobotomyPlaywright.Interfaces.System;
using Moq;
using Xunit;

namespace LobotomyPlaywright.Tests.Deployment
{
    public sealed class ProfileLoaderTests
    {
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly string _profilesPath = "/test/repo/profiles.json";

        public ProfileLoaderTests()
        {
            _mockFileSystem = new Mock<IFileSystem>();
            _ = _mockFileSystem.Setup(f => f.FileExists(_profilesPath)).Returns(true);
        }

        [Fact]
        public void Load_returns_profiles_from_json_file()
        {
            // Arrange
            var json = /*lang=json,strict*/ """
                {
                    "vanilla": { "DeployTargets": [], "InstallLmm": false, "InstallModLoader": false },
                    "lmm": { "DeployTargets": [], "InstallLmm": true, "InstallModLoader": false }
                }
                """;
            _ = _mockFileSystem.Setup(f => f.ReadAllText(_profilesPath)).Returns(json);
            var loader = new ProfileLoader(_mockFileSystem.Object, _profilesPath);

            // Act
            var profiles = loader.Load();

            // Assert
            Assert.Equal(2, profiles.Count);
            Assert.False(profiles["vanilla"].InstallLmm);
            Assert.True(profiles["lmm"].InstallLmm);
        }

        [Fact]
        public void Load_deserializes_deploy_targets()
        {
            // Arrange
            var json = /*lang=json,strict*/ """
                {
                    "mods": {
                        "DeployTargets": ["Mod.A", "Mod.B"],
                        "InstallLmm": true,
                        "InstallModLoader": false
                    }
                }
                """;
            _ = _mockFileSystem.Setup(f => f.ReadAllText(_profilesPath)).Returns(json);
            var loader = new ProfileLoader(_mockFileSystem.Object, _profilesPath);

            // Act
            var profiles = loader.Load();

            // Assert
            Assert.Equal(2, profiles["mods"].DeployTargets.Count);
            Assert.Contains("Mod.A", profiles["mods"].DeployTargets);
            Assert.Contains("Mod.B", profiles["mods"].DeployTargets);
        }

        [Fact]
        public void Load_deserializes_deploy_overrides()
        {
            // Arrange
            var json = /*lang=json,strict*/ """
                {
                    "custom": {
                        "DeployTargets": ["Plugin"],
                        "InstallLmm": false,
                        "InstallModLoader": true,
                        "DeployOverrides": { "Plugin": "plugins/Plugin" }
                    }
                }
                """;
            _ = _mockFileSystem.Setup(f => f.ReadAllText(_profilesPath)).Returns(json);
            var loader = new ProfileLoader(_mockFileSystem.Object, _profilesPath);

            // Act
            var profiles = loader.Load();

            // Assert
            Assert.NotNull(profiles["custom"].DeployOverrides);
            Assert.Equal("plugins/Plugin", profiles["custom"].DeployOverrides!["Plugin"]);
        }

        [Fact]
        public void Load_throws_when_file_not_found()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.FileExists(_profilesPath)).Returns(false);
            var loader = new ProfileLoader(_mockFileSystem.Object, _profilesPath);

            // Act & Assert
            _ = Assert.Throws<FileNotFoundException>(loader.Load);
        }

        [Fact]
        public void Load_throws_when_file_is_empty()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.ReadAllText(_profilesPath)).Returns(string.Empty);
            var loader = new ProfileLoader(_mockFileSystem.Object, _profilesPath);

            // Act & Assert
            _ = Assert.Throws<System.InvalidOperationException>(loader.Load);
        }

        [Fact]
        public void Load_throws_when_json_is_invalid()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.ReadAllText(_profilesPath)).Returns("not json");
            var loader = new ProfileLoader(_mockFileSystem.Object, _profilesPath);

            // Act & Assert
            _ = Assert.Throws<System.InvalidOperationException>(loader.Load);
        }
    }
}
