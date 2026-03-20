// SPDX-License-Identifier: MIT

using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using LiveMarkdown.Avalonia;

namespace Harmony2ForLmm.Views
{
    /// <summary>
    /// Window displaying DebugPanel information and install/reinstall button.
    /// </summary>
    public sealed partial class DebugPanelWindow : Window
    {
        private readonly Func<string>? _installAction;
        private string? _pendingMarkdown;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugPanelWindow"/> class.
        /// </summary>
        public DebugPanelWindow()
        {
            InitializeComponent();
            Opened += OnOpened;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugPanelWindow"/> class.
        /// </summary>
        /// <param name="markdownContent">The markdown content to display.</param>
        /// <param name="isInstalled">Whether DebugPanel is already installed.</param>
        /// <param name="installAction">Function that installs DebugPanel. Returns empty string on success, error message on failure.</param>
        public DebugPanelWindow(string markdownContent, bool isInstalled, Func<string> installAction)
            : this()
        {
            _pendingMarkdown = markdownContent;
            _installAction = installAction;
            InstallButton.Content = isInstalled ? "Reinstall" : "Install";
        }

        private async void OnOpened(object? sender, EventArgs e)
        {
            if (_pendingMarkdown == null)
            {
                return;
            }

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

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
