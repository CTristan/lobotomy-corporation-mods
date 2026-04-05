// SPDX-License-Identifier: MIT

using System.IO;
using LobotomyPlaywright.Implementations.Deployment;
using LobotomyPlaywright.Interfaces.System;
using Moq;
using Xunit;

namespace LobotomyPlaywright.Tests.Deployment
{
    public sealed class PlaywrightConfigLoaderTests
    {
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly string _configPath = "/test/repo/playwright.json";

        public PlaywrightConfigLoaderTests()
        {
            _mockFileSystem = new Mock<IFileSystem>();
            _ = _mockFileSystem.Setup(f => f.FileExists(_configPath)).Returns(true);
        }

        [Fact]
        public void Load_returns_config_with_deploy_targets_and_profiles()
        {
            // Arrange
            var json = /*lang=json,strict*/ """
                {
                    "deployTargets": [
                        { "projectName": "Mod.A", "assemblyName": "Mod.A", "deploySubdir": "BaseMods/Mod.A", "isMod": true },
                        { "projectName": "Patcher.B", "assemblyName": "Patcher.B", "deploySubdir": "patchers/Patcher.B", "isMod": false }
                    ],
                    "profiles": {
                        "vanilla": { "DeployTargets": [], "InstallLmm": false, "InstallModLoader": false },
                        "dev": { "DeployTargets": ["Mod.A"], "InstallLmm": true, "InstallModLoader": true }
                    },
                    "thirdPartyModsPath": "external/thirdparty-mods",
                    "bepInExSourcePath": "Resources/bepinex"
                }
                """;
            _ = _mockFileSystem.Setup(f => f.ReadAllText(_configPath)).Returns(json);
            var loader = new PlaywrightConfigLoader(_mockFileSystem.Object, _configPath);

            // Act
            var config = loader.Load();

            // Assert
            Assert.Equal(2, config.DeployTargets.Count);
            Assert.Equal("Mod.A", config.DeployTargets[0].ProjectName);
            Assert.True(config.DeployTargets[0].IsMod);
            Assert.Equal("Patcher.B", config.DeployTargets[1].ProjectName);
            Assert.False(config.DeployTargets[1].IsMod);

            Assert.Equal(2, config.Profiles.Count);
            Assert.False(config.Profiles["vanilla"].InstallLmm);
            Assert.True(config.Profiles["dev"].InstallLmm);
            Assert.Contains("Mod.A", config.Profiles["dev"].DeployTargets);

            Assert.Equal("external/thirdparty-mods", config.ThirdPartyModsPath);
            Assert.Equal("Resources/bepinex", config.BepInExSourcePath);
        }

        [Fact]
        public void Load_deserializes_deploy_target_with_custom_project_path()
        {
            // Arrange
            var json = /*lang=json,strict*/ """
                {
                    "deployTargets": [
                        {
                            "projectName": "DemoMod",
                            "assemblyName": "DemoMod",
                            "deploySubdir": "BaseMods/DemoMod",
                            "isMod": true,
                            "projectPath": "SubRepo/DemoMod/DemoMod.csproj"
                        }
                    ],
                    "profiles": {}
                }
                """;
            _ = _mockFileSystem.Setup(f => f.ReadAllText(_configPath)).Returns(json);
            var loader = new PlaywrightConfigLoader(_mockFileSystem.Object, _configPath);

            // Act
            var config = loader.Load();

            // Assert
            Assert.Single(config.DeployTargets);
            Assert.Equal("SubRepo/DemoMod/DemoMod.csproj", config.DeployTargets[0].ProjectPath);
        }

        [Fact]
        public void Load_deserializes_harmony_interop_config()
        {
            // Arrange
            var json = /*lang=json,strict*/ """
                {
                    "deployTargets": [],
                    "profiles": {},
                    "harmonyInteropDlls": ["0Harmony109.dll", "0Harmony12.dll"],
                    "harmonyInteropSourcePath": "RetargetHarmony/lib"
                }
                """;
            _ = _mockFileSystem.Setup(f => f.ReadAllText(_configPath)).Returns(json);
            var loader = new PlaywrightConfigLoader(_mockFileSystem.Object, _configPath);

            // Act
            var config = loader.Load();

            // Assert
            Assert.NotNull(config.HarmonyInteropDlls);
            Assert.Equal(2, config.HarmonyInteropDlls!.Count);
            Assert.Equal("RetargetHarmony/lib", config.HarmonyInteropSourcePath);
        }

        [Fact]
        public void Load_deserializes_profile_deploy_overrides()
        {
            // Arrange
            var json = /*lang=json,strict*/ """
                {
                    "deployTargets": [
                        { "projectName": "Plugin", "assemblyName": "Plugin", "deploySubdir": "BaseMods/Plugin", "isMod": true }
                    ],
                    "profiles": {
                        "custom": {
                            "DeployTargets": ["Plugin"],
                            "InstallLmm": false,
                            "InstallModLoader": true,
                            "DeployOverrides": { "Plugin": "plugins/Plugin" }
                        }
                    }
                }
                """;
            _ = _mockFileSystem.Setup(f => f.ReadAllText(_configPath)).Returns(json);
            var loader = new PlaywrightConfigLoader(_mockFileSystem.Object, _configPath);

            // Act
            var config = loader.Load();

            // Assert
            Assert.NotNull(config.Profiles["custom"].DeployOverrides);
            Assert.Equal("plugins/Plugin", config.Profiles["custom"].DeployOverrides!["Plugin"]);
        }

        [Fact]
        public void Load_throws_when_file_not_found()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.FileExists(_configPath)).Returns(false);
            var loader = new PlaywrightConfigLoader(_mockFileSystem.Object, _configPath);

            // Act & Assert
            _ = Assert.Throws<FileNotFoundException>(loader.Load);
        }

        [Fact]
        public void Load_throws_when_file_is_empty()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.ReadAllText(_configPath)).Returns(string.Empty);
            var loader = new PlaywrightConfigLoader(_mockFileSystem.Object, _configPath);

            // Act & Assert
            _ = Assert.Throws<System.InvalidOperationException>(loader.Load);
        }

        [Fact]
        public void Load_throws_when_json_is_invalid()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.ReadAllText(_configPath)).Returns("not json");
            var loader = new PlaywrightConfigLoader(_mockFileSystem.Object, _configPath);

            // Act & Assert
            _ = Assert.Throws<System.InvalidOperationException>(loader.Load);
        }
    }
}
