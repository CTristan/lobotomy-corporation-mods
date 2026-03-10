// SPDX-License-Identifier: MIT

// ReSharper disable once CheckNamespace
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    internal sealed class ExcludeFromCodeCoverageAttribute : Attribute
    {
        public string Justification { get; set; }
    }
}
