// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Extensions
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal static class GameObjectTestExtensions
    {
        internal static bool IsUnityNull(this GameObject gameObject)
        {
            return !gameObject;
        }
    }
}
