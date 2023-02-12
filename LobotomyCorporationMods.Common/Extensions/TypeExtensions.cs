// SPDX-License-Identifier: MIT

using System;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.Common.Extensions
{
    internal static class TypeExtensions
    {
        /// <summary>
        ///     Fastest and safest way to determine if a type inherits a base class or implements an interface.
        ///     Just remember that you need to check that the parent is assignable from the inheritor, not the other way around.
        ///     [InterfaceOrBaseClass].IsAssignableFrom([ClassThatInheritsOrImplements]();
        ///     https://stackoverflow.com/a/4963190/1410257
        /// </summary>
        internal static bool IsHarmonyPatch(this Type type)
        {
            return typeof(HarmonyPatchBase).IsAssignableFrom(type);
        }
    }
}
