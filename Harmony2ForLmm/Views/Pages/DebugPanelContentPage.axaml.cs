// SPDX-License-Identifier: MIT

using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LiveMarkdown.Avalonia;

namespace Harmony2ForLmm.Views.Pages
{
    /// <summary>
    /// Page displaying DebugPanel information and install/reinstall button.
    /// </summary>
    public sealed partial class DebugPanelContentPage : UserControl, IAsyncLoadablePage
    {
        private readonly Func<string>? _installAction;
        private readonly Func<string>? _uninstallAction;
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
        public DebugPanelContentPage(string markdownContent, bool isInstalled, Func<string> installAction, Func<string>? uninstallAction = null)
            : this()
        {
            _pendingMarkdown = markdownContent;
            _installAction = installAction;
            _uninstallAction = uninstallAction;
            InstallButton.Content = isInstalled ? "Reinstall" : "Install";

            if (isInstalled && uninstallAction != null)
            {
                UninstallButton.Content = "Uninstall";
                UninstallButton.IsVisible = true;
            }
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

            var builder = new ObservableStringBuilder();
            _ = builder.Append(content);
            MarkdownViewer.MarkdownBuilder = builder;

            // Wait for the render pass to complete before swapping visibility,
            // so the content is painted before the loading indicator disappears.
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render);

            LoadingPanel.IsVisible = false;
            ContentScrollViewer.IsVisible = true;
        }

        private void UninstallButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_uninstallAction == null)
            {
                return;
            }

            var error = _uninstallAction();
            if (string.IsNullOrEmpty(error))
            {
                StatusText.Text = "DebugPanel uninstalled successfully.";
                StatusText.Foreground = Avalonia.Media.Brushes.LimeGreen;
                UninstallButton.IsVisible = false;
                InstallButton.Content = "Install";
            }
            else
            {
                StatusText.Text = error;
                StatusText.Foreground = Avalonia.Media.Brushes.OrangeRed;
            }
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

                if (_uninstallAction != null)
                {
                    UninstallButton.Content = "Uninstall";
                    UninstallButton.IsVisible = true;
                }
            }
            else
            {
                StatusText.Text = error;
                StatusText.Foreground = Avalonia.Media.Brushes.OrangeRed;
            }
        }
    }
}
