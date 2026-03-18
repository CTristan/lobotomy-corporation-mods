// SPDX-License-Identifier: MIT

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Harmony2ForLmm.Services;
using Harmony2ForLmm.ViewModels;
using Harmony2ForLmm.Views;

namespace Harmony2ForLmm
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

                var bundleVersion = typeof(App).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";
                var manifestService = new ManifestService(bundleVersion);
                var gamePathFinder = new GamePathFinder();
                var validator = new GameDirectoryValidator();
                var installer = new InstallerService(resourcesPath, manifestService);
                var baseModsAnalyzer = new BaseModsAnalyzer();
                var uninstaller = new UninstallerService(baseModsAnalyzer, manifestService);
                var stateDetector = new InstallationStateDetector(manifestService, bundleVersion);

                var viewModel = new MainWindowViewModel(
                    gamePathFinder,
                    validator,
                    installer,
                    uninstaller,
                    baseModsAnalyzer,
                    stateDetector);

                desktop.MainWindow = new MainWindow
                {
                    DataContext = viewModel,
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
