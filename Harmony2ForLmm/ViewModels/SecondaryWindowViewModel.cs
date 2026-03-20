// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using Avalonia.Controls;

namespace Harmony2ForLmm.ViewModels
{
    /// <summary>
    /// View model for the secondary navigation window that supports in-place navigation with back.
    /// </summary>
    public sealed class SecondaryWindowViewModel : ViewModelBase
    {
        private readonly Stack<PageState> _navigationStack = new();
        private Action? _closeAction;

        /// <summary>
        /// Gets the current page being displayed.
        /// </summary>
        public UserControl? CurrentPage
        {
            get;
            private set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the window title.
        /// </summary>
        public string Title
        {
            get;
            private set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged();
                }
            }
        } = string.Empty;

        /// <summary>
        /// Gets the preferred width for the window.
        /// </summary>
        public double PreferredWidth
        {
            get;
            private set
            {
                if (!Equals(field, value))
                {
                    field = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the preferred height for the window.
        /// </summary>
        public double PreferredHeight
        {
            get;
            private set
            {
                if (!Equals(field, value))
                {
                    field = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets whether the back button should be visible.
        /// </summary>
        public bool CanGoBack => _navigationStack.Count > 0;

        /// <summary>
        /// Gets the command to navigate back.
        /// </summary>
        public RelayCommand BackCommand { get; }

        /// <summary>
        /// Gets the command to close the window.
        /// </summary>
        public RelayCommand CloseCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecondaryWindowViewModel"/> class.
        /// </summary>
        public SecondaryWindowViewModel()
        {
            BackCommand = new RelayCommand(GoBack, () => CanGoBack);
            CloseCommand = new RelayCommand(() => _closeAction?.Invoke());
        }

        /// <summary>
        /// Navigates to a new page, pushing the current page onto the stack.
        /// </summary>
        public void NavigateTo(UserControl page, string title, double width, double height)
        {
            if (CurrentPage != null)
            {
                _navigationStack.Push(new PageState(CurrentPage, Title, PreferredWidth, PreferredHeight));
            }

            CurrentPage = page;
            Title = title;
            PreferredWidth = width;
            PreferredHeight = height;
            OnPropertyChanged(nameof(CanGoBack));
            BackCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Sets the action to invoke when the close command is executed.
        /// </summary>
        public void SetCloseAction(Action closeAction)
        {
            _closeAction = closeAction;
        }

        private void GoBack()
        {
            if (_navigationStack.Count == 0)
            {
                return;
            }

            var previous = _navigationStack.Pop();
            CurrentPage = previous.Page;
            Title = previous.Title;
            PreferredWidth = previous.Width;
            PreferredHeight = previous.Height;
            OnPropertyChanged(nameof(CanGoBack));
            BackCommand.NotifyCanExecuteChanged();
        }

        private sealed record PageState(UserControl Page, string Title, double Width, double Height);
    }
}
