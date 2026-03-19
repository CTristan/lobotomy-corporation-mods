// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;

#endregion

namespace Hemocode.Playwright.Queries
{
    /// <summary>
    /// Search results container for GameObject queries.
    /// </summary>
    public sealed class GameObjectSearchResult
    {
        /// <summary>
        /// Description of the search criteria used.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Total number of results found.
        /// </summary>
        public int ResultCount { get; set; }

        /// <summary>
        /// Flat list of matching GameObjects (no children populated).
        /// </summary>
        public ICollection<GameObjectNodeData> Results { get; set; }
    }
}
