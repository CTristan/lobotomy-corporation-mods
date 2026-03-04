// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Extensions
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal static class TextureExtensions
    {
        internal static bool IsUnityNull(this Texture texture)
        {
            return !texture;
        }
    }
}
