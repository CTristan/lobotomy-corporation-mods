// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;

#endregion

namespace Hemocode.Playwright.Queries
{
    /// <summary>
    /// Represents a single node in the GameObject hierarchy tree.
    /// Used for structured scene exploration so text-only agents can discover any GameObject.
    /// </summary>
    public sealed class GameObjectNodeData
    {
        /// <summary>
        /// Name of the GameObject.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Slash-delimited path from the scene root to this node (e.g., "Canvas/Panel/Button").
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Whether the GameObject is active in the hierarchy.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// The tag assigned to this GameObject (e.g., "Untagged", "MainCamera").
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// The layer index of this GameObject.
        /// </summary>
        public int Layer { get; set; }

        /// <summary>
        /// Component type names attached to this GameObject.
        /// </summary>
        public string[] Components { get; set; }

        /// <summary>
        /// Number of direct children of this GameObject.
        /// </summary>
        public int ChildCount { get; set; }

        /// <summary>
        /// Child nodes, populated only to the requested depth.
        /// </summary>
        public ICollection<GameObjectNodeData> Children { get; set; }
    }
}
