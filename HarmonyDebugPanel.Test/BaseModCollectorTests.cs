// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using HarmonyDebugPanel.Implementations.Collectors;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;
using Xunit;

#pragma warning disable CA1515 // Test classes must be public for xUnit

namespace HarmonyDebugPanel.Test;

public sealed class BaseModCollectorTests
{
    [Fact]
    public void TryGetBaseModNameFromPath_ReturnsTrue_WhenPathContainsBaseModsSegment()
    {
        var found = BaseModCollector.TryGetBaseModNameFromPath("C:/Games/LobotomyCorp_Data/BaseMods/MyMod/MyMod.dll", out var modName);

        found.Should().BeTrue();
        modName.Should().Be("MyMod");
    }

    [Fact]
    public void TryGetBaseModNameFromPath_ReturnsFalse_WhenBaseModsSegmentMissing()
    {
        var found = BaseModCollector.TryGetBaseModNameFromPath("C:/Games/LobotomyCorp_Data/Managed/SomeAssembly.dll", out var modName);

        found.Should().BeFalse();
        modName.Should().BeEmpty();
    }

    [Fact]
    public void Collect_ReturnsLoadedAssemblyModAndUnloadedDirectoryMod()
    {
        var fileSystem = new StubCollectorFileSystem(
            existingDirectories: new HashSet<string> { "/game/LobotomyCorp_Data/BaseMods", "/game/BaseMods" },
            directoriesByRoot: new Dictionary<string, IEnumerable<string>>
            {
                {
                    "/game/LobotomyCorp_Data/BaseMods",
                    new List<string>
                    {
                        "/game/LobotomyCorp_Data/BaseMods/LoadedMod",
                        "/game/LobotomyCorp_Data/BaseMods/UnloadedMod",
                    }
                },
                { "/game/BaseMods", System.Array.Empty<string>() },
            });

        var baseDirectoryProvider = new StubBaseDirectoryProvider("/game");
        var assemblySource = new StubAssemblySource(new List<AssemblyInspectionInfo>
        {
            new(
                "LoadedModAssembly",
                "1.0.0",
                "/game/LobotomyCorp_Data/BaseMods/LoadedMod/LoadedModAssembly.dll",
                new List<AssemblyName> { new("0Harmony109") }),
        });

        var collector = new BaseModCollector(fileSystem, baseDirectoryProvider, assemblySource, new HarmonyVersionClassifier());

        var mods = collector.Collect();

        mods.Should().HaveCount(2);
        mods.Should().ContainSingle(m => m.Name == "LoadedMod" && m.HarmonyVersion == HarmonyVersion.Harmony1 && m.Version == "1.0.0");
        mods.Should().ContainSingle(m => m.Name == "UnloadedMod" && m.HarmonyVersion == HarmonyVersion.Unknown && m.Version == "Unknown");
    }

    private sealed class StubCollectorFileSystem : ICollectorFileSystem
    {
        private readonly HashSet<string> _existingDirectories;
        private readonly Dictionary<string, IEnumerable<string>> _directoriesByRoot;

        public StubCollectorFileSystem(HashSet<string> existingDirectories, Dictionary<string, IEnumerable<string>> directoriesByRoot)
        {
            _existingDirectories = existingDirectories;
            _directoriesByRoot = directoriesByRoot;
        }

        public bool DirectoryExists(string path)
        {
            return _existingDirectories.Contains(path);
        }

        public IEnumerable<string> EnumerateDirectories(string path)
        {
            return _directoriesByRoot[path];
        }
    }

    private sealed class StubBaseDirectoryProvider : IBaseDirectoryProvider
    {
        private readonly string _baseDirectory;

        public StubBaseDirectoryProvider(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }

        public string GetBaseDirectory()
        {
            return _baseDirectory;
        }
    }

    private sealed class StubAssemblySource : IAssemblyInspectionSource
    {
        private readonly IEnumerable<AssemblyInspectionInfo> _assemblies;

        public StubAssemblySource(IEnumerable<AssemblyInspectionInfo> assemblies)
        {
            _assemblies = assemblies;
        }

        public IEnumerable<AssemblyInspectionInfo> GetAssemblies()
        {
            return _assemblies;
        }
    }
}
