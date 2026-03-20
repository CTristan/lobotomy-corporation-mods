// SPDX-License-Identifier: MIT

using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using LiveMarkdown.Avalonia;

namespace Harmony2ForLmm.Views.Pages
{
    /// <summary>
    /// Page displaying DebugPanel information and install/reinstall button.
    /// </summary>
    public sealed partial class DebugPanelContentPage : UserControl, IAsyncLoadablePage
    {
        private readonly Func<string>? _installAction;
        private string? _pendingMarkdown;
        private bool _loaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugPanelContentPage"/> class.
        /// </summary>
        public DebugPanelContentPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugPanelContentPage"/> class.
        /// </summary>
        public DebugPanelContentPage(string markdownContent, bool isInstalled, Func<string> installAction)
            : this()
        {
            _pendingMarkdown = markdownContent;
            _installAction = installAction;
            InstallButton.Content = isInstalled ? "Reinstall" : "Install";
        }

        /// <inheritdoc />
        public async Task LoadContentAsync()
        {
            if (_loaded || _pendingMarkdown == null)
            {
                return;
            }

            _loaded = true;
            var content = _pendingMarkdown;
            _pendingMarkdown = null;

            await Task.Delay(100).ConfigureAwait(true);

            var builder = new ObservableStringBuilder();
            _ = builder.Append(content);
            MarkdownViewer.MarkdownBuilder = builder;
            LoadingPanel.IsVisible = false;
            ContentScrollViewer.IsVisible = true;
        }

        private void InstallButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_installAction == null)
            {
                return;
            }

            var error = _installAction();
            if (string.IsNullOrEmpty(error))
            {
                StatusText.Text = "DebugPanel installed successfully.";
                StatusText.Foreground = Avalonia.Media.Brushes.LimeGreen;
                InstallButton.Content = "Reinstall";
            }
            else
            {
                StatusText.Text = error;
                StatusText.Foreground = Avalonia.Media.Brushes.OrangeRed;
            }
        }
    }
}
