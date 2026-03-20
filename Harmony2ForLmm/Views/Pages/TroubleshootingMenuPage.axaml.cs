// SPDX-License-Identifier: MIT

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Harmony2ForLmm.Views.Pages
{
    /// <summary>
    /// Menu page with buttons for DebugPanel and User's Guide troubleshooting tools.
    /// </summary>
    public sealed partial class TroubleshootingMenuPage : UserControl
    {
        private readonly Action _openDebugPanel;
        private readonly Action _openUsersGuide;

        /// <summary>
        /// Initializes a new instance of the <see cref="TroubleshootingMenuPage"/> class.
        /// </summary>
        public TroubleshootingMenuPage()
        {
            _openDebugPanel = () => { };
            _openUsersGuide = () => { };
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TroubleshootingMenuPage"/> class.
        /// </summary>
        public TroubleshootingMenuPage(Action openDebugPanel, Action openUsersGuide)
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
    }
}
