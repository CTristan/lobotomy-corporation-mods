// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.IO;
using AwesomeAssertions;
using Harmony2ForLmm.Interfaces;
using Harmony2ForLmm.Models;
using Harmony2ForLmm.ViewModels;
using Moq;
using Xunit;

namespace Harmony2ForLmm.Test.ViewModels
{
    /// <summary>
    /// Tests for <see cref="MainWindowViewModel"/>.
    /// </summary>
    public sealed class MainWindowViewModelTests
    {
        private readonly Mock<IGamePathFinder> _mockPathFinder = new();
        private readonly Mock<IGameDirectoryValidator> _mockValidator = new();
        private readonly Mock<IInstallerService> _mockInstaller = new();
        private readonly Mock<IUninstallerService> _mockUninstaller = new();
        private readonly Mock<IBaseModsAnalyzer> _mockAnalyzer = new();
        private readonly Mock<IInstallationStateDetector> _mockStateDetector = new();
        private readonly Mock<IResourceProvider> _mockResourceProvider = new();

        private MainWindowViewModel CreateViewModel()
        {
            _mockValidator.Setup(v => v.Validate(It.IsAny<string>()))
                .Returns(GameDirectoryValidationResult.Failure("Not found"));
            _mockAnalyzer.Setup(a => a.Analyze(It.IsAny<string>()))
                .Returns([]);
            _mockStateDetector.Setup(d => d.Detect(It.IsAny<string>()))
                .Returns(InstallationStateResult.Fresh("1.0.0"));

            return new MainWindowViewModel(
                _mockPathFinder.Object,
                _mockValidator.Object,
                _mockInstaller.Object,
                _mockUninstaller.Object,
                _mockAnalyzer.Object,
                _mockStateDetector.Object,
                _mockResourceProvider.Object);
        }

        private MainWindowViewModel CreateViewModelWithValidPath()
        {
            _mockValidator.Setup(v => v.Validate(It.IsAny<string>()))
                .Returns(GameDirectoryValidationResult.Success());
            _mockAnalyzer.Setup(a => a.Analyze(It.IsAny<string>()))
                .Returns([]);
            _mockStateDetector.Setup(d => d.Detect(It.IsAny<string>()))
                .Returns(InstallationStateResult.Fresh("1.0.0"));

            return new MainWindowViewModel(
                _mockPathFinder.Object,
                _mockValidator.Object,
                _mockInstaller.Object,
                _mockUninstaller.Object,
                _mockAnalyzer.Object,
                _mockStateDetector.Object,
                _mockResourceProvider.Object);
        }

        [Fact]
        public void IsActionCompleted_starts_as_false()
        {
            var vm = CreateViewModel();

            vm.IsActionCompleted.Should().BeFalse();
        }

        [Fact]
        public void Successful_install_sets_IsActionCompleted_to_true()
        {
            _mockInstaller.Setup(i => i.Install(It.IsAny<string>()))
                .Returns(InstallResult.Success(["file.dll"]));
            var vm = CreateViewModelWithValidPath();
            vm.GamePath = "/valid/path";

            vm.PrimaryActionCommand.Execute(null);

            vm.IsActionCompleted.Should().BeTrue();
        }

        [Fact]
        public void Failed_install_does_not_set_IsActionCompleted()
        {
            _mockInstaller.Setup(i => i.Install(It.IsAny<string>()))
                .Returns(InstallResult.Failure("Something went wrong"));
            var vm = CreateViewModelWithValidPath();
            vm.GamePath = "/valid/path";

            vm.PrimaryActionCommand.Execute(null);

            vm.IsActionCompleted.Should().BeFalse();
        }

        [Fact]
        public void Successful_install_upgrades_DebugPanel_when_detected()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var debugPanelDir = Path.Combine(tempDir, "LobotomyCorp_Data", "BaseMods", "DebugPanel");
            Directory.CreateDirectory(debugPanelDir);
            try
            {
                _mockInstaller.Setup(i => i.Install(It.IsAny<string>()))
                    .Returns(InstallResult.Success(["file.dll"]));
                var vm = CreateViewModelWithValidPath();
                vm.GamePath = tempDir;

                vm.PrimaryActionCommand.Execute(null);

                _mockResourceProvider.Verify(
                    r => r.ExtractDebugPanelTo(It.IsAny<string>(), It.IsAny<ICollection<string>>()),
                    Times.Once());
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }

        [Fact]
        public void Successful_install_does_not_upgrade_DebugPanel_when_not_detected()
        {
            _mockInstaller.Setup(i => i.Install(It.IsAny<string>()))
                .Returns(InstallResult.Success(["file.dll"]));
            var vm = CreateViewModelWithValidPath();
            vm.GamePath = "/valid/path";

            vm.PrimaryActionCommand.Execute(null);

            _mockResourceProvider.Verify(
                r => r.ExtractDebugPanelTo(It.IsAny<string>(), It.IsAny<ICollection<string>>()),
                Times.Never());
        }

        [Fact]
        public void Successful_uninstall_sets_IsActionCompleted_to_true()
        {
            _mockStateDetector.Setup(d => d.Detect(It.IsAny<string>()))
                .Returns(InstallationStateResult.WithState(InstallationState.Current, "1.0.0", "1.0.0"));
            _mockUninstaller.Setup(u => u.Uninstall(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(UninstallResult.Success(["file.dll"], []));
            var vm = CreateViewModelWithValidPath();
            vm.GamePath = "/valid/path";

            vm.UninstallCommand.Execute(null);

            vm.IsActionCompleted.Should().BeTrue();
        }

        [Fact]
        public void Failed_uninstall_does_not_set_IsActionCompleted()
        {
            _mockStateDetector.Setup(d => d.Detect(It.IsAny<string>()))
                .Returns(InstallationStateResult.WithState(InstallationState.Current, "1.0.0", "1.0.0"));
            _mockUninstaller.Setup(u => u.Uninstall(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(UninstallResult.Failure("Something went wrong"));
            var vm = CreateViewModelWithValidPath();
            vm.GamePath = "/valid/path";

            vm.UninstallCommand.Execute(null);

            vm.IsActionCompleted.Should().BeFalse();
        }

        [Fact]
        public void ShowPrimaryAction_returns_false_when_IsActionCompleted()
        {
            _mockInstaller.Setup(i => i.Install(It.IsAny<string>()))
                .Returns(InstallResult.Success(["file.dll"]));
            var vm = CreateViewModelWithValidPath();
            vm.GamePath = "/valid/path";

            vm.PrimaryActionCommand.Execute(null);

            vm.ShowPrimaryAction.Should().BeFalse();
        }

        [Fact]
        public void ShowUninstallAction_returns_false_when_IsActionCompleted()
        {
            _mockStateDetector.Setup(d => d.Detect(It.IsAny<string>()))
                .Returns(InstallationStateResult.WithState(InstallationState.Current, "1.0.0", "1.0.0"));
            _mockUninstaller.Setup(u => u.Uninstall(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(UninstallResult.Success(["file.dll"], []));
            var vm = CreateViewModelWithValidPath();
            vm.GamePath = "/valid/path";

            vm.UninstallCommand.Execute(null);

            vm.ShowUninstallAction.Should().BeFalse();
        }

        [Fact]
        public void IsPathEditable_is_false_when_IsActionCompleted()
        {
            _mockInstaller.Setup(i => i.Install(It.IsAny<string>()))
                .Returns(InstallResult.Success(["file.dll"]));
            var vm = CreateViewModelWithValidPath();
            vm.GamePath = "/valid/path";

            vm.PrimaryActionCommand.Execute(null);

            vm.IsPathEditable.Should().BeFalse();
        }

        [Fact]
        public void IsPathEditable_is_true_when_not_working_and_not_completed()
        {
            var vm = CreateViewModel();

            vm.IsPathEditable.Should().BeTrue();
        }

        [Fact]
        public void AutoDetectCommand_cannot_execute_when_IsActionCompleted()
        {
            _mockInstaller.Setup(i => i.Install(It.IsAny<string>()))
                .Returns(InstallResult.Success(["file.dll"]));
            var vm = CreateViewModelWithValidPath();
            vm.GamePath = "/valid/path";

            vm.PrimaryActionCommand.Execute(null);

            vm.AutoDetectCommand.CanExecute(null).Should().BeFalse();
        }

        [Fact]
        public void IsLastActionFailed_starts_as_false()
        {
            var vm = CreateViewModel();

            vm.IsLastActionFailed.Should().BeFalse();
        }

        [Fact]
        public void Failed_install_sets_IsLastActionFailed_to_true()
        {
            _mockInstaller.Setup(i => i.Install(It.IsAny<string>()))
                .Returns(InstallResult.Failure("Something went wrong"));
            var vm = CreateViewModelWithValidPath();
            vm.GamePath = "/valid/path";

            vm.PrimaryActionCommand.Execute(null);

            vm.IsLastActionFailed.Should().BeTrue();
        }

        [Fact]
        public void Successful_install_does_not_set_IsLastActionFailed()
        {
            _mockInstaller.Setup(i => i.Install(It.IsAny<string>()))
                .Returns(InstallResult.Success(["file.dll"]));
            var vm = CreateViewModelWithValidPath();
            vm.GamePath = "/valid/path";

            vm.PrimaryActionCommand.Execute(null);

            vm.IsLastActionFailed.Should().BeFalse();
        }

        [Fact]
        public void Failed_uninstall_sets_IsLastActionFailed_to_true()
        {
            _mockStateDetector.Setup(d => d.Detect(It.IsAny<string>()))
                .Returns(InstallationStateResult.WithState(InstallationState.Current, "1.0.0", "1.0.0"));
            _mockUninstaller.Setup(u => u.Uninstall(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(UninstallResult.Failure("Something went wrong"));
            var vm = CreateViewModelWithValidPath();
            vm.GamePath = "/valid/path";

            vm.UninstallCommand.Execute(null);

            vm.IsLastActionFailed.Should().BeTrue();
        }

        [Fact]
        public void Successful_install_resets_IsLastActionFailed_from_previous_failure()
        {
            _mockInstaller.SetupSequence(i => i.Install(It.IsAny<string>()))
                .Returns(InstallResult.Failure("First attempt failed"))
                .Returns(InstallResult.Success(["file.dll"]));
            var vm = CreateViewModelWithValidPath();
            vm.GamePath = "/valid/path";

            vm.PrimaryActionCommand.Execute(null);
            vm.IsLastActionFailed.Should().BeTrue();

            vm.PrimaryActionCommand.Execute(null);
            vm.IsLastActionFailed.Should().BeFalse();
        }

        [Fact]
        public void GuidesCommand_invokes_open_guides_action()
        {
            var vm = CreateViewModel();
            var invoked = false;
            vm.SetOpenGuidesAction(() => invoked = true);

            vm.GuidesCommand.Execute(null);

            invoked.Should().BeTrue();
        }

        [Fact]
        public void TroubleshootingCommand_invokes_open_troubleshooting_action()
        {
            var vm = CreateViewModel();
            var invoked = false;
            vm.SetOpenTroubleshootingAction(() => invoked = true);

            vm.TroubleshootingCommand.Execute(null);

            invoked.Should().BeTrue();
        }

        [Fact]
        public void InstallDebugPanel_returns_empty_string_on_success()
        {
            var vm = CreateViewModelWithValidPath();
            vm.GamePath = "/valid/path";

            var result = vm.InstallDebugPanel();

            result.Should().BeEmpty();
            _mockResourceProvider.Verify(
                r => r.ExtractDebugPanelTo(It.IsAny<string>(), It.IsAny<ICollection<string>>()),
                Times.Once());
        }

        [Fact]
        public void InstallDebugPanel_returns_error_message_on_failure()
        {
            _mockResourceProvider
                .Setup(r => r.ExtractDebugPanelTo(It.IsAny<string>(), It.IsAny<ICollection<string>>()))
                .Throws(new IOException("Permission denied"));
            var vm = CreateViewModelWithValidPath();
            vm.GamePath = "/valid/path";

            var result = vm.InstallDebugPanel();

            result.Should().Be("Permission denied");
        }

        [Fact]
        public void IsDebugPanelInstalled_returns_false_when_dll_does_not_exist()
        {
            var vm = CreateViewModelWithValidPath();
            vm.GamePath = "/nonexistent/path";

            vm.IsDebugPanelInstalled().Should().BeFalse();
        }

        [Fact]
        public void IsDebugPanelInstalled_returns_true_when_dll_exists()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var debugPanelDir = Path.Combine(tempDir, "LobotomyCorp_Data", "BaseMods", "DebugPanel");
            Directory.CreateDirectory(debugPanelDir);
            File.WriteAllBytes(Path.Combine(debugPanelDir, "DebugPanel.dll"), [1, 2, 3]);
            try
            {
                var vm = CreateViewModelWithValidPath();
                vm.GamePath = tempDir;

                vm.IsDebugPanelInstalled().Should().BeTrue();
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }

        [Fact]
        public void IsDebugPanelDetected_is_true_when_DebugPanel_directory_exists()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var debugPanelDir = Path.Combine(tempDir, "LobotomyCorp_Data", "BaseMods", "DebugPanel");
            Directory.CreateDirectory(debugPanelDir);
            try
            {
                var vm = CreateViewModelWithValidPath();
                vm.GamePath = tempDir;

                vm.IsDebugPanelDetected.Should().BeTrue();
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }

        [Fact]
        public void IsDebugPanelDetected_is_true_when_legacy_directory_exists()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var legacyDir = Path.Combine(tempDir, "LobotomyCorp_Data", "BaseMods", "Hemocode.DebugPanel");
            Directory.CreateDirectory(legacyDir);
            try
            {
                var vm = CreateViewModelWithValidPath();
                vm.GamePath = tempDir;

                vm.IsDebugPanelDetected.Should().BeTrue();
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }

        [Fact]
        public void IsDebugPanelDetected_is_false_when_no_directories_exist()
        {
            var vm = CreateViewModelWithValidPath();
            vm.GamePath = "/nonexistent/path";

            vm.IsDebugPanelDetected.Should().BeFalse();
        }

        [Fact]
        public void RemoveDebugPanel_defaults_to_true()
        {
            var vm = CreateViewModel();

            vm.RemoveDebugPanel.Should().BeTrue();
        }

        [Fact]
        public void ShowDebugPanelCheckbox_is_false_when_state_is_Fresh()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var debugPanelDir = Path.Combine(tempDir, "LobotomyCorp_Data", "BaseMods", "DebugPanel");
            Directory.CreateDirectory(debugPanelDir);
            try
            {
                // Fresh state means nothing is installed yet — checkbox should be hidden
                var vm = CreateViewModelWithValidPath();
                vm.GamePath = tempDir;

                vm.IsDebugPanelDetected.Should().BeTrue();
                vm.ShowDebugPanelCheckbox.Should().BeFalse();
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }

        [Fact]
        public void ShowDebugPanelCheckbox_is_true_when_detected_and_not_Fresh()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var debugPanelDir = Path.Combine(tempDir, "LobotomyCorp_Data", "BaseMods", "DebugPanel");
            Directory.CreateDirectory(debugPanelDir);
            try
            {
                var vm = CreateViewModelWithValidPath();

                // Override state detector after VM creation so setting GamePath triggers non-Fresh state
                _mockStateDetector.Setup(d => d.Detect(It.IsAny<string>()))
                    .Returns(InstallationStateResult.WithState(InstallationState.Current, "1.0.0", "1.0.0"));
                vm.GamePath = tempDir;

                vm.ShowDebugPanelCheckbox.Should().BeTrue();
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }

        [Fact]
        public void UninstallDebugPanel_removes_all_directory_variants()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var baseModsDir = Path.Combine(tempDir, "LobotomyCorp_Data", "BaseMods");
            var dir1 = Path.Combine(baseModsDir, "DebugPanel");
            var dir2 = Path.Combine(baseModsDir, "LobotomyCorporationMods.DebugPanel");
            var dir3 = Path.Combine(baseModsDir, "Hemocode.DebugPanel");
            Directory.CreateDirectory(dir1);
            Directory.CreateDirectory(dir2);
            Directory.CreateDirectory(dir3);
            try
            {
                var vm = CreateViewModelWithValidPath();
                vm.GamePath = tempDir;

                var result = vm.UninstallDebugPanel();

                result.Should().BeEmpty();
                Directory.Exists(dir1).Should().BeFalse();
                Directory.Exists(dir2).Should().BeFalse();
                Directory.Exists(dir3).Should().BeFalse();
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                }
            }
        }

        [Fact]
        public void UninstallDebugPanel_returns_empty_when_no_directories_exist()
        {
            var vm = CreateViewModelWithValidPath();
            vm.GamePath = "/nonexistent/path";

            var result = vm.UninstallDebugPanel();

            result.Should().BeEmpty();
        }

        [Fact]
        public void UninstallDebugPanel_updates_IsDebugPanelDetected()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var debugPanelDir = Path.Combine(tempDir, "LobotomyCorp_Data", "BaseMods", "DebugPanel");
            Directory.CreateDirectory(debugPanelDir);
            try
            {
                var vm = CreateViewModelWithValidPath();
                vm.GamePath = tempDir;
                vm.IsDebugPanelDetected.Should().BeTrue();

                vm.UninstallDebugPanel();

                vm.IsDebugPanelDetected.Should().BeFalse();
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                }
            }
        }

        [Fact]
        public void Uninstall_passes_removeDebugPanel_to_service()
        {
            _mockStateDetector.Setup(d => d.Detect(It.IsAny<string>()))
                .Returns(InstallationStateResult.WithState(InstallationState.Current, "1.0.0", "1.0.0"));
            _mockUninstaller.Setup(u => u.Uninstall(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(UninstallResult.Success([], []));
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var debugPanelDir = Path.Combine(tempDir, "LobotomyCorp_Data", "BaseMods", "DebugPanel");
            Directory.CreateDirectory(debugPanelDir);
            try
            {
                var vm = CreateViewModelWithValidPath();
                vm.GamePath = tempDir;
                vm.RemoveDebugPanel = true;

                vm.UninstallCommand.Execute(null);

                _mockUninstaller.Verify(u => u.Uninstall(tempDir, false, true), Times.Once);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                }
            }
        }

        [Fact]
        public void Uninstall_does_not_pass_removeDebugPanel_when_unchecked()
        {
            _mockStateDetector.Setup(d => d.Detect(It.IsAny<string>()))
                .Returns(InstallationStateResult.WithState(InstallationState.Current, "1.0.0", "1.0.0"));
            _mockUninstaller.Setup(u => u.Uninstall(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(UninstallResult.Success([], []));
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var debugPanelDir = Path.Combine(tempDir, "LobotomyCorp_Data", "BaseMods", "DebugPanel");
            Directory.CreateDirectory(debugPanelDir);
            try
            {
                var vm = CreateViewModelWithValidPath();
                vm.GamePath = tempDir;
                vm.RemoveDebugPanel = false;

                vm.UninstallCommand.Execute(null);

                _mockUninstaller.Verify(u => u.Uninstall(tempDir, false, false), Times.Once);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                }
            }
        }

        [Fact]
        public void OpenGuide_invokes_open_guide_action_with_content()
        {
            _mockResourceProvider.Setup(r => r.ReadDocumentText("UsersGuide.md"))
                .Returns("# User Guide Content");
            var vm = CreateViewModel();
            string? capturedTitle = null;
            string? capturedContent = null;
            vm.SetOpenGuideAction((title, content, _, _) =>
            {
                capturedTitle = title;
                capturedContent = content;
            });

            vm.OpenGuide("User's Guide", "UsersGuide.md");

            capturedTitle.Should().Be("User's Guide");
            capturedContent.Should().StartWith("> **Note:**");
            capturedContent.Should().Contain("# User Guide Content");
        }

        [Fact]
        public void OpenGuide_passes_doc_file_path_when_game_path_is_set()
        {
            _mockResourceProvider.Setup(r => r.ReadDocumentText("UsersGuide.md"))
                .Returns("# Content");
            var vm = CreateViewModelWithValidPath();
            vm.GamePath = "/game";
            string? capturedDocPath = null;
            vm.SetOpenGuideAction((_, _, _, docPath) => capturedDocPath = docPath);

            vm.OpenGuide("Guide", "UsersGuide.md");

            capturedDocPath.Should().Contain(".harmony2forlmm");
            capturedDocPath.Should().Contain("UsersGuide.md");
        }

        [Fact]
        public void OpenGuide_does_nothing_when_document_not_found()
        {
            _mockResourceProvider.Setup(r => r.ReadDocumentText("Missing.md"))
                .Returns((string?)null);
            var vm = CreateViewModel();
            var invoked = false;
            vm.SetOpenGuideAction((_, _, _, _) => invoked = true);

            vm.OpenGuide("Missing", "Missing.md");

            invoked.Should().BeFalse();
        }
    }
}
