// SPDX-License-Identifier: MIT

using System;

namespace Hemocode.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class ValidatedNotNullAttribute : Attribute
    {
    }
}
