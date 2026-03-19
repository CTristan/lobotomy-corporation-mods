// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using LiveMarkdown.Avalonia;

namespace Harmony2ForLmm.Views
{
    /// <summary>
    /// Window for displaying markdown guide content.
    /// </summary>
    public sealed partial class GuideWindow : Window
    {
        private readonly string? _zipPath;
        private readonly string? _extractedPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="GuideWindow"/> class.
        /// </summary>
        public GuideWindow()
        {
            InitializeComponent();
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
            SetMarkdownContent(markdownContent);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GuideWindow"/> class with content
        /// and an associated sample project zip. Shows "View Sample Project" button.
        /// </summary>
        /// <param name="title">The window title.</param>
        /// <param name="markdownContent">The markdown content to display.</param>
        /// <param name="zipPath">Path to the sample project zip.</param>
        public GuideWindow(string title, string markdownContent, string zipPath)
            : this(title, markdownContent)
        {
            _zipPath = zipPath;
            SampleProjectButton.IsVisible = true;
        }

        private GuideWindow(string title, string markdownContent, string? zipPath, string extractedPath)
            : this(title, markdownContent)
        {
            _zipPath = zipPath;
            _extractedPath = extractedPath;
            OpenFolderButton.IsVisible = true;
        }

        private void SetMarkdownContent(string markdownContent)
        {
            var builder = new ObservableStringBuilder();
            _ = builder.Append(markdownContent);
            MarkdownViewer.MarkdownBuilder = builder;
        }

        private void SampleProjectButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_zipPath == null || !File.Exists(_zipPath))
            {
                return;
            }

            string? readme = null;
            using (var archive = ZipFile.OpenRead(_zipPath))
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

            var extractedPath = ExtractZipToTemp(_zipPath);
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

        private static string? ExtractZipToTemp(string zipPath)
        {
            try
            {
                var tempDir = Path.Combine(Path.GetTempPath(), "DemoMod-" + Guid.NewGuid().ToString("N")[..8]);
                ZipFile.ExtractToDirectory(zipPath, tempDir);

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
