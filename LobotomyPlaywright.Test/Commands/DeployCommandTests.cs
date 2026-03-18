// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using AwesomeAssertions;
using LobotomyPlaywright.Commands;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.Deployment;
using LobotomyPlaywright.Interfaces.System;
using Moq;
using Xunit;

namespace LobotomyPlaywright.Tests.Commands
{
    public sealed class DeployCommandTests
    {
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly Mock<IConfigManager> _mockConfigManager;
        private readonly Mock<IProcessRunner> _mockProcessRunner;
        private readonly Mock<IGameRestorer> _mockGameRestorer;
        private readonly Mock<ILmmInstaller> _mockLmmInstaller;
        private readonly Mock<IBepInExInstaller> _mockBepInExInstaller;
        private readonly Mock<IProfileLoader> _mockProfileLoader;
        private readonly DeployCommand _deployCommand;
        private readonly string _gamePath = "/test/game/path";
        private readonly string _repoRoot = "/test/repo/root";

        public DeployCommandTests()
        {
            _mockFileSystem = new Mock<IFileSystem>();
            _mockConfigManager = new Mock<IConfigManager>();
            _mockProcessRunner = new Mock<IProcessRunner>();
            _mockGameRestorer = new Mock<IGameRestorer>();
            _mockLmmInstaller = new Mock<ILmmInstaller>();
            _mockBepInExInstaller = new Mock<IBepInExInstaller>();
            _mockProfileLoader = new Mock<IProfileLoader>();
            _deployCommand = new DeployCommand(_mockConfigManager.Object, _mockFileSystem.Object, _mockProcessRunner.Object, _mockGameRestorer.Object, _mockLmmInstaller.Object, _mockBepInExInstaller.Object, _mockProfileLoader.Object);

            // Setup default config
            Config config = new()
            {
                GamePath = _gamePath
            };
            _ = _mockConfigManager.Setup(c => c.Load()).Returns(config);
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(_gamePath)).Returns(true);
            _ = _mockFileSystem.Setup(f => f.GetCurrentDirectory()).Returns(_repoRoot);
            _ = _mockFileSystem.Setup(f => f.FileExists(Path.Combine(_repoRoot, "LobotomyCorporationMods.sln"))).Returns(true);
            _ = _mockFileSystem.Setup(f => f.GetFileSize(It.IsAny<string>())).Returns(100);
            _ = _mockFileSystem.Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns([]);
        }

        [Fact]
        public void Run_Deployment_CopiesAllRequiredFiles()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
            _ = _mockFileSystem.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);
            _ = _mockFileSystem.Setup(f => f.GetFiles(It.IsAny<string>(), "Hemocode.Common.*.dll"))
                .Returns(["Hemocode.Common.6.0.2.dll"]);

            _ = _mockProcessRunner.Setup(p => p.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string?, bool>>()))
                .Returns(0);

            // Act
            int result = _deployCommand.Run([]);

            // Assert
            _ = result.Should().Be(0);

            // Verify tool project DLL deployments
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("Hemocode.Playwright.dll")), It.IsAny<string>(), true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("RetargetHarmony.dll")), It.IsAny<string>(), true), Times.Once);

            // Verify mod DLL deployments
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("Hemocode.BadLuckProtectionForGifts.dll")), It.IsAny<string>(), true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("Hemocode.BugFixes.dll")), It.IsAny<string>(), true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("Hemocode.DebugPanel.dll")), It.IsAny<string>(), true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("Hemocode.FreeCustomization.dll")), It.IsAny<string>(), true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("Hemocode.GiftAlertIcon.dll")), It.IsAny<string>(), true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("Hemocode.NotifyWhenAgentReceivesGift.dll")), It.IsAny<string>(), true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("Hemocode.WarnWhenAgentWillDieFromWorking.dll")), It.IsAny<string>(), true), Times.Once);

            // Verify Common DLL deployed for each mod (8 mods)
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("Hemocode.Common")), It.IsAny<string>(), true), Times.Exactly(8));

            // Verify interop DLLs
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("0Harmony109.dll")), It.IsAny<string>(), true), Times.Once);
            // 0Harmony12.dll is copied twice: once for itself, once as source for 12Harmony.dll
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("0Harmony12.dll")), It.IsAny<string>(), true), Times.Exactly(2));
        }

        [Fact]
        public void Run_BuildPhase_HandlesBuildFailures()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.FileExists(It.Is<string>(s => s.EndsWith("LobotomyCorporationMods.sln")))).Returns(true);
            _ = _mockFileSystem.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);

            _ = _mockProcessRunner.Setup(p => p.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string?, bool>>()))
                .Returns(1); // Failure

            // Act
            int result = _deployCommand.Run([]);

            // Assert
            _ = result.Should().Be(1);
        }

        [Fact]
        public void Run_Deployment_DeploysModContentDirectories()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
            _ = _mockFileSystem.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);
            _ = _mockFileSystem.Setup(f => f.GetFiles(It.IsAny<string>(), "Hemocode.Common.*.dll"))
                .Returns(["Hemocode.Common.6.0.2.dll"]);

            _ = _mockProcessRunner.Setup(p => p.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string?, bool>>()))
                .Returns(0);

            // Act
            int result = _deployCommand.Run([]);

            // Assert
            _ = result.Should().Be(0);

            // Verify CopyDirectory called for content dirs (Info, Assets, Localize, Data exist for all 8 mods since DirectoryExists returns true)
            _mockFileSystem.Verify(f => f.CopyDirectory(It.IsAny<string>(), It.IsAny<string>(), true), Times.Exactly(32));
        }

        [Fact]
        public void Run_with_profile_restores_game_before_deploying()
        {
            // Arrange
            SetupProfileLoader();
            SetupSuccessfulBuildAndDeploy();

            // Act
            int result = _deployCommand.Run(["--profile", "all"]);

            // Assert
            _ = result.Should().Be(0);
            _mockGameRestorer.Verify(r => r.RestoreTargeted(_gamePath, Path.Combine(_repoRoot, "external", "snapshots")), Times.Once);
        }

        [Fact]
        public void Run_with_full_flag_uses_full_restore()
        {
            // Arrange
            SetupProfileLoader();
            SetupSuccessfulBuildAndDeploy();

            // Act
            int result = _deployCommand.Run(["--profile", "all", "--full"]);

            // Assert
            _ = result.Should().Be(0);
            _mockGameRestorer.Verify(r => r.RestoreFull(_gamePath, Path.Combine(_repoRoot, "external", "snapshots")), Times.Once);
        }

        [Fact]
        public void Run_with_vanilla_profile_deploys_nothing_after_clean()
        {
            // Arrange
            SetupProfileLoader();

            // Act
            int result = _deployCommand.Run(["--profile", "vanilla"]);

            // Assert
            _ = result.Should().Be(0);
            _mockGameRestorer.Verify(r => r.RestoreTargeted(_gamePath, It.IsAny<string>()), Times.Once);
            _mockLmmInstaller.Verify(i => i.Install(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockBepInExInstaller.Verify(i => i.Install(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockProcessRunner.Verify(p => p.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string?, bool>>()), Times.Never);
        }

        [Fact]
        public void Run_with_lmm_profile_installs_lmm_but_not_mod_loader()
        {
            // Arrange
            SetupProfileLoader();

            // Act
            int result = _deployCommand.Run(["--profile", "lmm"]);

            // Assert
            _ = result.Should().Be(0);
            _mockLmmInstaller.Verify(i => i.Install(_gamePath, Path.Combine(_repoRoot, "external", "snapshots", "LobotomyModManager")), Times.Once);
            _mockBepInExInstaller.Verify(i => i.Install(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Run_with_bepinex_profile_installs_mod_loader_but_not_lmm()
        {
            // Arrange
            SetupProfileLoader();

            // Act
            int result = _deployCommand.Run(["--profile", "bepinex"]);

            // Assert
            _ = result.Should().Be(0);
            _mockBepInExInstaller.Verify(i => i.Install(_gamePath, Path.Combine(_repoRoot, "RetargetHarmony.Installer", "Resources", "bepinex")), Times.Once);
            _mockLmmInstaller.Verify(i => i.Install(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Run_with_mods_profile_deploys_all_mods_but_not_retarget()
        {
            // Arrange
            SetupProfileLoader();
            SetupSuccessfulBuildAndDeploy();

            // Act
            int result = _deployCommand.Run(["--profile", "mods"]);

            // Assert
            _ = result.Should().Be(0);
            _mockLmmInstaller.Verify(i => i.Install(_gamePath, It.IsAny<string>()), Times.Once);
            _mockBepInExInstaller.Verify(i => i.Install(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

            // All mod DLLs deployed including Playwright
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("Hemocode.Playwright.dll")), It.IsAny<string>(), true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("Hemocode.BadLuckProtectionForGifts.dll")), It.IsAny<string>(), true), Times.Once);

            // RetargetHarmony not deployed in mods profile
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("RetargetHarmony.dll")), It.IsAny<string>(), true), Times.Never);
        }

        [Fact]
        public void Run_with_unknown_profile_returns_error()
        {
            // Arrange
            SetupProfileLoader();

            // Act
            int result = _deployCommand.Run(["--profile", "nonexistent"]);

            // Assert
            _ = result.Should().Be(1);
        }

        [Fact]
        public void Run_with_deploy_overrides_routes_plugin_to_bepinex_plugins()
        {
            // Arrange
            var profiles = new Dictionary<string, DeploymentProfile>
            {
                ["bepinex-playwright"] = new DeploymentProfile
                {
                    DeployTargets = new Collection<string>(["LobotomyCorporationMods.Playwright", "RetargetHarmony"]),
                    InstallLmm = false,
                    InstallModLoader = true,
                    DeployOverrides = new Dictionary<string, string>
                    {
                        ["LobotomyCorporationMods.Playwright"] = "plugins/Hemocode.Playwright"
                    }
                }
            };
            _ = _mockProfileLoader.Setup(p => p.Load()).Returns(profiles);
            SetupSuccessfulBuildAndDeploy();

            // Act
            int result = _deployCommand.Run(["--profile", "bepinex-playwright"]);

            // Assert
            _ = result.Should().Be(0);

            // Plugin should be deployed to BepInEx/plugins/Hemocode.Playwright, not BaseMods
            _mockFileSystem.Verify(f => f.CopyFile(
                It.Is<string>(s => s.Contains("Hemocode.Playwright.dll")),
                It.Is<string>(s => s.Contains(Path.Combine("BepInEx", "plugins", "Hemocode.Playwright"))),
                true), Times.Once);

            // RetargetHarmony should still go to BepInEx/patchers (no override)
            _mockFileSystem.Verify(f => f.CopyFile(
                It.Is<string>(s => s.Contains("RetargetHarmony.dll")),
                It.Is<string>(s => s.Contains(Path.Combine("BepInEx", "patchers", "RetargetHarmony"))),
                true), Times.Once);
        }

        [Fact]
        public void Run_without_deploy_overrides_uses_default_paths()
        {
            // Arrange
            SetupProfileLoader();
            SetupSuccessfulBuildAndDeploy();

            // Act
            int result = _deployCommand.Run(["--profile", "mods"]);

            // Assert
            _ = result.Should().Be(0);

            // Playwright should go to BaseMods (default, no override)
            _mockFileSystem.Verify(f => f.CopyFile(
                It.Is<string>(s => s.Contains("Hemocode.Playwright.dll")),
                It.Is<string>(s => s.Contains(Path.Combine("BaseMods", "Hemocode.Playwright"))),
                true), Times.Once);
        }

        [Fact]
        public void Run_with_profile_but_profiles_file_missing_returns_error()
        {
            // Arrange
            _ = _mockProfileLoader.Setup(p => p.Load()).Throws(new FileNotFoundException("Profiles file not found"));

            // Act
            int result = _deployCommand.Run(["--profile", "vanilla"]);

            // Assert
            _ = result.Should().Be(1);
        }

        [Fact]
        public void Run_with_profile_creates_snapshot_directory_when_missing()
        {
            // Arrange - use raw profile loader without SetupProfileLoader (which sets vanilla path to exist)
            var profiles = new Dictionary<string, DeploymentProfile>
            {
                ["vanilla"] = new DeploymentProfile { DeployTargets = [], InstallLmm = false, InstallModLoader = false }
            };
            _ = _mockProfileLoader.Setup(p => p.Load()).Returns(profiles);

            // Vanilla path does NOT exist (default mock behavior)
            var vanillaManagedPath = Path.Combine(_repoRoot, "external", "snapshots", "LobotomyCorp_vanilla", "LobotomyCorp_Data", "Managed");

            // Act
            int result = _deployCommand.Run(["--profile", "vanilla"]);

            // Assert
            _ = result.Should().Be(1);
            _mockFileSystem.Verify(f => f.CreateDirectory(vanillaManagedPath), Times.Once);
            _mockGameRestorer.Verify(r => r.RestoreTargeted(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Run_with_profile_creates_LMM_directory_when_missing()
        {
            // Arrange - vanilla exists but LMM does not
            var vanillaPath = Path.Combine(_repoRoot, "external", "snapshots", "LobotomyCorp_vanilla");
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(vanillaPath)).Returns(true);

            var profiles = new Dictionary<string, DeploymentProfile>
            {
                ["lmm"] = new DeploymentProfile { DeployTargets = [], InstallLmm = true, InstallModLoader = false }
            };
            _ = _mockProfileLoader.Setup(p => p.Load()).Returns(profiles);

            // LMM path does NOT exist (default mock behavior)
            var lmmPath = Path.Combine(_repoRoot, "external", "snapshots", "LobotomyModManager");

            // Act
            int result = _deployCommand.Run(["--profile", "lmm"]);

            // Assert
            _ = result.Should().Be(1);
            _mockFileSystem.Verify(f => f.CreateDirectory(lmmPath), Times.Once);
            _mockLmmInstaller.Verify(i => i.Install(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Run_without_profile_deploys_all_targets_unchanged()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
            _ = _mockFileSystem.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);
            _ = _mockFileSystem.Setup(f => f.GetFiles(It.IsAny<string>(), "Hemocode.Common.*.dll"))
                .Returns(["Hemocode.Common.6.0.2.dll"]);
            _ = _mockProcessRunner.Setup(p => p.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string?, bool>>()))
                .Returns(0);

            // Act
            int result = _deployCommand.Run([]);

            // Assert
            _ = result.Should().Be(0);
            _mockGameRestorer.Verify(r => r.RestoreTargeted(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockGameRestorer.Verify(r => r.RestoreFull(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

            // All 9 targets deployed
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("Hemocode.Playwright.dll")), It.IsAny<string>(), true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("Hemocode.BadLuckProtectionForGifts.dll")), It.IsAny<string>(), true), Times.Once);
        }

        private void SetupProfileLoader()
        {
            // Vanilla snapshot must exist for profile-based deployment
            var vanillaPath = Path.Combine(_repoRoot, "external", "snapshots", "LobotomyCorp_vanilla");
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(vanillaPath)).Returns(true);

            // LMM source must exist for profiles that install LMM
            var lmmPath = Path.Combine(_repoRoot, "external", "snapshots", "LobotomyModManager");
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(lmmPath)).Returns(true);

            var profiles = new Dictionary<string, DeploymentProfile>
            {
                ["vanilla"] = new DeploymentProfile { DeployTargets = [], InstallLmm = false, InstallModLoader = false },
                ["lmm"] = new DeploymentProfile { DeployTargets = [], InstallLmm = true, InstallModLoader = false },
                ["bepinex"] = new DeploymentProfile { DeployTargets = [], InstallLmm = false, InstallModLoader = true },
                ["mods"] = new DeploymentProfile
                {
                    DeployTargets = new Collection<string>(
                    [
                        "LobotomyCorporationMods.BadLuckProtectionForGifts", "LobotomyCorporationMods.BugFixes",
                        "LobotomyCorporationMods.DebugPanel", "LobotomyCorporationMods.FreeCustomization",
                        "LobotomyCorporationMods.GiftAlertIcon", "LobotomyCorporationMods.NotifyWhenAgentReceivesGift",
                        "LobotomyCorporationMods.Playwright", "LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking"
                    ]),
                    InstallLmm = true,
                    InstallModLoader = false
                },
                ["dev"] = new DeploymentProfile
                {
                    DeployTargets = new Collection<string>(
                    [
                        "LobotomyCorporationMods.BadLuckProtectionForGifts", "LobotomyCorporationMods.BugFixes",
                        "LobotomyCorporationMods.DebugPanel", "LobotomyCorporationMods.FreeCustomization",
                        "LobotomyCorporationMods.GiftAlertIcon", "LobotomyCorporationMods.NotifyWhenAgentReceivesGift",
                        "LobotomyCorporationMods.Playwright", "LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking",
                        "RetargetHarmony"
                    ]),
                    InstallLmm = true,
                    InstallModLoader = true
                },
                ["all"] = new DeploymentProfile
                {
                    DeployTargets = new Collection<string>(
                    [
                        "LobotomyCorporationMods.BadLuckProtectionForGifts", "LobotomyCorporationMods.BugFixes",
                        "LobotomyCorporationMods.DebugPanel", "LobotomyCorporationMods.FreeCustomization",
                        "LobotomyCorporationMods.GiftAlertIcon", "LobotomyCorporationMods.NotifyWhenAgentReceivesGift",
                        "LobotomyCorporationMods.Playwright", "LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking",
                        "RetargetHarmony"
                    ]),
                    InstallLmm = true,
                    InstallModLoader = true,
                    IncludeThirdPartyMods = true
                }
            };
            _ = _mockProfileLoader.Setup(p => p.Load()).Returns(profiles);
        }

        private void SetupSuccessfulBuildAndDeploy()
        {
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
            _ = _mockFileSystem.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);
            _ = _mockFileSystem.Setup(f => f.GetFiles(It.IsAny<string>(), "Hemocode.Common.*.dll"))
                .Returns(["Hemocode.Common.6.0.2.dll"]);
            _ = _mockProcessRunner.Setup(p => p.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string?, bool>>()))
                .Returns(0);
        }

        private void SetupThirdPartyMods(params (string ModName, string DllName)[] mods)
        {
            var thirdPartyDir = Path.Combine(_repoRoot, "external", "thirdparty-mods");
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(thirdPartyDir)).Returns(true);

            var modDirs = new List<string>();
            foreach (var (modName, dllName) in mods)
            {
                var modDir = Path.Combine(thirdPartyDir, modName);
                modDirs.Add(modDir);
                _ = _mockFileSystem.Setup(f => f.GetFiles(modDir, "*.dll"))
                    .Returns([Path.Combine(modDir, dllName)]);
            }

            _ = _mockFileSystem.Setup(f => f.GetDirectories(thirdPartyDir, "*"))
                .Returns(modDirs.ToArray());
        }

        [Fact]
        public void Run_with_all_profile_deploys_third_party_mods()
        {
            // Arrange
            SetupProfileLoader();
            SetupSuccessfulBuildAndDeploy();
            SetupThirdPartyMods(("SkinTone", "SkinTone.dll"), ("AnotherMod", "AnotherMod.dll"));

            // Act
            int result = _deployCommand.Run(["--profile", "all"]);

            // Assert
            _ = result.Should().Be(0);

            // Verify third-party DLLs deployed to BaseMods
            _mockFileSystem.Verify(f => f.CopyFile(
                It.Is<string>(s => s.Contains("SkinTone.dll")),
                It.Is<string>(s => s.Contains(Path.Combine("BaseMods", "SkinTone"))),
                true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(
                It.Is<string>(s => s.Contains("AnotherMod.dll")),
                It.Is<string>(s => s.Contains(Path.Combine("BaseMods", "AnotherMod"))),
                true), Times.Once);
        }

        [Fact]
        public void Run_with_dev_profile_does_not_deploy_third_party_mods()
        {
            // Arrange
            SetupProfileLoader();
            SetupSuccessfulBuildAndDeploy();
            SetupThirdPartyMods(("SkinTone", "SkinTone.dll"));

            // Act
            int result = _deployCommand.Run(["--profile", "dev"]);

            // Assert
            _ = result.Should().Be(0);

            // Third-party mod should NOT be deployed
            _mockFileSystem.Verify(f => f.CopyFile(
                It.Is<string>(s => s.Contains("SkinTone.dll")),
                It.IsAny<string>(),
                true), Times.Never);
        }

        [Fact]
        public void Run_with_all_profile_skips_third_party_mod_with_no_dll()
        {
            // Arrange
            SetupProfileLoader();
            SetupSuccessfulBuildAndDeploy();

            var thirdPartyDir = Path.Combine(_repoRoot, "external", "thirdparty-mods");
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(thirdPartyDir)).Returns(true);

            var emptyModDir = Path.Combine(thirdPartyDir, "EmptyMod");
            _ = _mockFileSystem.Setup(f => f.GetDirectories(thirdPartyDir, "*"))
                .Returns([emptyModDir]);
            _ = _mockFileSystem.Setup(f => f.GetFiles(emptyModDir, "*.dll"))
                .Returns([]);

            // Act
            int result = _deployCommand.Run(["--profile", "all"]);

            // Assert
            _ = result.Should().Be(0);
        }

        [Fact]
        public void Run_with_all_profile_skips_third_party_mod_with_multiple_dlls()
        {
            // Arrange
            SetupProfileLoader();
            SetupSuccessfulBuildAndDeploy();

            var thirdPartyDir = Path.Combine(_repoRoot, "external", "thirdparty-mods");
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(thirdPartyDir)).Returns(true);

            var ambiguousModDir = Path.Combine(thirdPartyDir, "AmbiguousMod");
            _ = _mockFileSystem.Setup(f => f.GetDirectories(thirdPartyDir, "*"))
                .Returns([ambiguousModDir]);
            _ = _mockFileSystem.Setup(f => f.GetFiles(ambiguousModDir, "*.dll"))
                .Returns([Path.Combine(ambiguousModDir, "Mod.dll"), Path.Combine(ambiguousModDir, "ModHelper.dll")]);

            // Act
            int result = _deployCommand.Run(["--profile", "all"]);

            // Assert
            _ = result.Should().Be(0);

            // Neither DLL should be deployed
            _mockFileSystem.Verify(f => f.CopyFile(
                It.Is<string>(s => s.Contains("Mod.dll")),
                It.Is<string>(s => s.Contains(Path.Combine("BaseMods", "AmbiguousMod"))),
                true), Times.Never);
        }

        [Fact]
        public void Run_with_all_profile_handles_missing_third_party_directory()
        {
            // Arrange
            SetupProfileLoader();
            SetupSuccessfulBuildAndDeploy();

            // Third-party directory does NOT exist (default mock behavior returns false)

            // Act
            int result = _deployCommand.Run(["--profile", "all"]);

            // Assert - should succeed, just no third-party mods deployed
            _ = result.Should().Be(0);
        }

        [Fact]
        public void Run_with_all_profile_deploys_third_party_mod_content_directories()
        {
            // Arrange
            SetupProfileLoader();
            SetupSuccessfulBuildAndDeploy();
            SetupThirdPartyMods(("SkinTone", "SkinTone.dll"));

            var skinToneDir = Path.Combine(_repoRoot, "external", "thirdparty-mods", "SkinTone");

            // Info dir exists for this mod
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(Path.Combine(skinToneDir, "Info"))).Returns(true);

            // Act
            int result = _deployCommand.Run(["--profile", "all"]);

            // Assert
            _ = result.Should().Be(0);

            // Verify Info directory was copied
            _mockFileSystem.Verify(f => f.CopyDirectory(
                Path.Combine(skinToneDir, "Info"),
                It.Is<string>(s => s.Contains(Path.Combine("BaseMods", "SkinTone", "Info"))),
                true), Times.Once);
        }
    }
}

