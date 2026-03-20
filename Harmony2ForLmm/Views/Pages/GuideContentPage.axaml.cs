// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
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
        private string? _docFilePath;
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
            Action<UserControl, string, double, double> navigateAction,
            string? docFilePath = null)
            : this(markdownContent)
        {
            _docFilePath = docFilePath;
            _openZipStream = openZipStream;
            _navigateAction = navigateAction;
            SampleProjectButton.IsVisible = true;

            if (_docFilePath != null)
            {
                OpenDocButton.IsVisible = true;
            }
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

        /// <summary>
        /// Creates a <see cref="GuideContentPage"/> with markdown content and an on-disk document path.
        /// </summary>
        public static GuideContentPage WithDocPath(string markdownContent, string? docFilePath)
        {
            var page = new GuideContentPage(markdownContent) { _docFilePath = docFilePath };

            if (docFilePath != null)
            {
                page.OpenDocButton.IsVisible = true;
            }

            return page;
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

        private void OpenDocButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_docFilePath == null)
            {
                return;
            }

            if (File.Exists(_docFilePath))
            {
                DocStatusMessage.IsVisible = false;
                OpenWithDefaultApp(_docFilePath);
            }
            else
            {
                DocStatusMessage.Text = "Document not found — install first to copy docs to disk.";
                DocStatusMessage.IsVisible = true;
            }
        }

        private void OpenFolderButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_extractedPath == null || !Directory.Exists(_extractedPath))
            {
                return;
            }

            OpenWithDefaultApp(_extractedPath);
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

        private static void OpenWithDefaultApp(string path)
        {
            try
            {
                _ = Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch
            {
                // Best-effort — silently ignore if the default app can't be opened
            }
        }
    }
}
