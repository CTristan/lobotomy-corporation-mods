// SPDX-License-Identifier: MIT

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Harmony2ForLmm.Views
{
    /// <summary>
    /// Window with buttons for opening User's Guide and Modder's Guide.
    /// </summary>
    public sealed partial class GuidesWindow : Window
    {
        private readonly Action _openUsersGuide;
        private readonly Action _openModdersGuide;

        /// <summary>
        /// Initializes a new instance of the <see cref="GuidesWindow"/> class.
        /// </summary>
        public GuidesWindow()
        {
            _openUsersGuide = () => { };
            _openModdersGuide = () => { };
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GuidesWindow"/> class.
        /// </summary>
        /// <param name="openUsersGuide">Action to open the User's Guide.</param>
        /// <param name="openModdersGuide">Action to open the Modder's Guide.</param>
        public GuidesWindow(Action openUsersGuide, Action openModdersGuide)
        {
            _openUsersGuide = openUsersGuide;
            _openModdersGuide = openModdersGuide;
            InitializeComponent();
        }

        private void UsersGuideButton_Click(object? sender, RoutedEventArgs e)
        {
            _openUsersGuide();
        }

        private void ModdersGuideButton_Click(object? sender, RoutedEventArgs e)
        {
            _openModdersGuide();
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
