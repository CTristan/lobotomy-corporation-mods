// SPDX-License-Identifier: MIT

using System.Collections.Generic;

namespace LobotomyPlaywright.Queries
{
    /// <summary>
    /// Represents a game window or panel in the UI accessibility tree.
    /// </summary>
    public class UiWindowData
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
        public List<UiNodeData> Children { get; set; }
    }
}
