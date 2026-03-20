// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LiveMarkdown.Avalonia;

namespace Harmony2ForLmm.Views
{
    /// <summary>
    /// Window for displaying markdown guide content.
    /// </summary>
    public sealed partial class GuideWindow : Window
    {
        private readonly Func<Stream?>? _openZipStream;
        private readonly string? _extractedPath;
        private string? _pendingMarkdown;

        /// <summary>
        /// Initializes a new instance of the <see cref="GuideWindow"/> class.
        /// </summary>
        public GuideWindow()
        {
            InitializeComponent();
            Opened += OnOpened;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GuideWindow"/> class with content.
        /// </summary>
        /// <param name="title">The window title.</param>
        /// <param name="markdownContent">The markdown content to display.</param>
        public GuideWindow(string title, string markdownContent)
            : this()
        {
            Title = title;
            _pendingMarkdown = markdownContent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GuideWindow"/> class with content
        /// and an associated sample project zip. Shows "View Sample Project" button.
        /// </summary>
        /// <param name="title">The window title.</param>
        /// <param name="markdownContent">The markdown content to display.</param>
        /// <param name="openZipStream">Function that opens a stream to the sample project zip.</param>
        public GuideWindow(string title, string markdownContent, Func<Stream?> openZipStream)
            : this(title, markdownContent)
        {
            _openZipStream = openZipStream;
            SampleProjectButton.IsVisible = true;
        }

        private GuideWindow(string title, string markdownContent, Func<Stream?>? openZipStream, string extractedPath)
            : this(title, markdownContent)
        {
            _openZipStream = openZipStream;
            _extractedPath = extractedPath;
            OpenFolderButton.IsVisible = true;
        }

        private void OnOpened(object? sender, EventArgs e)
        {
            if (_pendingMarkdown == null)
            {
                return;
            }

            var content = _pendingMarkdown;
            _pendingMarkdown = null;

            _ = Dispatcher.UIThread.InvokeAsync(() =>
            {
                var builder = new ObservableStringBuilder();
                _ = builder.Append(content);
                MarkdownViewer.MarkdownBuilder = builder;
                LoadingPanel.IsVisible = false;
                ContentScrollViewer.IsVisible = true;
            }, DispatcherPriority.Background);
        }

        private void SampleProjectButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_openZipStream == null)
            {
                return;
            }

            using var stream = _openZipStream();
            if (stream == null)
            {
                return;
            }

            string? readme = null;
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    if (string.Equals(entry.Name, "README.md", StringComparison.OrdinalIgnoreCase))
                    {
                        using var reader = new StreamReader(entry.Open());
                        readme = reader.ReadToEnd();

                        break;
                    }
                }
            }

            if (readme == null)
            {
                return;
            }

            var extractedPath = ExtractZipToTemp(_openZipStream);
            if (extractedPath == null)
            {
                return;
            }

            // Open the DemoMod subfolder directly rather than the temp extraction root
            var demoModPath = Path.Combine(extractedPath, "DemoMod");
            var openPath = Directory.Exists(demoModPath) ? demoModPath : extractedPath;

            var sampleWindow = new GuideWindow("Sample Project — DemoMod", readme, null, openPath);
            _ = sampleWindow.ShowDialog(this);
        }

        private void OpenFolderButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_extractedPath == null || !Directory.Exists(_extractedPath))
            {
                return;
            }

            OpenFolder(_extractedPath);
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private static string? ExtractZipToTemp(Func<Stream?> openZipStream)
        {
            try
            {
                using var stream = openZipStream();
                if (stream == null)
                {
                    return null;
                }

                var tempDir = Path.Combine(Path.GetTempPath(), "DemoMod-" + Guid.NewGuid().ToString("N")[..8]);
                var fullTempDir = Path.GetFullPath(tempDir + Path.DirectorySeparatorChar);
                using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
                foreach (var entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        continue;
                    }

                    var destPath = Path.GetFullPath(Path.Combine(tempDir, entry.FullName));

                    // Ensure the entry doesn't escape the target directory (zip slip protection)
                    if (!destPath.StartsWith(fullTempDir, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var destDir = Path.GetDirectoryName(destPath);
                    if (destDir != null)
                    {
                        _ = Directory.CreateDirectory(destDir);
                    }

                    using var entryStream = entry.Open();
                    using var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write);
                    entryStream.CopyTo(fileStream);
                }

                return tempDir;
            }
            catch
            {
                return null;
            }
        }

        private static void OpenFolder(string path)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _ = Process.Start(new ProcessStartInfo("explorer.exe", path) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    _ = Process.Start(new ProcessStartInfo("open", path) { UseShellExecute = true });
                }
                else
                {
                    _ = Process.Start(new ProcessStartInfo("xdg-open", path) { UseShellExecute = true });
                }
            }
            catch
            {
                // Best-effort — silently ignore if the file manager can't be opened
            }
        }
    }
}
