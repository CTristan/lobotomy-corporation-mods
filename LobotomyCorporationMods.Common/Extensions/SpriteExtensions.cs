// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using UnityEngine;

namespace Hemocode.Common.Extensions
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public static class SpriteExtensions
    {
        internal static bool IsUnityNull(this Sprite sprite)
        {
            return !sprite;
        }
    }
}
