// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class ColorExtensions
    {
        /// <summary>Converts the given color to a hexadecimal representation in the RGB format.</summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The color as a string in the RGB format.</returns>
        [NotNull]
        public static string ToHtmlStringRgb(this Color color)
        {
            return $"#{ColorUtility.ToHtmlStringRGB(color)}";
        }
    }
}
