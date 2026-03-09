// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AwesomeAssertions;
using Mono.Cecil;
using Xunit;

namespace SetupExternal.Test.Tests
{
    public sealed class AssemblyRetargeterTests : IDisposable
    {
        private readonly string _testAssemblyPath;

        public AssemblyRetargeterTests()
        {
            _testAssemblyPath = Path.Combine(Path.GetTempPath(), $"TestAssembly_{Guid.NewGuid()}.dll");
        }

        public void Dispose()
        {
            if (File.Exists(_testAssemblyPath))
            {
                try
                {
                    File.Delete(_testAssemblyPath);
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }
        }

        [Fact]
        public void Retarget_DuplicateMscorlib_ConsolidatesToV2()
        {
            // Arrange
            CreateTestAssembly([new Version(2, 0, 0, 0), new Version(4, 0, 0, 0)]);

            // Act
            bool result = AssemblyRetargeter.Retarget(_testAssemblyPath);

            // Assert
            _ = result.Should().BeTrue();
            VerifyMscorlibVersion(new Version(2, 0, 0, 0), 1);
        }

        [Fact]
        public void Retarget_SingleMscorlibV4_DowngradesToV2()
        {
            // Arrange
            CreateTestAssembly([new Version(4, 0, 0, 0)]);

            // Act
            bool result = AssemblyRetargeter.Retarget(_testAssemblyPath);

            // Assert
            _ = result.Should().BeTrue();
            VerifyMscorlibVersion(new Version(2, 0, 0, 0), 1);
        }

        [Fact]
        public void Retarget_CorrectMscorlibV2_NoChanges()
        {
            // Arrange
            CreateTestAssembly([new Version(2, 0, 0, 0)]);
            DateTime lastWriteTime = File.GetLastWriteTime(_testAssemblyPath);

            // Act
            bool result = AssemblyRetargeter.Retarget(_testAssemblyPath);

            // Assert
            _ = result.Should().BeFalse();

            // Ensure the file wasn't rewritten (last write time should be the same)
            _ = File.GetLastWriteTime(_testAssemblyPath).Should().Be(lastWriteTime);
        }

        [Fact]
        public void Retarget_NoMscorlib_NoChanges()
        {
            // Arrange
            CreateTestAssembly([]);

            // Act
            bool result = AssemblyRetargeter.Retarget(_testAssemblyPath);

            // Assert
            _ = result.Should().BeFalse();
        }

        [Fact]
        public void Retarget_NonExistentFile_ReturnsFalse()
        {
            // Act
            bool result = AssemblyRetargeter.Retarget("non_existent_file.dll");

            // Assert
            _ = result.Should().BeFalse();
        }

        private void CreateTestAssembly(Version[] mscorlibVersions)
        {
            using AssemblyDefinition assembly = AssemblyDefinition.CreateAssembly(
                new AssemblyNameDefinition("TestAssembly", new Version(1, 0, 0, 0)),
                "TestModule",
                ModuleKind.Dll);

            // Mono.Cecil might add a default mscorlib ref when creating assembly
            assembly.MainModule.AssemblyReferences.Clear();

            foreach (Version version in mscorlibVersions)
            {
                AssemblyNameReference mscorlibRef = new("mscorlib", version);
                assembly.MainModule.AssemblyReferences.Add(mscorlibRef);
            }

            assembly.Write(_testAssemblyPath);
        }

        private void VerifyMscorlibVersion(Version expectedVersion, int expectedCount)
        {
            using AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(_testAssemblyPath);
            List<AssemblyNameReference> mscorlibRefs = [.. assembly.MainModule.AssemblyReferences.Where(r => r.Name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase))];

            _ = mscorlibRefs.Should().HaveCount(expectedCount);
            _ = mscorlibRefs.All(r => r.Version == expectedVersion).Should().BeTrue();
        }
    }
}
