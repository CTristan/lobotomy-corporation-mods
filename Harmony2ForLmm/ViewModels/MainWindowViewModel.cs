// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Input;
using Harmony2ForLmm.Interfaces;
using Harmony2ForLmm.Models;
using Harmony2ForLmm.Services;

namespace Harmony2ForLmm.ViewModels
{
    /// <summary>
    /// View model for the main installer window.
    /// </summary>
    public sealed class MainWindowViewModel : ViewModelBase
    {
        private readonly IGamePathFinder _gamePathFinder;
        private readonly IGameDirectoryValidator _validator;
        private readonly IInstallerService _installerService;
        private readonly IUninstallerService _uninstallerService;
        private readonly IBaseModsAnalyzer _baseModsAnalyzer;
        private readonly IInstallationStateDetector _stateDetector;
        private readonly IResourceProvider _resourceProvider;
        private bool WasInstalledAtStartup { get; }
        private Action? _closeAction;
        private Action? _openGuidesAction;
        private Action? _openTroubleshootingAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        public MainWindowViewModel(
            IGamePathFinder gamePathFinder,
            IGameDirectoryValidator validator,
            IInstallerService installerService,
            IUninstallerService uninstallerService,
            IBaseModsAnalyzer baseModsAnalyzer,
            IInstallationStateDetector stateDetector,
            IResourceProvider resourceProvider)
        {
            _gamePathFinder = gamePathFinder;
            _validator = validator;
            _installerService = installerService;
            _uninstallerService = uninstallerService;
            _baseModsAnalyzer = baseModsAnalyzer;
            _stateDetector = stateDetector;
            _resourceProvider = resourceProvider;

            PrimaryActionCommand = new RelayCommand(ExecuteInstall, () => IsPathValid && !IsWorking);
            UninstallCommand = new RelayCommand(ExecuteUninstall, () => IsPathValid && !IsWorking && CurrentState != InstallationState.Fresh);
            AutoDetectCommand = new RelayCommand(ExecuteAutoDetect, () => !IsWorking && !IsActionCompleted);
            CloseCommand = new RelayCommand(() => _closeAction?.Invoke());
            GuidesCommand = new RelayCommand(() => _openGuidesAction?.Invoke());
            TroubleshootingCommand = new RelayCommand(() => _openTroubleshootingAction?.Invoke());

            ExecuteAutoDetect();
            WasInstalledAtStartup = CurrentState != InstallationState.Fresh;
        }

        /// <summary>
        /// Parameterless constructor for design-time support.
        /// </summary>
        public MainWindowViewModel()
        {
            _gamePathFinder = null!;
            _validator = null!;
            _installerService = null!;
            _uninstallerService = null!;
            _baseModsAnalyzer = null!;
            _stateDetector = null!;
            _resourceProvider = null!;
            PrimaryActionCommand = new RelayCommand(() => { });
            UninstallCommand = new RelayCommand(() => { });
            AutoDetectCommand = new RelayCommand(() => { });
            CloseCommand = new RelayCommand(() => { });
            GuidesCommand = new RelayCommand(() => { });
            TroubleshootingCommand = new RelayCommand(() => { });
        }

        /// <summary>
        /// Gets or sets the game installation path.
        /// </summary>
        public string GamePath
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged();
                    ValidatePath();
                }
            }
        } = string.Empty;

        /// <summary>
        /// Gets the window title including the version number.
        /// </summary>
        public static string WindowTitle { get; } =
            $"Harmony 2 for LMM v{typeof(MainWindowViewModel).Assembly.GetName().Version?.ToString(3) ?? "0.0.0"}";

        /// <summary>
        /// Gets the current status message.
        /// </summary>
        public string StatusMessage { get; private set => SetAndNotify(ref field, value); } = string.Empty;

        /// <summary>
        /// Gets the validation message for the current path.
        /// </summary>
        public string ValidationMessage { get; private set => SetAndNotify(ref field, value); } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the current path is valid.
        /// </summary>
        public bool IsPathValid
        {
            get;
            private set
            {
                if (SetAndNotify(ref field, value))
                {
                    NotifyAllCommands();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether an operation is in progress.
        /// </summary>
        public bool IsWorking
        {
            get;
            private set
            {
                if (SetAndNotify(ref field, value))
                {
                    OnPropertyChanged(nameof(IsPathEditable));
                    NotifyAllCommands();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether an action has completed successfully.
        /// When true, action buttons and path controls are disabled.
        /// </summary>
        public bool IsActionCompleted
        {
            get;
            private set
            {
                if (SetAndNotify(ref field, value))
                {
                    OnPropertyChanged(nameof(ShowPrimaryAction));
                    OnPropertyChanged(nameof(ShowUninstallAction));
                    OnPropertyChanged(nameof(ShowDebugPanelCheckbox));
                    OnPropertyChanged(nameof(IsPathEditable));
                    NotifyAllCommands();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the last action failed.
        /// Used to style the status message in red.
        /// </summary>
        public bool IsLastActionFailed { get; private set => SetAndNotify(ref field, value); }

        /// <summary>
        /// Gets a value indicating whether the path input controls should be editable.
        /// </summary>
        public bool IsPathEditable => !IsWorking && !IsActionCompleted;

        /// <summary>
        /// Gets the result details from the last operation.
        /// </summary>
        public string ResultDetails { get; private set => SetAndNotify(ref field, value); } = string.Empty;

        /// <summary>
        /// Gets the list of flagged mods from the last analysis.
        /// </summary>
        public IReadOnlyList<FlaggedMod> FlaggedMods { get; private set => SetAndNotify(ref field, value); } = [];

        /// <summary>
        /// Gets the current installation state.
        /// </summary>
        public InstallationState CurrentState
        {
            get;
            private set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PrimaryActionLabel));
                    OnPropertyChanged(nameof(ShowPrimaryAction));
                    OnPropertyChanged(nameof(ShowUninstallAction));
                    OnPropertyChanged(nameof(ShowDebugPanelCheckbox));
                    OnPropertyChanged(nameof(ShowVersionWarning));
                    OnPropertyChanged(nameof(VersionInfoText));
                    NotifyAllCommands();
                }
            }
        }

        /// <summary>
        /// Gets the installed version string, if available.
        /// </summary>
        public string InstalledVersion { get; private set => SetAndNotify(ref field, value); } = string.Empty;

        /// <summary>
        /// Gets the installer's bundle version string.
        /// </summary>
        public string InstallerVersion { get; private set => SetAndNotify(ref field, value); } = string.Empty;

        /// <summary>
        /// Gets the list of missing files (for Corrupted state).
        /// </summary>
        public IReadOnlyList<string> MissingFiles { get; private set => SetAndNotify(ref field, value); } = [];

        /// <summary>
        /// Gets a value indicating whether any DebugPanel directory variant is detected.
        /// </summary>
        public bool IsDebugPanelDetected
        {
            get;
            private set
            {
                if (SetAndNotify(ref field, value))
                {
                    OnPropertyChanged(nameof(ShowDebugPanelCheckbox));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether DebugPanel should be removed during uninstall.
        /// </summary>
        public bool RemoveDebugPanel { get; set => SetAndNotify(ref field, value); } = true;

        /// <summary>
        /// Gets the primary action button label based on current state.
        /// </summary>
        public string PrimaryActionLabel => CurrentState switch
        {
            InstallationState.Fresh => "Install",
            InstallationState.Current => "Reinstall",
            InstallationState.Outdated => "Upgrade",
            InstallationState.Newer => "Downgrade",
            InstallationState.Corrupted => "Repair",
            _ => "Install",
        };

        /// <summary>
        /// Gets a value indicating whether the uninstall button should be visible.
        /// </summary>
        public bool ShowUninstallAction => !IsActionCompleted && CurrentState != InstallationState.Fresh;

        /// <summary>
        /// Gets a value indicating whether the DebugPanel removal checkbox should be visible.
        /// Only shown when DebugPanel is detected and the uninstall action is available.
        /// </summary>
        public bool ShowDebugPanelCheckbox => IsDebugPanelDetected && ShowUninstallAction;

        /// <summary>
        /// Gets a value indicating whether the version warning banner should be visible.
        /// </summary>
        public bool ShowVersionWarning => CurrentState == InstallationState.Newer;

        /// <summary>
        /// Gets a value indicating whether the primary action button should be visible.
        /// Hidden when current version matches and the app was not already installed at startup.
        /// </summary>
        public bool ShowPrimaryAction => !IsActionCompleted && (CurrentState != InstallationState.Current || WasInstalledAtStartup);

        /// <summary>
        /// Gets a formatted version info string.
        /// </summary>
        public string VersionInfoText =>
            string.IsNullOrEmpty(InstalledVersion)
                ? string.Empty
                : $"Installed: {InstalledVersion}  |  Installer: {InstallerVersion}";

        /// <summary>
        /// Gets the primary action command (Install/Reinstall/Upgrade/Downgrade/Repair).
        /// </summary>
        public RelayCommand PrimaryActionCommand { get; }

        /// <summary>
        /// Gets the command to uninstall.
        /// </summary>
        public RelayCommand UninstallCommand { get; }

        /// <summary>
        /// Gets the command to auto-detect the game path.
        /// </summary>
        public RelayCommand AutoDetectCommand { get; }

        /// <summary>
        /// Gets the command to close the window.
        /// </summary>
        public ICommand CloseCommand { get; }

        /// <summary>
        /// Gets the command to open the Guides window.
        /// </summary>
        public ICommand GuidesCommand { get; }

        /// <summary>
        /// Gets the command to open the Troubleshooting window.
        /// </summary>
        public ICommand TroubleshootingCommand { get; }

        /// <summary>
        /// Sets the action to invoke when the close command is executed.
        /// </summary>
        public void SetCloseAction(Action closeAction)
        {
            _closeAction = closeAction;
        }

        /// <summary>
        /// Gets the current open guide action, or null if not set.
        /// </summary>
        public Action<string, string, Func<Stream?>?, string?>? OpenGuideAction { get; private set; }

        /// <summary>
        /// Sets the action to invoke when a guide window should be opened.
        /// </summary>
        /// <param name="openGuideAction">Action accepting (title, markdownContent, openDemoModZip, docFilePath).</param>
        public void SetOpenGuideAction(Action<string, string, Func<Stream?>?, string?> openGuideAction)
        {
            OpenGuideAction = openGuideAction;
        }

        /// <summary>
        /// Sets the action to invoke when the Guides window should be opened.
        /// </summary>
        public void SetOpenGuidesAction(Action openGuidesAction)
        {
            _openGuidesAction = openGuidesAction;
        }

        /// <summary>
        /// Sets the action to invoke when the Troubleshooting window should be opened.
        /// </summary>
        public void SetOpenTroubleshootingAction(Action openTroubleshootingAction)
        {
            _openTroubleshootingAction = openTroubleshootingAction;
        }

        /// <summary>
        /// Installs DebugPanel to the game's BaseMods directory.
        /// </summary>
        /// <returns>Empty string on success, error message on failure.</returns>
        public string InstallDebugPanel()
        {
            try
            {
                // Remove legacy directory variants before installing
                var cleanupError = UninstallDebugPanel();
                if (!string.IsNullOrEmpty(cleanupError))
                {
                    return cleanupError;
                }

                var baseModsPath = Path.Combine(GamePath, "LobotomyCorp_Data", "BaseMods");
                var filesWritten = new List<string>();
                _resourceProvider.ExtractDebugPanelTo(baseModsPath, filesWritten);

                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Uninstalls DebugPanel by removing all known directory variants.
        /// </summary>
        /// <returns>Empty string on success, error message on failure.</returns>
        public string UninstallDebugPanel()
        {
            try
            {
                var baseModsPath = Path.Combine(GamePath, "LobotomyCorp_Data", "BaseMods");
                foreach (var dirName in UninstallerService.DebugPanelDirectoryNames)
                {
                    var dirPath = Path.Combine(baseModsPath, dirName);
                    if (Directory.Exists(dirPath))
                    {
                        Directory.Delete(dirPath, recursive: true);
                    }
                }

                IsDebugPanelDetected = DetectDebugPanel();

                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Checks whether DebugPanel is already installed.
        /// </summary>
        public bool IsDebugPanelInstalled()
        {
            var dllPath = Path.Combine(GamePath, "LobotomyCorp_Data", "BaseMods", "DebugPanel", "DebugPanel.dll");

            return File.Exists(dllPath);
        }

        /// <summary>
        /// Reads the DebugPanel documentation markdown content.
        /// </summary>
        /// <returns>The markdown content, or null if not found.</returns>
        public string? ReadDebugPanelDoc()
        {
            return _resourceProvider.ReadDocumentText("DebugPanel.md");
        }

        /// <summary>
        /// Opens a guide document in a new window.
        /// </summary>
        public void OpenGuide(string title, string fileName)
        {
            var content = _resourceProvider.ReadDocumentText(fileName);
            if (content == null)
            {
                return;
            }

            // Check if the DemoMod resource exists by probing and disposing the stream
            Func<Stream?>? openDemoModZip = null;
            using (var probe = _resourceProvider.OpenDemoModZip())
            {
                if (probe != null)
                {
                    openDemoModZip = _resourceProvider.OpenDemoModZip;
                }
            }

            // Compute the on-disk doc path and prepend an availability note
            string? docFilePath = null;
            if (!string.IsNullOrEmpty(GamePath))
            {
                docFilePath = Path.Combine(GamePath, IManifestService.ManifestDirectory, "docs", fileName);
            }

            var note = $"> **Note:** After installation, this document is also available at `.harmony2forlmm/docs/{fileName}` in your game directory.\n\n";
            content = note + content;

            OpenGuideAction?.Invoke(title, content, openDemoModZip, docFilePath);
        }

        private void NotifyAllCommands()
        {
            PrimaryActionCommand.NotifyCanExecuteChanged();
            UninstallCommand.NotifyCanExecuteChanged();
            AutoDetectCommand.NotifyCanExecuteChanged();
        }

        private void ValidatePath()
        {
            var result = _validator.Validate(GamePath);
            IsPathValid = result.IsValid;

            if (result.IsValid)
            {
                ValidationMessage = "Valid Lobotomy Corporation installation detected.";
                FlaggedMods = _baseModsAnalyzer.Analyze(GamePath);
                IsDebugPanelDetected = DetectDebugPanel();
                DetectState();
            }
            else
            {
                ValidationMessage = result.ErrorMessage ?? "Invalid path.";
                FlaggedMods = [];
                IsDebugPanelDetected = false;
                CurrentState = InstallationState.Fresh;
                InstalledVersion = string.Empty;
                InstallerVersion = string.Empty;
                MissingFiles = [];
            }
        }

        private bool DetectDebugPanel()
        {
            var baseModsPath = Path.Combine(GamePath, "LobotomyCorp_Data", "BaseMods");
            foreach (var dirName in UninstallerService.DebugPanelDirectoryNames)
            {
                if (Directory.Exists(Path.Combine(baseModsPath, dirName)))
                {
                    return true;
                }
            }

            return false;
        }

        private void DetectState()
        {
            var stateResult = _stateDetector.Detect(GamePath);
            CurrentState = stateResult.State;
            InstalledVersion = stateResult.InstalledVersion ?? string.Empty;
            InstallerVersion = stateResult.InstallerVersion;
            MissingFiles = stateResult.MissingFiles;
        }

        private void ExecuteAutoDetect()
        {
            StatusMessage = "Searching for Lobotomy Corporation installation...";
            var path = _gamePathFinder.FindGamePath();

            if (path != null)
            {
                GamePath = path;
                StatusMessage = "Game installation auto-detected.";
            }
            else
            {
                StatusMessage = "Could not auto-detect game installation. Please browse manually.";
            }
        }

        private void ExecuteInstall()
        {
            IsWorking = true;
            IsLastActionFailed = false;
            StatusMessage = "Installing BepInEx 5 and RetargetHarmony...";
            ResultDetails = string.Empty;

            try
            {
                var result = _installerService.Install(GamePath);

                if (result.IsSuccess)
                {
                    StatusMessage = "Installation completed successfully.";
                    ResultDetails = FormatFileList("Files installed:", result.FilesWritten);
                    ValidatePath();
                    IsActionCompleted = true;
                }
                else
                {
                    StatusMessage = "Installation failed.";
                    ResultDetails = result.ErrorMessage ?? "Unknown error.";
                    IsLastActionFailed = true;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Installation failed with an unexpected error.";
                ResultDetails = ex.Message;
                IsLastActionFailed = true;
            }
            finally
            {
                IsWorking = false;
            }
        }

        private void ExecuteUninstall()
        {
            IsWorking = true;
            IsLastActionFailed = false;
            StatusMessage = "Uninstalling BepInEx 5 and RetargetHarmony...";
            ResultDetails = string.Empty;

            try
            {
                var removeFlaggedMods = FlaggedMods.Count > 0;
                var result = _uninstallerService.Uninstall(GamePath, removeFlaggedMods, RemoveDebugPanel && IsDebugPanelDetected);

                if (result.IsSuccess)
                {
                    StatusMessage = "Uninstallation completed successfully.";
                    var sb = new StringBuilder();

                    if (result.FilesRemoved.Count > 0)
                    {
                        _ = sb.AppendLine(FormatFileList("Files removed:", result.FilesRemoved));
                    }

                    if (result.DirectoriesRemoved.Count > 0)
                    {
                        _ = sb.AppendLine(FormatFileList("Directories removed:", result.DirectoriesRemoved));
                    }

                    ResultDetails = sb.ToString();
                    ValidatePath();
                    IsActionCompleted = true;
                }
                else
                {
                    StatusMessage = "Uninstallation failed.";
                    ResultDetails = result.ErrorMessage ?? "Unknown error.";
                    IsLastActionFailed = true;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Uninstallation failed with an unexpected error.";
                ResultDetails = ex.Message;
                IsLastActionFailed = true;
            }
            finally
            {
                IsWorking = false;
            }
        }

        private static string FormatFileList(string header, IReadOnlyList<string> files)
        {
            var sb = new StringBuilder();
            _ = sb.AppendLine(header);
            foreach (var file in files)
            {
                _ = sb.AppendLine($"  {file}");
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Simple relay command implementation.
    /// </summary>
    public sealed class RelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
    {
        /// <inheritdoc />
        public event EventHandler? CanExecuteChanged;

        /// <inheritdoc />
        public bool CanExecute(object? parameter)
        {
            return canExecute?.Invoke() ?? true;
        }

        /// <inheritdoc />
        public void Execute(object? parameter)
        {
            execute();
        }

        /// <summary>
        /// Notifies that the <see cref="CanExecuteChanged"/> state has changed.
        /// </summary>
        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
