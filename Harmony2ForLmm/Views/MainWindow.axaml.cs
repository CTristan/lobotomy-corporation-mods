// SPDX-License-Identifier: MIT

using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Harmony2ForLmm.ViewModels;
using Harmony2ForLmm.Views.Pages;

namespace Harmony2ForLmm.Views
{
    /// <summary>
    /// Main window code-behind for folder browser interaction.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private SecondaryWindowViewModel? _activeSecondaryViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                if (DataContext is MainWindowViewModel viewModel)
                {
                    viewModel.SetCloseAction(Close);
                    viewModel.SetOpenGuideAction(NavigateToGuide);
                    viewModel.SetOpenGuidesAction(() => OpenSecondaryWithMenu(viewModel, isGuides: true));
                    viewModel.SetOpenTroubleshootingAction(() => OpenSecondaryWithMenu(viewModel, isGuides: false));
                }
            };
        }

        private async void OpenSecondaryWithMenu(MainWindowViewModel viewModel, bool isGuides)
        {
            var secondaryViewModel = new SecondaryWindowViewModel();
            _activeSecondaryViewModel = secondaryViewModel;

            // Temporarily re-point the guide action to navigate within the secondary window
            var originalGuideAction = viewModel.OpenGuideAction;
            viewModel.SetOpenGuideAction(NavigateToGuideInSecondary);

            UserControl menuPage;
            string title;

            if (isGuides)
            {
                title = "Guides";
                menuPage = new GuidesMenuPage(
                    () => viewModel.OpenGuide("User's Guide", "UsersGuide.md"),
                    () => viewModel.OpenGuide("Modder's Guide", "ModdersGuide.md"));
            }
            else
            {
                title = "Troubleshooting";
                menuPage = new TroubleshootingMenuPage(
                    () =>
                    {
                        var isInstalled = viewModel.IsDebugPanelInstalled();
                        var markdownContent = "DebugPanel";
                        var docContent = viewModel.ReadDebugPanelDoc();
                        if (docContent != null)
                        {
                            markdownContent = docContent;
                        }

                        var debugPage = new DebugPanelContentPage(markdownContent, isInstalled, viewModel.InstallDebugPanel, viewModel.UninstallDebugPanel);
                        secondaryViewModel.NavigateTo(debugPage, "DebugPanel", 700, 600);
                    },
                    () => viewModel.OpenGuide("User's Guide", "UsersGuide.md"));
            }

            // Create window first so it subscribes to PropertyChanged before NavigateTo fires
            var secondaryWindow = new SecondaryWindow(secondaryViewModel);
            secondaryViewModel.NavigateTo(menuPage, title, 400, 250);

            await secondaryWindow.ShowDialog(this).ConfigureAwait(true);

            // Restore the original guide action when the secondary window closes
            viewModel.SetOpenGuideAction(originalGuideAction ?? ((_, _, _, _) => { }));
            _activeSecondaryViewModel = null;
        }

        private void NavigateToGuideInSecondary(string title, string content, System.Func<Stream?>? openDemoModZip, string? docFilePath)
        {
            if (_activeSecondaryViewModel == null)
            {
                return;
            }

            GuideContentPage guidePage;
            if (openDemoModZip != null)
            {
                guidePage = new GuideContentPage(content, openDemoModZip, _activeSecondaryViewModel.NavigateTo, docFilePath);
            }
            else
            {
                guidePage = GuideContentPage.WithDocPath(content, docFilePath);
            }

            _activeSecondaryViewModel.NavigateTo(guidePage, title, 800, 700);
        }

        private void NavigateToGuide(string title, string content, System.Func<Stream?>? openDemoModZip, string? docFilePath)
        {
            // Fallback when no secondary window is active — should not normally be reached
            NavigateToGuideInSecondary(title, content, openDemoModZip, docFilePath);
        }

        private async void BrowseButton_Click(object? sender, RoutedEventArgs e)
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Lobotomy Corporation installation directory",
                AllowMultiple = false,
            }).ConfigureAwait(true);

            if (folders.Count > 0 && DataContext is MainWindowViewModel viewModel)
            {
                var path = folders[0].Path.LocalPath;
                viewModel.GamePath = path;
            }
        }
    }
}
