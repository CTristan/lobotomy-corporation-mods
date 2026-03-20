// SPDX-License-Identifier: MIT

using System;

namespace DebugPanel.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class ValidatedNotNullAttribute : Attribute
    {
    }
}
