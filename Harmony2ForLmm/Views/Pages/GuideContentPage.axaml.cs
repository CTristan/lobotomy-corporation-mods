// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using LiveMarkdown.Avalonia;

namespace Harmony2ForLmm.Views.Pages
{
    /// <summary>
    /// Page for displaying markdown guide content with optional sample project support.
    /// </summary>
    public sealed partial class GuideContentPage : UserControl, IAsyncLoadablePage
    {
        private readonly Func<Stream?>? _openZipStream;
        private readonly Action<UserControl, string, double, double>? _navigateAction;
        private readonly string? _extractedPath;
        private string? _pendingMarkdown;
        private bool _loaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="GuideContentPage"/> class.
        /// </summary>
        public GuideContentPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GuideContentPage"/> class with markdown content.
        /// </summary>
        public GuideContentPage(string markdownContent)
            : this()
        {
            _pendingMarkdown = markdownContent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GuideContentPage"/> class with sample project support.
        /// </summary>
        public GuideContentPage(
            string markdownContent,
            Func<Stream?> openZipStream,
            Action<UserControl, string, double, double> navigateAction)
            : this(markdownContent)
        {
            _openZipStream = openZipStream;
            _navigateAction = navigateAction;
            SampleProjectButton.IsVisible = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GuideContentPage"/> class for an extracted sample project.
        /// </summary>
        public GuideContentPage(string markdownContent, string extractedPath)
            : this(markdownContent)
        {
            _extractedPath = extractedPath;
            OpenFolderButton.IsVisible = true;
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

        private void SampleProjectButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_openZipStream == null || _navigateAction == null)
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

            var samplePage = new GuideContentPage(readme, openPath);
            _navigateAction(samplePage, "Sample Project — DemoMod", 800, 700);
        }

        private void OpenFolderButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_extractedPath == null || !Directory.Exists(_extractedPath))
            {
                return;
            }

            OpenFolder(_extractedPath);
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
