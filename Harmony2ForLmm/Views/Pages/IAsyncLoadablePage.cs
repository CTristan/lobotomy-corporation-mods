// SPDX-License-Identifier: MIT

using System.Threading.Tasks;

namespace Harmony2ForLmm.Views.Pages
{
    /// <summary>
    /// Interface for pages that require asynchronous content loading after being shown.
    /// </summary>
    public interface IAsyncLoadablePage
    {
        /// <summary>
        /// Loads the page content asynchronously.
        /// </summary>
        Task LoadContentAsync();
    }
}
