// SPDX-License-Identifier: MIT

#region

using System;
using System.IO;
using System.Linq;
using AwesomeAssertions;
using Hemocode.DebugPanel.Implementations;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class PeAssemblyRefReaderTests
    {
        private readonly PeAssemblyRefReader _reader;

        public PeAssemblyRefReaderTests()
        {
            _reader = new PeAssemblyRefReader();
        }

        [Fact]
        public void ReadAssemblyReferences_throws_when_peBytes_is_null()
        {
            Action act = () => _reader.ReadAssemblyReferences(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("peBytes");
        }

        [Fact]
        public void ReadAssemblyReferences_returns_empty_for_short_bytes()
        {
            var result = _reader.ReadAssemblyReferences(new byte[10]);

            result.Should().BeEmpty();
        }

        [Fact]
        public void ReadAssemblyReferences_returns_empty_for_non_pe_bytes()
        {
            var result = _reader.ReadAssemblyReferences(new byte[256]);

            result.Should().BeEmpty();
        }

        [Fact]
        public void ReadAssemblyReferences_returns_empty_for_empty_array()
        {
            var result = _reader.ReadAssemblyReferences([]);

            result.Should().BeEmpty();
        }

        [Fact]
        public void ReadAssemblyReferences_finds_all_references_in_valid_assembly()
        {
            var peBytes = PeTestHelper.BuildAssemblyWithRefs("0Harmony", "mscorlib", "UnityEngine");

            var result = _reader.ReadAssemblyReferences(peBytes);

            result.Should().HaveCount(3);
            result.Should().Contain("0Harmony");
            result.Should().Contain("mscorlib");
            result.Should().Contain("UnityEngine");
        }

        [Fact]
        public void ReadAssemblyReferences_returns_empty_for_assembly_with_no_references()
        {
            var peBytes = PeTestHelper.BuildAssemblyWithRefs();

            var result = _reader.ReadAssemblyReferences(peBytes);

            result.Should().BeEmpty();
        }

        [Fact]
        public void ReadAssemblyReferences_finds_12Harmony_when_it_is_an_actual_reference()
        {
            var peBytes = PeTestHelper.BuildAssemblyWithRefs("12Harmony");

            var result = _reader.ReadAssemblyReferences(peBytes);

            result.Should().ContainSingle().Which.Should().Be("12Harmony");
        }

        [Fact]
        public void ReadAssemblyReferences_finds_0Harmony12_when_it_is_an_actual_reference()
        {
            var peBytes = PeTestHelper.BuildAssemblyWithRefs("0Harmony12");

            var result = _reader.ReadAssemblyReferences(peBytes);

            result.Should().ContainSingle().Which.Should().Be("0Harmony12");
        }

        [Fact]
        public void ReadAssemblyReferences_does_not_find_string_constants_as_references()
        {
            // Build an assembly with only mscorlib as a reference.
            // Even if the DLL bytes happen to contain "12Harmony" as a string constant
            // elsewhere in the binary, the PE reader should NOT report it.
            var peBytes = PeTestHelper.BuildAssemblyWithRefs("mscorlib");

            var result = _reader.ReadAssemblyReferences(peBytes);

            result.Should().ContainSingle().Which.Should().Be("mscorlib");
            result.Should().NotContain("12Harmony");
            result.Should().NotContain("0Harmony");
        }

        [Fact]
        public void ReadAssemblyReferences_preserves_reference_order()
        {
            var peBytes = PeTestHelper.BuildAssemblyWithRefs("Alpha", "Beta", "Gamma");

            var result = _reader.ReadAssemblyReferences(peBytes);

            result.Should().HaveCount(3);
            result[0].Should().Be("Alpha");
            result[1].Should().Be("Beta");
            result[2].Should().Be("Gamma");
        }

        [Fact]
        public void ReadAssemblyReferences_correctly_parses_real_dotnet_assembly()
        {
            var assembly = typeof(PeAssemblyRefReader).Assembly;
            var bytes = File.ReadAllBytes(assembly.Location);

            var result = _reader.ReadAssemblyReferences(bytes);

            result.Should().NotBeEmpty();
            result.Should().Contain("mscorlib");
        }

        [Theory]
        [MemberData(nameof(GetRealAssemblyPaths))]
        public void ReadAssemblyReferences_handles_various_real_assemblies(string assemblyPath)
        {
            var bytes = File.ReadAllBytes(assemblyPath);

            var result = _reader.ReadAssemblyReferences(bytes);

            result.Should().NotBeNull();
        }

        public static TheoryData<string> GetRealAssemblyPaths()
        {
            var data = new TheoryData<string>();
            var assemblies = new[]
            {
                typeof(PeAssemblyRefReader).Assembly,
                typeof(object).Assembly,
                typeof(FactAttribute).Assembly,
                typeof(System.ComponentModel.INotifyPropertyChanged).Assembly,
                typeof(Enumerable).Assembly,
                typeof(System.Runtime.InteropServices.Marshal).Assembly,
                typeof(AssertionExtensions).Assembly,
                typeof(Moq.Mock).Assembly,
            };

            foreach (var asm in assemblies)
            {
                if (!string.IsNullOrEmpty(asm.Location) && File.Exists(asm.Location))
                {
                    data.Add(asm.Location);
                }
            }

            return data;
        }

        [Fact]
        public void ReadAssemblyReferences_returns_empty_for_mz_header_with_invalid_pe_signature()
        {
            var bytes = new byte[256];
            bytes[0] = 0x4D;
            bytes[1] = 0x5A;
            bytes[0x3C] = 64;

            var result = _reader.ReadAssemblyReferences(bytes);

            result.Should().BeEmpty();
        }

        [Fact]
        public void ReadAssemblyReferences_returns_empty_for_truncated_pe()
        {
            var validPe = PeTestHelper.BuildAssemblyWithRefs("mscorlib");
            var truncated = new byte[100];
            Array.Copy(validPe, truncated, Math.Min(validPe.Length, 100));

            var result = _reader.ReadAssemblyReferences(truncated);

            result.Should().BeEmpty();
        }
    }
}
