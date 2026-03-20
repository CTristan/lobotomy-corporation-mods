// SPDX-License-Identifier: MIT

#if !NET6_0_OR_GREATER
// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    /*
     * Polyfill for CallerArgumentExpression support on older targets.
     * This works because the project is compiled with a modern C# compiler
     * even though it targets .NET Framework 3.5.
     */
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class CallerArgumentExpressionAttribute : Attribute
    {
        public CallerArgumentExpressionAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }

        public string ParameterName { get; private set; }
    }
}
#endif