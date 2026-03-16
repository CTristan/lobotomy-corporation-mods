// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Playwright.Queries
{
    /// <summary>
    /// Represents a single UI element in the accessibility tree.
    /// Used for structured UI introspection so text-only agents can "see" the game's UI.
    /// </summary>
    public sealed class UiNodeData
    {
        /// <summary>
        /// Slash-delimited path from the window root to this node (e.g., "Panel/TitleText").
        /// Capped at 3-4 levels to keep output concise.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Type of UI element: text, button, toggle, slider, image, or other.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Current value or text content of the element.
        /// For buttons: the label text.
        /// For toggles: "true"/"false".
        /// For sliders: the numeric value.
        /// For text: the displayed text.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Whether the element is interactable (can be clicked/interacted with by the player).
        /// </summary>
        public bool Interactable { get; set; }
    }
}
