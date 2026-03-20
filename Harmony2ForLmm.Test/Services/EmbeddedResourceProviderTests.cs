// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using AwesomeAssertions;
using Harmony2ForLmm.Services;
using Moq;
using Xunit;

namespace Harmony2ForLmm.Test.Services
{
    /// <summary>
    /// Tests for <see cref="EmbeddedResourceProvider"/>.
    /// </summary>
    public sealed class EmbeddedResourceProviderTests : IDisposable
    {
        private readonly string _tempDir;

        public EmbeddedResourceProviderTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }

        [Fact]
        public void ReadDocumentText_returns_content_for_known_resource()
        {
            var assembly = CreateMockAssembly(new Dictionary<string, byte[]>
            {
                ["Resources.docs.UsersGuide.md"] = "# User Guide"u8.ToArray(),
            });
            var provider = new EmbeddedResourceProvider(assembly);

            var result = provider.ReadDocumentText("UsersGuide.md");

            result.Should().Be("# User Guide");
        }

        [Fact]
        public void ReadDocumentText_returns_null_for_unknown_resource()
        {
            var assembly = CreateMockAssembly([]);
            var provider = new EmbeddedResourceProvider(assembly);

            var result = provider.ReadDocumentText("NonExistent.md");

            result.Should().BeNull();
        }

        [Fact]
        public void ExtractBepInExTo_creates_files_in_target_directory()
        {
            var zipBytes = CreateZipWithEntries(new Dictionary<string, byte[]>
            {
                ["winhttp.dll"] = [1, 2, 3],
                ["BepInEx/core/BepInEx.dll"] = [4, 5, 6],
            });
            var assembly = CreateMockAssembly(new Dictionary<string, byte[]>
            {
                ["Resources.bepinex.zip"] = zipBytes,
            });
            var provider = new EmbeddedResourceProvider(assembly);
            var filesWritten = new List<string>();

            provider.ExtractBepInExTo(_tempDir, filesWritten);

            File.Exists(Path.Combine(_tempDir, "winhttp.dll")).Should().BeTrue();
            File.Exists(Path.Combine(_tempDir, "BepInEx", "core", "BepInEx.dll")).Should().BeTrue();
            filesWritten.Should().HaveCount(2);
        }

        [Fact]
        public void ExtractBepInExTo_does_nothing_when_resource_is_missing()
        {
            var assembly = CreateMockAssembly([]);
            var provider = new EmbeddedResourceProvider(assembly);
            var filesWritten = new List<string>();

            provider.ExtractBepInExTo(_tempDir, filesWritten);

            filesWritten.Should().BeEmpty();
        }

        [Fact]
        public void CopyDllTo_creates_destination_file_with_correct_content()
        {
            byte[] dllContent = [7, 8, 9, 10];
            var assembly = CreateMockAssembly(new Dictionary<string, byte[]>
            {
                ["Resources.RetargetHarmony.dll"] = dllContent,
            });
            var provider = new EmbeddedResourceProvider(assembly);
            var destPath = Path.Combine(_tempDir, "RetargetHarmony.dll");
            var filesWritten = new List<string>();

            provider.CopyDllTo("RetargetHarmony.dll", destPath, filesWritten);

            File.Exists(destPath).Should().BeTrue();
            File.ReadAllBytes(destPath).Should().Equal(dllContent);
            filesWritten.Should().ContainSingle().Which.Should().Be(destPath);
        }

        [Fact]
        public void CopyDllTo_does_nothing_when_resource_is_missing()
        {
            var assembly = CreateMockAssembly([]);
            var provider = new EmbeddedResourceProvider(assembly);
            var destPath = Path.Combine(_tempDir, "Missing.dll");
            var filesWritten = new List<string>();

            provider.CopyDllTo("Missing.dll", destPath, filesWritten);

            File.Exists(destPath).Should().BeFalse();
            filesWritten.Should().BeEmpty();
        }

        [Fact]
        public void OpenDemoModZip_returns_stream_when_resource_exists()
        {
            var assembly = CreateMockAssembly(new Dictionary<string, byte[]>
            {
                ["Resources.DemoMod.zip"] = [1, 2, 3],
            });
            var provider = new EmbeddedResourceProvider(assembly);

            using var stream = provider.OpenDemoModZip();

            stream.Should().NotBeNull();
        }

        [Fact]
        public void OpenDemoModZip_returns_null_when_resource_is_missing()
        {
            var assembly = CreateMockAssembly([]);
            var provider = new EmbeddedResourceProvider(assembly);

            var stream = provider.OpenDemoModZip();

            stream.Should().BeNull();
        }

        [Fact]
        public void ExtractDebugPanelTo_creates_files_in_DebugPanel_subdirectory()
        {
            var zipBytes = CreateZipWithEntries(new Dictionary<string, byte[]>
            {
                ["DebugPanel.dll"] = [1, 2, 3],
                ["Info/en/Info.xml"] = [4, 5, 6],
                ["Data/known-issues.json"] = [7, 8, 9],
            });
            var assembly = CreateMockAssembly(new Dictionary<string, byte[]>
            {
                ["Resources.debugpanel.zip"] = zipBytes,
            });
            var provider = new EmbeddedResourceProvider(assembly);
            var filesWritten = new List<string>();

            provider.ExtractDebugPanelTo(_tempDir, filesWritten);

            File.Exists(Path.Combine(_tempDir, "DebugPanel", "DebugPanel.dll")).Should().BeTrue();
            File.Exists(Path.Combine(_tempDir, "DebugPanel", "Info", "en", "Info.xml")).Should().BeTrue();
            File.Exists(Path.Combine(_tempDir, "DebugPanel", "Data", "known-issues.json")).Should().BeTrue();
            filesWritten.Should().HaveCount(3);
        }

        [Fact]
        public void ExtractDebugPanelTo_does_nothing_when_resource_is_missing()
        {
            var assembly = CreateMockAssembly([]);
            var provider = new EmbeddedResourceProvider(assembly);
            var filesWritten = new List<string>();

            provider.ExtractDebugPanelTo(_tempDir, filesWritten);

            filesWritten.Should().BeEmpty();
        }

        private static Assembly CreateMockAssembly(Dictionary<string, byte[]> resources)
        {
            var mock = new Mock<Assembly>();
            mock.Setup(a => a.GetManifestResourceStream(It.IsAny<string>()))
                .Returns<string>(name => resources.TryGetValue(name, out var data)
                    ? new MemoryStream(data)
                    : null);

            return mock.Object;
        }

        private static byte[] CreateZipWithEntries(Dictionary<string, byte[]> entries)
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var (name, content) in entries)
                {
                    var entry = archive.CreateEntry(name);
                    using var entryStream = entry.Open();
                    entryStream.Write(content, 0, content.Length);
                }
            }

            return memoryStream.ToArray();
        }
    }
}
