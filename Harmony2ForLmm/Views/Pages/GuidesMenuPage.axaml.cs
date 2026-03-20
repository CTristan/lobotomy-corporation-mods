// SPDX-License-Identifier: MIT

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Harmony2ForLmm.Views.Pages
{
    /// <summary>
    /// Menu page with buttons for opening User's Guide and Modder's Guide.
    /// </summary>
    public sealed partial class GuidesMenuPage : UserControl
    {
        private readonly Action _openUsersGuide;
        private readonly Action _openModdersGuide;

        /// <summary>
        /// Initializes a new instance of the <see cref="GuidesMenuPage"/> class.
        /// </summary>
        public GuidesMenuPage()
        {
            _openUsersGuide = () => { };
            _openModdersGuide = () => { };
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GuidesMenuPage"/> class.
        /// </summary>
        public GuidesMenuPage(Action openUsersGuide, Action openModdersGuide)
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
    }
}
