// SPDX-License-Identifier: MIT

// ReSharper disable CheckNamespace

namespace System.Diagnostics.CodeAnalysis
{
    /// <inheritdoc />
    /// <summary>
    ///     ONLY to be used for Adapter classes since they are just wrappers for Unity methods and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    internal sealed class ExcludeFromCodeCoverageAttribute : Attribute
    {
    }
}
