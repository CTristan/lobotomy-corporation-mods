// SPDX-License-Identifier: MIT

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Harmony2ForLmm.ViewModels;

namespace Harmony2ForLmm.Views
{
    /// <summary>
    /// Main window code-behind for folder browser interaction.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
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
                    viewModel.SetOpenGuideAction((title, content, openDemoModZip) =>
                    {
                        var guideWindow = openDemoModZip != null
                            ? new GuideWindow(title, content, openDemoModZip)
                            : new GuideWindow(title, content);
                        _ = guideWindow.ShowDialog(this);
                    });
                    viewModel.SetOpenGuidesAction(() =>
                    {
                        var guidesWindow = new GuidesWindow(
                            () => viewModel.OpenGuide("User's Guide", "UsersGuide.md"),
                            () => viewModel.OpenGuide("Modder's Guide", "ModdersGuide.md"));
                        _ = guidesWindow.ShowDialog(this);
                    });
                    viewModel.SetOpenTroubleshootingAction(() =>
                    {
                        var troubleshootingWindow = new TroubleshootingWindow(
                            () =>
                            {
                                var content = viewModel.IsDebugPanelInstalled();
                                var markdownContent = "DebugPanel";
                                var docContent = viewModel.ReadDebugPanelDoc();
                                if (docContent != null)
                                {
                                    markdownContent = docContent;
                                }

                                var debugPanelWindow = new DebugPanelWindow(
                                    markdownContent,
                                    content,
                                    viewModel.InstallDebugPanel);
                                _ = debugPanelWindow.ShowDialog(this);
                            },
                            () => viewModel.OpenGuide("User's Guide", "UsersGuide.md"));
                        _ = troubleshootingWindow.ShowDialog(this);
                    });
                }
            };
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
