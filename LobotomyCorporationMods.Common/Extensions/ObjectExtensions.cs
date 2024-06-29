// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class ObjectExtensions
    {
        [ContractAnnotation("null => true")]
        public static bool IsNull([CanBeNull] [ValidatedNotNull] this object value)
        {
#pragma warning disable IDE0041
            return ReferenceEquals(value, null);
#pragma warning restore IDE0041
        }

        [ContractAnnotation("null => false")]
        public static bool IsNotNull([CanBeNull] [ValidatedNotNull] this object value)
        {
            return !value.IsNull();
        }
    }
}
