// SPDX-License-Identifier:MIT

namespace LobotomyCorporationMods.Common.Enums
{
    /// <summary>
    ///     Specifies how a UI element created by <c>UiFactory</c> should be positioned and sized.
    /// </summary>
    public enum UiLayoutMode
    {
        /// <summary>
        ///     The element is positioned manually using an anchored position and size.
        ///     Use this when placing UI directly on a canvas or overlay.
        /// </summary>
        Absolute,

        /// <summary>
        ///     The element is placed inside a layout group (vertical or horizontal) created by <c>UiFactory</c>.
        ///     The group automatically controls the element's positioning and size.
        ///     For example:
        ///     <code>
        /// var panel = UiFactory.CreateVerticalGroup(canvas.transform, "MyGroup");
        /// var text = UiFactory.CreateText(panel.transform, "MyText", UiLayoutMode.Grouped);
        /// </code>
        /// </summary>
        Grouped
    }
}
