// SPDX-License-Identifier: MIT

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RetargetHarmony.Installer.Services;
using RetargetHarmony.Installer.ViewModels;
using RetargetHarmony.Installer.Views;

namespace RetargetHarmony.Installer
{
    /// <summary>
    /// Avalonia application class.
    /// </summary>
    public sealed class App : Application
    {
        /// <inheritdoc />
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <inheritdoc />
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var resourcesPath = System.IO.Path.Combine(
                    System.AppContext.BaseDirectory,
                    "Resources");

                var gamePathFinder = new GamePathFinder();
                var validator = new GameDirectoryValidator();
                var installer = new InstallerService(resourcesPath);
                var baseModsAnalyzer = new BaseModsAnalyzer();
                var uninstaller = new UninstallerService(baseModsAnalyzer);

                var viewModel = new MainWindowViewModel(
                    gamePathFinder,
                    validator,
                    installer,
                    uninstaller,
                    baseModsAnalyzer);

                desktop.MainWindow = new MainWindow
                {
                    DataContext = viewModel,
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
