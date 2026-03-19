// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;

#endregion

namespace Hemocode.Playwright.Queries
{
    /// <summary>
    /// Detailed inspection result for a single GameObject.
    /// </summary>
    public sealed class GameObjectInspectData
    {
        /// <summary>
        /// Name of the GameObject.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Slash-delimited path from the scene root to this node.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Whether the GameObject is active in the hierarchy.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// The tag assigned to this GameObject.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// The layer index of this GameObject.
        /// </summary>
        public int Layer { get; set; }

        /// <summary>
        /// Detailed component data for each component on this GameObject.
        /// </summary>
        public ICollection<ComponentData> Components { get; set; }
    }

    /// <summary>
    /// Data about a single component attached to a GameObject.
    /// </summary>
    public sealed class ComponentData
    {
        /// <summary>
        /// Fully qualified type name of the component.
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Reflected fields of the component (populated only in "full" detail mode).
        /// </summary>
        public ICollection<ComponentFieldData> Fields { get; set; }
    }

    /// <summary>
    /// Data about a single field on a component.
    /// </summary>
    public sealed class ComponentFieldData
    {
        /// <summary>
        /// Name of the field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type name of the field.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// String representation of the field's value.
        /// </summary>
        public string Value { get; set; }
    }
}
