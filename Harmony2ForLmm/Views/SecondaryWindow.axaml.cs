// SPDX-License-Identifier: MIT

using System;
using System.ComponentModel;
using Avalonia.Controls;
using Harmony2ForLmm.ViewModels;
using Harmony2ForLmm.Views.Pages;

namespace Harmony2ForLmm.Views
{
    /// <summary>
    /// Secondary window that supports in-place navigation with a back button.
    /// </summary>
    public sealed partial class SecondaryWindow : Window
    {
        private readonly SecondaryWindowViewModel? _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecondaryWindow"/> class.
        /// </summary>
        public SecondaryWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecondaryWindow"/> class.
        /// </summary>
        public SecondaryWindow(SecondaryWindowViewModel viewModel)
            : this()
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            _viewModel = viewModel;
            DataContext = viewModel;
            viewModel.SetCloseAction(Close);
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_viewModel == null)
            {
                return;
            }

            if (e.PropertyName is nameof(SecondaryWindowViewModel.PreferredWidth)
                or nameof(SecondaryWindowViewModel.PreferredHeight))
            {
                Width = _viewModel.PreferredWidth;
                Height = _viewModel.PreferredHeight;
                CenterOnOwner();
            }
            else if (e.PropertyName is nameof(SecondaryWindowViewModel.CurrentPage))
            {
                OnCurrentPageChanged();
            }
        }

        private void CenterOnOwner()
        {
            if (Owner is not Window owner)
            {
                return;
            }

            var ownerX = owner.Position.X;
            var ownerY = owner.Position.Y;
            var ownerWidth = owner.Width;
            var ownerHeight = owner.Height;

            var x = ownerX + ((ownerWidth - Width) / 2);
            var y = ownerY + ((ownerHeight - Height) / 2);
            Position = new Avalonia.PixelPoint((int)x, (int)y);
        }

        private async void OnCurrentPageChanged()
        {
            if (_viewModel?.CurrentPage is IAsyncLoadablePage loadablePage)
            {
                await loadablePage.LoadContentAsync().ConfigureAwait(true);
            }
        }
    }
}
