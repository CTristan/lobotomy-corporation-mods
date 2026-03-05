// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using HarmonyDebugPanel.Implementations.Collectors;
using HarmonyDebugPanel.Interfaces;
using Xunit;

#pragma warning disable CA1515 // Test classes must be public for xUnit

namespace HarmonyDebugPanel.Test;

public sealed class AppDomainAndFileSystemCollectorTests
{
    [Fact]
    public void AppDomainAssemblyInspectionSource_ReturnsLoadedAssemblies()
    {
        var source = new AppDomainAssemblyInspectionSource();

        var assemblies = source.GetAssemblies().ToList();

        assemblies.Should().NotBeEmpty();
        assemblies.Should().Contain(a => a.Name == typeof(AppDomainAndFileSystemCollectorTests).Assembly.GetName().Name);
    }

    [Fact]
    public void AppDomainBaseDirectoryProvider_ReturnsCurrentDomainBaseDirectory()
    {
        var provider = new AppDomainBaseDirectoryProvider();

        var baseDirectory = provider.GetBaseDirectory();

        baseDirectory.Should().Be(AppDomain.CurrentDomain.BaseDirectory);
    }

    [Fact]
    public void CollectorFileSystem_DirectoryOperations_WorkAsExpected()
    {
        var fileSystem = new CollectorFileSystem();
        var tempDirectory = Path.Combine(Path.GetTempPath(), "HarmonyDebugPanel.Test", Guid.NewGuid().ToString("N"));
        var childDirectory = Path.Combine(tempDirectory, "Child");

        try
        {
            Directory.CreateDirectory(childDirectory);

            fileSystem.DirectoryExists(tempDirectory).Should().BeTrue();
            fileSystem.EnumerateDirectories(tempDirectory).Should().ContainSingle(d => d == childDirectory);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }
}
