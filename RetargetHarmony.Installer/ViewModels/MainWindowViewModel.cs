// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Input;
using RetargetHarmony.Installer.Interfaces;

namespace RetargetHarmony.Installer.ViewModels
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

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        public MainWindowViewModel(
            IGamePathFinder gamePathFinder,
            IGameDirectoryValidator validator,
            IInstallerService installerService,
            IUninstallerService uninstallerService,
            IBaseModsAnalyzer baseModsAnalyzer)
        {
            _gamePathFinder = gamePathFinder;
            _validator = validator;
            _installerService = installerService;
            _uninstallerService = uninstallerService;
            _baseModsAnalyzer = baseModsAnalyzer;

            InstallCommand = new RelayCommand(ExecuteInstall, () => IsPathValid && !IsWorking);
            UninstallCommand = new RelayCommand(ExecuteUninstall, () => IsPathValid && !IsWorking);
            AutoDetectCommand = new RelayCommand(ExecuteAutoDetect, () => !IsWorking);

            ExecuteAutoDetect();
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
            InstallCommand = new RelayCommand(() => { });
            UninstallCommand = new RelayCommand(() => { });
            AutoDetectCommand = new RelayCommand(() => { });
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
        public bool IsPathValid { get; private set => SetAndNotify(ref field, value); }

        /// <summary>
        /// Gets a value indicating whether BepInEx is currently installed.
        /// </summary>
        public bool IsBepInExInstalled { get; private set => SetAndNotify(ref field, value); }

        /// <summary>
        /// Gets a value indicating whether RetargetHarmony is currently installed.
        /// </summary>
        public bool IsRetargetHarmonyInstalled { get; private set => SetAndNotify(ref field, value); }

        /// <summary>
        /// Gets a value indicating whether an operation is in progress.
        /// </summary>
        public bool IsWorking { get; private set => SetAndNotify(ref field, value); }

        /// <summary>
        /// Gets the result details from the last operation.
        /// </summary>
        public string ResultDetails { get; private set => SetAndNotify(ref field, value); } = string.Empty;

        /// <summary>
        /// Gets the list of flagged mods from the last analysis.
        /// </summary>
        public IReadOnlyList<FlaggedMod> FlaggedMods { get; private set => SetAndNotify(ref field, value); } = [];

        /// <summary>
        /// Gets the command to install BepInEx and RetargetHarmony.
        /// </summary>
        public ICommand InstallCommand { get; }

        /// <summary>
        /// Gets the command to uninstall BepInEx and RetargetHarmony.
        /// </summary>
        public ICommand UninstallCommand { get; }

        /// <summary>
        /// Gets the command to auto-detect the game path.
        /// </summary>
        public ICommand AutoDetectCommand { get; }

        private void ValidatePath()
        {
            var result = _validator.Validate(GamePath);
            IsPathValid = result.IsValid;

            if (result.IsValid)
            {
                ValidationMessage = "Valid Lobotomy Corporation installation detected.";
                IsBepInExInstalled = _installerService.IsBepInExInstalled(GamePath);
                IsRetargetHarmonyInstalled = _installerService.IsRetargetHarmonyInstalled(GamePath);
                FlaggedMods = _baseModsAnalyzer.Analyze(GamePath);
            }
            else
            {
                ValidationMessage = result.ErrorMessage ?? "Invalid path.";
                IsBepInExInstalled = false;
                IsRetargetHarmonyInstalled = false;
                FlaggedMods = [];
            }
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
                }
                else
                {
                    StatusMessage = "Installation failed.";
                    ResultDetails = result.ErrorMessage ?? "Unknown error.";
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                StatusMessage = "Installation failed with an unexpected error.";
                ResultDetails = ex.Message;
            }
            finally
            {
                IsWorking = false;
            }
        }

        private void ExecuteUninstall()
        {
            IsWorking = true;
            StatusMessage = "Uninstalling BepInEx 5 and RetargetHarmony...";
            ResultDetails = string.Empty;

            try
            {
                var removeFlaggedMods = FlaggedMods.Count > 0;
                var result = _uninstallerService.Uninstall(GamePath, removeFlaggedMods);

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
                }
                else
                {
                    StatusMessage = "Uninstallation failed.";
                    ResultDetails = result.ErrorMessage ?? "Unknown error.";
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                StatusMessage = "Uninstallation failed with an unexpected error.";
                ResultDetails = ex.Message;
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
