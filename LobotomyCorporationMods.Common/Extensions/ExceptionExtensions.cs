// SPDX-License-Identifier: MIT

#region

using System;

#endregion

namespace LobotomyCorporationMods.Common.Extensions
{
    internal static class ExceptionExtensions
    {
        /// <summary>
        ///     Only use for Adapter classes for unit testing.
        /// </summary>
        internal static bool IsUnityException(this Exception exception)
        {
            return exception is SystemException or MissingMethodException;
        }
    }
}
