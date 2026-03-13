// SPDX-License-Identifier: MIT

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using RetargetHarmony.Installer.ViewModels;

namespace RetargetHarmony.Installer.Views
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
