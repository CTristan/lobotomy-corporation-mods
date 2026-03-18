// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;

#endregion

namespace Hemocode.Playwright.Queries
{
    /// <summary>
    /// Represents a game window or panel in the UI accessibility tree.
    /// </summary>
    public sealed class UiWindowData
    {
        /// <summary>
        /// Name of the window (e.g., "AgentInfoWindow", "CommandWindow").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Whether the window is currently open and visible.
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// Type/category of window for categorization.
        /// </summary>
        public string WindowType { get; set; }

        /// <summary>
        /// Child UI elements within this window (populated when depth="full" or depth="window").
        /// May be null for summary queries.
        /// </summary>
        public ICollection<UiNodeData> Children { get; set; }
    }
}
