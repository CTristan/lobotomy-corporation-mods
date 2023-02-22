// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using LobotomyCorporationMods.Common.Attributes;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.Common.Attributes
{
    public sealed class ExcludeFromCodeCoverageAttributeTests
    {
        private const string ExcludeFromCodeCoverageAttributeTypeName = "System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute";
        private const string ModsNamespace = "LobotomyCorporationMods.";
        private static readonly int s_namespaceMinLength = ModsNamespace.Length;

        /// <summary>
        ///     Uses reflection to get every mod being referenced by the Tests projects, then iterates through all of the classes
        ///     and methods using the ExcludeFromCodeCoverage attribute to make sure they also have another attribute that
        ///     indicates why we're excluding from code coverage.
        /// </summary>
        [Fact]
        public void Verify_that_code_coverage_exclusions_are_only_on_appropriately_attributed_classes_and_methods()
        {
            var currentAssembly = typeof(ExcludeFromCodeCoverageAttributeTests).Assembly;
            var referencedAssemblies = currentAssembly.GetReferencedAssemblies()
                .Where(static name => name.Name.Length >= s_namespaceMinLength && name.Name.Substring(0, s_namespaceMinLength) == ModsNamespace);

            var invalidAttributeFound = AnyModIsIncorrectlyExcludedFromCodeCoverage(referencedAssemblies);

            invalidAttributeFound.Should().BeNullOrWhiteSpace();
        }

        #region Helper Methods

        private static string AnyModIsIncorrectlyExcludedFromCodeCoverage(IEnumerable<AssemblyName> referencedAssemblies)
        {
            var invalidClasses = referencedAssemblies.Select(static assemblyName => Assembly.Load(assemblyName.Name))
                .Select(static assembly =>
                    assembly.GetTypes()
                        .Where(static type => type.IsClass && type.Namespace is not null && type.Namespace.Length >= s_namespaceMinLength &&
                                              type.Namespace.Substring(0, s_namespaceMinLength) == ModsNamespace))
                .Select(static classes => AnyClassIsIncorrectlyExcludedFromCodeCoverage(classes))
                .Where(static className => !string.IsNullOrEmpty(className))
                .ToList();

            return invalidClasses.Any() ? invalidClasses.First() : string.Empty;
        }

        private static string AnyClassIsIncorrectlyExcludedFromCodeCoverage(IEnumerable<Type> classes)
        {
            // Iterate through every class in every mod
            foreach (var reflectionClass in classes)
            {
                // First need to check if the class itself is marked with ExcludeFromCodeCoverage
                var className = ClassIsIncorrectlyExcludedFromCodeCoverage(reflectionClass);
                if (!string.IsNullOrEmpty(className))
                {
                    return className;
                }

                // Now we need to check all of the methods in the class
                var methods = reflectionClass.GetMethods();
                var methodName = AnyMethodIsIncorrectlyExcludedFromCodeCoverage(methods);
                if (!string.IsNullOrEmpty(methodName))
                {
                    return methodName + " in " + reflectionClass;
                }
            }

            return string.Empty;
        }

        private static string AnyMethodIsIncorrectlyExcludedFromCodeCoverage(MethodInfo[] methods)
        {
            var invalidMethods = methods.Select(static method => MethodIsIncorrectlyExcludeFromCodeCoverage(method)).Where(static methodName => !string.IsNullOrEmpty(methodName)).ToList();

            return invalidMethods.Any() ? invalidMethods.First() : string.Empty;
        }

        private static string MethodIsIncorrectlyExcludeFromCodeCoverage(MemberInfo method)
        {
            var attributes = method.GetCustomAttributes(false);
            foreach (var attribute in attributes)
            {
                if (attribute.ToString() == ExcludeFromCodeCoverageAttributeTypeName && !attributes.Any(static o => o is EntryPointAttribute))
                {
                    return method.ToString();
                }
            }

            return string.Empty;
        }

        private static string ClassIsIncorrectlyExcludedFromCodeCoverage(MemberInfo reflectionClass)
        {
            var attributes = reflectionClass.GetCustomAttributes(false);
            foreach (var attribute in attributes)
            {
                if (attribute.ToString() == ExcludeFromCodeCoverageAttributeTypeName && !attributes.Any(static o => o is AdapterClassAttribute))
                {
                    // Only Adapter classes can have code coverage excluded for the entire class
                    return reflectionClass.ToString();
                }
            }

            return string.Empty;
        }

        #endregion
    }
}
