// SPDX-License-Identifier: MIT

#region

#endregion

// ReSharper disable once CheckNamespace
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsageAttribute(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property |
                             AttributeTargets.Event,
        Inherited = false)]
    // ReSharper disable once MemberCanBeInternal
    public sealed class ExcludeFromCodeCoverageAttribute : Attribute
    {
        /// <summary>Gets or sets the justification for excluding the member from code coverage.</summary>
        // ReSharper disable once UnusedMember.Global
        public string Justification { get; set; }
    }
}
