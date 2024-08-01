// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.CommonTests.AttributeTests
{
    /// <summary>Tests for verifying that code coverage exclusions are only applied to appropriately attributed classes and methods.</summary>
    public sealed class ExcludeFromCodeCoverageAttributeTests
    {
        private const string ExcludeFromCodeCoverageAttributeTypeName = "System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute";
        private const string ModsNamespace = "LobotomyCorporationMods.";
        private static readonly int s_namespaceMinLength = ModsNamespace.Length;

        /// <summary>Verifies that code coverage exclusions are only on appropriately attributed classes and methods.</summary>
        [Fact]
        public void Verify_that_code_coverage_exclusions_are_only_on_appropriately_attributed_classes_and_methods()
        {
            var currentAssembly = typeof(ExcludeFromCodeCoverageAttributeTests).Assembly;
            var referencedAssemblies = currentAssembly.GetReferencedAssemblies().Where(name => IsInModsNamespace(name.Name));
            var invalidAttributeFound = AnyModIsIncorrectlyExcludedFromCodeCoverage(referencedAssemblies);
            invalidAttributeFound.Should().BeNullOrWhiteSpace();
        }

        /// <summary>Checks if the given name is within the Mods namespace.</summary>
        /// <param name="name">The name to check.</param>
        /// <returns>True if the name is within the Mods namespace, otherwise false.</returns>
        private static bool IsInModsNamespace([CanBeNull] string name)
        {
            return !string.IsNullOrEmpty(name) && name.Length >= s_namespaceMinLength && name.StartsWith(ModsNamespace, StringComparison.InvariantCulture);
        }

        /// <summary>Checks if any referenced assemblies are incorrectly excluded from code coverage.</summary>
        /// <param name="referencedAssemblies">The referenced assemblies to check.</param>
        /// <returns>The name of the first incorrectly excluded class or method, or an empty string if none are found.</returns>
        [NotNull]
        private static string AnyModIsIncorrectlyExcludedFromCodeCoverage([NotNull] IEnumerable<AssemblyName> referencedAssemblies)
        {
            return referencedAssemblies.Select(assemblyName => Assembly.Load(assemblyName.Name))
                .SelectMany(assembly => assembly.GetTypes().Where(type => type.IsClass && IsInModsNamespace(type.Namespace))).Select(CheckClassAndMethodsForInvalidExclusion)
                .FirstOrDefault(className => !string.IsNullOrEmpty(className)) ?? string.Empty;
        }

        /// <summary>Checks if the given class or its methods are incorrectly excluded from code coverage.</summary>
        /// <param name="reflectionClass">The class to check.</param>
        /// <returns>The name of the first incorrectly excluded class or method, or an empty string if none are found.</returns>
        [NotNull]
        private static string CheckClassAndMethodsForInvalidExclusion([NotNull] Type reflectionClass)
        {
            var className = ClassIsIncorrectlyExcludedFromCodeCoverage(reflectionClass);
            if (!string.IsNullOrEmpty(className))
            {
                return className;
            }

            return reflectionClass.GetMethods().Select(MethodIsIncorrectlyExcludedFromCodeCoverage).FirstOrDefault(methodName => !string.IsNullOrEmpty(methodName)) ?? string.Empty;
        }

        /// <summary>Checks if the given method is incorrectly excluded from code coverage.</summary>
        /// <param name="method">The method to check.</param>
        /// <returns>The name of the method if it is incorrectly excluded, or an empty string if it is not.</returns>
        [NotNull]
        private static string MethodIsIncorrectlyExcludedFromCodeCoverage([NotNull] MemberInfo method)
        {
            var attributes = method.GetCustomAttributes(false).ToList();

            return attributes.Exists(attribute => attribute.ToString() == ExcludeFromCodeCoverageAttributeTypeName && !attributes.Exists(o => o is EntryPointAttribute))
                ? method.ToString()
                : string.Empty;
        }

        /// <summary>Checks if the given class is incorrectly excluded from code coverage.</summary>
        /// <param name="reflectionClass">The class to check.</param>
        /// <returns>The name of the class if it is incorrectly excluded, or an empty string if it is not.</returns>
        [NotNull]
        private static string ClassIsIncorrectlyExcludedFromCodeCoverage([NotNull] Type reflectionClass)
        {
            var attributes = reflectionClass.GetCustomAttributes(false).ToList();

            return attributes.Exists(attribute => attribute.ToString() == ExcludeFromCodeCoverageAttributeTypeName && !attributes.Exists(o => o is ValidCodeCoverageExceptionAttribute))
                ? reflectionClass.ToString()
                : string.Empty;
        }
    }
}
