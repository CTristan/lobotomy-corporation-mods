// SPDX-License-Identifier: MIT

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
            var builder = new ObservableStringBuilder();
            _ = builder.Append(markdownContent);
            MarkdownViewer.MarkdownBuilder = builder;
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
