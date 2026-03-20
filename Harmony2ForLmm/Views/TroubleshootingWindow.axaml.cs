// SPDX-License-Identifier: MIT

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Harmony2ForLmm.Views
{
    /// <summary>
    /// Window with buttons for DebugPanel and User's Guide troubleshooting tools.
    /// </summary>
    public sealed partial class TroubleshootingWindow : Window
    {
        private readonly Action _openDebugPanel;
        private readonly Action _openUsersGuide;

        /// <summary>
        /// Initializes a new instance of the <see cref="TroubleshootingWindow"/> class.
        /// </summary>
        public TroubleshootingWindow()
        {
            _openDebugPanel = () => { };
            _openUsersGuide = () => { };
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TroubleshootingWindow"/> class.
        /// </summary>
        /// <param name="openDebugPanel">Action to open the DebugPanel window.</param>
        /// <param name="openUsersGuide">Action to open the User's Guide.</param>
        public TroubleshootingWindow(Action openDebugPanel, Action openUsersGuide)
        {
            _openDebugPanel = openDebugPanel;
            _openUsersGuide = openUsersGuide;
            InitializeComponent();
        }

        private void DebugPanelButton_Click(object? sender, RoutedEventArgs e)
        {
            _openDebugPanel();
        }

        private void UsersGuideButton_Click(object? sender, RoutedEventArgs e)
        {
            _openUsersGuide();
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
