// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Extensions;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.Common.Attributes
{
    public sealed class ExcludeFromCodeCoverageAttributeTests
    {
        private const string ExcludeFromCodeCoverageAttributeTypeName = "System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute";

        /// <summary>
        ///     Uses reflection to get every mod being referenced by the Tests projects, then iterates through all the classes and methods using the ExcludeFromCodeCoverage attribute to
        ///     make sure they also have another attribute that indicates why we're excluding from code coverage.
        /// </summary>
        [Fact]
        public void Verify_that_code_coverage_exclusions_are_only_on_appropriately_attributed_classes_and_methods()
        {
            var currentAssembly = typeof(ExcludeFromCodeCoverageAttributeTests).Assembly;
            var referencedAssemblies = currentAssembly.GetReferencedAssemblies().Where(name => IsInModsNamespace(name.Name));

            var invalidAttributeFound = AnyModIsIncorrectlyExcludedFromCodeCoverage(referencedAssemblies);

            invalidAttributeFound.Should().BeNullOrWhiteSpace();
        }

        #region Helper Methods

        private static bool IsInModsNamespace([CanBeNull] string name)
        {
            const string ModsNamespace = "LobotomyCorporationMods.";
            var namespaceMinLength = ModsNamespace.Length;

            return !name.IsNull() && name.Length >= namespaceMinLength && name.Substring(0, namespaceMinLength) == ModsNamespace;
        }

        private static string AnyModIsIncorrectlyExcludedFromCodeCoverage([NotNull] IEnumerable<AssemblyName> referencedAssemblies)
        {
            var invalidClasses = referencedAssemblies.Select(assemblyName => Assembly.Load(assemblyName.Name))
                .Select(assembly => assembly.GetTypes().Where(type => type.IsClass && IsInModsNamespace(type.Namespace))).Select(AnyClassIsIncorrectlyExcludedFromCodeCoverage)
                .Where(className => !string.IsNullOrEmpty(className)).ToList();

            return invalidClasses.Count != 0 ? invalidClasses[0] : string.Empty;
        }

        [NotNull]
        private static string AnyClassIsIncorrectlyExcludedFromCodeCoverage([NotNull] IEnumerable<Type> classes)
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

                // Now we need to check all the methods in the class
                var methods = reflectionClass.GetMethods();
                var methodName = AnyMethodIsIncorrectlyExcludedFromCodeCoverage(methods);
                if (!string.IsNullOrEmpty(methodName))
                {
                    return methodName + " in " + reflectionClass;
                }
            }

            return string.Empty;
        }

        private static string AnyMethodIsIncorrectlyExcludedFromCodeCoverage([NotNull] IEnumerable<MethodInfo> methods)
        {
            var invalidMethods = methods.Select(MethodIsIncorrectlyExcludedFromCodeCoverage).Where(methodName => !string.IsNullOrEmpty(methodName)).ToList();

            return invalidMethods.Count != 0 ? invalidMethods[0] : string.Empty;
        }

        [NotNull]
        private static string MethodIsIncorrectlyExcludedFromCodeCoverage([NotNull] MemberInfo method)
        {
            var attributes = method.GetCustomAttributes(false).ToList();

            return attributes.Exists(attribute => attribute.ToString() == ExcludeFromCodeCoverageAttributeTypeName && !attributes.Exists(o => o is EntryPointAttribute))
                ? method.ToString()
                : string.Empty;
        }

        [NotNull]
        private static string ClassIsIncorrectlyExcludedFromCodeCoverage([NotNull] Type reflectionClass)
        {
            var attributes = reflectionClass.GetCustomAttributes(false).ToList();

            return attributes.Exists(attribute => attribute.ToString() == ExcludeFromCodeCoverageAttributeTypeName && !attributes.Exists(o => o is AdapterClassAttribute))
                ? reflectionClass.ToString()
                : string.Empty;
        }

        #endregion
    }
}
