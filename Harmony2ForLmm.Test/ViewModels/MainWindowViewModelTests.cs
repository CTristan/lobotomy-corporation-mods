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
        public void Successful_uninstall_sets_IsActionCompleted_to_true()
        {
            _mockStateDetector.Setup(d => d.Detect(It.IsAny<string>()))
                .Returns(InstallationStateResult.WithState(InstallationState.Current, "1.0.0", "1.0.0"));
            _mockUninstaller.Setup(u => u.Uninstall(It.IsAny<string>(), It.IsAny<bool>()))
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
            _mockUninstaller.Setup(u => u.Uninstall(It.IsAny<string>(), It.IsAny<bool>()))
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
            _mockUninstaller.Setup(u => u.Uninstall(It.IsAny<string>(), It.IsAny<bool>()))
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
            _mockUninstaller.Setup(u => u.Uninstall(It.IsAny<string>(), It.IsAny<bool>()))
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
        public void OpenGuide_invokes_open_guide_action_with_content()
        {
            _mockResourceProvider.Setup(r => r.ReadDocumentText("UsersGuide.md"))
                .Returns("# User Guide Content");
            var vm = CreateViewModel();
            string? capturedTitle = null;
            string? capturedContent = null;
            vm.SetOpenGuideAction((title, content, _) =>
            {
                capturedTitle = title;
                capturedContent = content;
            });

            vm.OpenGuide("User's Guide", "UsersGuide.md");

            capturedTitle.Should().Be("User's Guide");
            capturedContent.Should().Be("# User Guide Content");
        }

        [Fact]
        public void OpenGuide_does_nothing_when_document_not_found()
        {
            _mockResourceProvider.Setup(r => r.ReadDocumentText("Missing.md"))
                .Returns((string?)null);
            var vm = CreateViewModel();
            var invoked = false;
            vm.SetOpenGuideAction((_, _, _) => invoked = true);

            vm.OpenGuide("Missing", "Missing.md");

            invoked.Should().BeFalse();
        }
    }
}
