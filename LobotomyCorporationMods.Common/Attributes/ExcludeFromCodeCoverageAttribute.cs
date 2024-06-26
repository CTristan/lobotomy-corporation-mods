// SPDX-License-Identifier: MIT

// ReSharper disable once CheckNamespace

namespace System.Diagnostics.CodeAnalysis
{
    /// <inheritdoc />
    /// <summary>
    ///     ONLY to be used for Adapter classes since they are just wrappers for Unity methods and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public sealed class ExcludeFromCodeCoverageAttribute : Attribute
    {
    }
}
