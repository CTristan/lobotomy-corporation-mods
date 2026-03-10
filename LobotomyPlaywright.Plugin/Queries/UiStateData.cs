// SPDX-License-Identifier: MIT

using System.Collections.Generic;

namespace LobotomyPlaywright.Queries
{
    /// <summary>
    /// Represents the complete UI state for accessibility queries.
    /// This is the primary way text-only agents "see" the game's visual/UI state.
    /// </summary>
    public class UiStateData
    {
        /// <summary>
        /// List of known game windows with their open/closed states.
        /// For summary queries, contains all windows with IsOpen boolean.
        /// For full queries, includes Children for open windows.
        /// </summary>
        public ICollection<UiWindowData> Windows { get; set; }

        /// <summary>
        /// Current activated slot names (0-4) from UIActivateManager.
        /// Represents which agent slots are currently active in the UI.
        /// </summary>
        public ICollection<string> ActivatedSlots { get; set; }

        /// <summary>
        /// List of mod-specific UI elements detected in the scene.
        /// Populated by scanning for known mod UI patterns (e.g., GiftAlertIcon).
        /// </summary>
        public ICollection<UiNodeData> ModElements { get; set; }
    }
}
