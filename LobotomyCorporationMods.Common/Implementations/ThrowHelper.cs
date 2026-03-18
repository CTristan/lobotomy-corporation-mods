// SPDX-License-Identifier: MIT

using System;
using System.Runtime.CompilerServices;

namespace Hemocode.Common.Implementations
{
    public static class ThrowHelper
    {
        public static void ThrowIfNull(
            object argument,
            [CallerArgumentExpression("argument")] string paramName = null)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}