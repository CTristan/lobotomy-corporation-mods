// SPDX-License-Identifier: MIT

using System;
using System.IO;
using AwesomeAssertions;
using Mono.Cecil;
using RetargetHarmony.Installer.Interfaces;
using RetargetHarmony.Installer.Services;
using Xunit;

namespace RetargetHarmony.Installer.Test.Services
{
    /// <summary>
    /// Tests for <see cref="BaseModsAnalyzer"/>.
    /// </summary>
    public sealed class BaseModsAnalyzerTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly string _gameDir;
        private readonly string _baseModsDir;
        private readonly BaseModsAnalyzer _analyzer;

        public BaseModsAnalyzerTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            _gameDir = Path.Combine(_tempDir, "game");
            _baseModsDir = Path.Combine(_gameDir, "BaseMods");
            Directory.CreateDirectory(_baseModsDir);
            _analyzer = new BaseModsAnalyzer();
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }

        [Fact]
        public void Analyze_returns_empty_when_BaseMods_does_not_exist()
        {
            var gameDirWithoutBaseMods = Path.Combine(_tempDir, "empty_game");
            Directory.CreateDirectory(gameDirWithoutBaseMods);

            var result = _analyzer.Analyze(gameDirWithoutBaseMods);

            result.Should().BeEmpty();
        }

        [Fact]
        public void Analyze_skips_non_dotnet_dlls()
        {
            File.WriteAllBytes(Path.Combine(_baseModsDir, "native.dll"), [0x4D, 0x5A, 0, 0]);

            var result = _analyzer.Analyze(_gameDir);

            result.Should().BeEmpty();
        }

        [Fact]
        public void Analyze_flags_dll_referencing_Harmony_2()
        {
            CreateTestAssembly("HarmonyMod.dll", ("0Harmony", new Version(2, 0, 0, 0)));

            var result = _analyzer.Analyze(_gameDir);

            result.Should().HaveCount(1);
            result[0].FileName.Should().Be("HarmonyMod.dll");
            result[0].Reason.Should().Be(FlagReason.Harmony2Reference);
            result[0].ReferencedAssembly.Should().Contain("0Harmony");
        }

        [Fact]
        public void Analyze_does_not_flag_dll_referencing_Harmony_1()
        {
            CreateTestAssembly("Harmony1Mod.dll", ("0Harmony", new Version(1, 2, 0, 0)));

            var result = _analyzer.Analyze(_gameDir);

            result.Should().BeEmpty();
        }

        [Fact]
        public void Analyze_flags_dll_referencing_BepInEx_assemblies()
        {
            CreateTestAssembly("BepInExMod.dll", ("BepInEx.Core", new Version(5, 4, 21, 0)));

            var result = _analyzer.Analyze(_gameDir);

            result.Should().HaveCount(1);
            result[0].FileName.Should().Be("BepInExMod.dll");
            result[0].Reason.Should().Be(FlagReason.BepInExReference);
            result[0].ReferencedAssembly.Should().Contain("BepInEx");
        }

        [Fact]
        public void Analyze_does_not_flag_clean_dll()
        {
            CreateTestAssembly("CleanMod.dll");

            var result = _analyzer.Analyze(_gameDir);

            result.Should().BeEmpty();
        }

        [Fact]
        public void Analyze_scans_subdirectories()
        {
            var subDir = Path.Combine(_baseModsDir, "SubFolder");
            Directory.CreateDirectory(subDir);
            CreateTestAssemblyAtPath(Path.Combine(subDir, "NestedMod.dll"), ("BepInEx.Harmony", new Version(1, 0, 0, 0)));

            var result = _analyzer.Analyze(_gameDir);

            result.Should().HaveCount(1);
            result[0].FileName.Should().Be("NestedMod.dll");
        }

        private void CreateTestAssembly(string fileName, params (string Name, Version Version)[] references)
        {
            CreateTestAssemblyAtPath(Path.Combine(_baseModsDir, fileName), references);
        }

        private static void CreateTestAssemblyAtPath(string path, params (string Name, Version Version)[] references)
        {
            var assemblyName = new AssemblyNameDefinition(Path.GetFileNameWithoutExtension(path), new Version(1, 0, 0, 0));
            using var assembly = AssemblyDefinition.CreateAssembly(assemblyName, "MainModule", ModuleKind.Dll);

            foreach (var (name, version) in references)
            {
                var reference = new AssemblyNameReference(name, version);
                assembly.MainModule.AssemblyReferences.Add(reference);
            }

            assembly.Write(path);
        }
    }
}
