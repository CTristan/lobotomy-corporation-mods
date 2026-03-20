// SPDX-License-Identifier: MIT

#region

using System;

#endregion

namespace DebugPanel.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class EntryPointAttribute : Attribute
    {
    }
}
