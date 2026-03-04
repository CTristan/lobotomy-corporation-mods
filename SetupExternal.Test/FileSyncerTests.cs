using System;
using System.IO;
using System.Text;
using FluentAssertions;
using SetupExternal;
using Xunit;

namespace SetupExternal.Test;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
#pragma warning disable CA1515 // Types need to be public for testability

public sealed class FileSyncerTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _sourceDir;
    private readonly string _destDir;
    private readonly string _sourceManagedDir;
    private readonly string _destManagedDir;

    public FileSyncerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _sourceDir = Path.Combine(_tempDir, "source");
        _destDir = Path.Combine(_tempDir, "dest");
        _sourceManagedDir = Path.Combine(_sourceDir, "LobotomyCorp_Data", "Managed");
        _destManagedDir = Path.Combine(_destDir, "LobotomyCorp_Data", "Managed");

        Directory.CreateDirectory(_sourceManagedDir);
        Directory.CreateDirectory(_destManagedDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void SyncDlls_WithNewFiles_CopiesAllFiles()
    {
        // Arrange
        CreateTestFile(_sourceManagedDir, "Test1.dll", "content1");
        CreateTestFile(_sourceManagedDir, "Test2.dll", "content2");

        // Act
        var result = FileSyncer.SyncDlls(_sourceDir, _destDir);

        // Assert
        result.FilesCopied.Should().Be(2);
        result.FilesUpdated.Should().Be(0);
        result.FilesSkipped.Should().Be(0);
        result.AssemblyCSharpChanged.Should().BeFalse();

        File.Exists(Path.Combine(_destManagedDir, "Test1.dll")).Should().BeTrue();
        File.Exists(Path.Combine(_destManagedDir, "Test2.dll")).Should().BeTrue();
    }

    [Fact]
    public void SyncDlls_WithAssemblyCSharp_MarksAsChanged()
    {
        // Arrange
        CreateTestFile(_sourceManagedDir, "Assembly-CSharp.dll", "content");

        // Act
        var result = FileSyncer.SyncDlls(_sourceDir, _destDir);

        // Assert
        result.FilesCopied.Should().Be(1);
        result.AssemblyCSharpChanged.Should().BeTrue();
    }

    [Fact]
    public void SyncDlls_WithLobotomyBaseModLib_MarksAsChanged()
    {
        // Arrange
        CreateTestFile(_sourceManagedDir, "LobotomyBaseModLib.dll", "content");

        // Act
        var result = FileSyncer.SyncDlls(_sourceDir, _destDir);

        // Assert
        result.FilesCopied.Should().Be(1);
        result.LobotomyBaseModLibChanged.Should().BeTrue();
    }

    [Fact]
    public void SyncDlls_WithExistingSameHash_SkipsFiles()
    {
        // Arrange
        var content = "same content";
        CreateTestFile(_sourceManagedDir, "Test.dll", content);
        CreateTestFile(_destManagedDir, "Test.dll", content);

        // Act
        var result = FileSyncer.SyncDlls(_sourceDir, _destDir);

        // Assert
        result.FilesCopied.Should().Be(0);
        result.FilesUpdated.Should().Be(0);
        result.FilesSkipped.Should().Be(1);
    }

    [Fact]
    public void SyncDlls_WithDifferentHash_UpdatesFiles()
    {
        // Arrange
        CreateTestFile(_sourceManagedDir, "Test.dll", "new content");
        CreateTestFile(_destManagedDir, "Test.dll", "old content");

        // Act
        var result = FileSyncer.SyncDlls(_sourceDir, _destDir);

        // Assert
        result.FilesCopied.Should().Be(0);
        result.FilesUpdated.Should().Be(1);
        result.FilesSkipped.Should().Be(0);

        var destContent = File.ReadAllText(Path.Combine(_destManagedDir, "Test.dll"));
        destContent.Should().Be("new content");
    }

    [Fact]
    public void SyncDlls_WithMixedStates_HandlesCorrectly()
    {
        // Arrange
        var sameContent = "same content";
        CreateTestFile(_sourceManagedDir, "Same.dll", sameContent);
        CreateTestFile(_destManagedDir, "Same.dll", sameContent);

        CreateTestFile(_sourceManagedDir, "New.dll", "new content");

        CreateTestFile(_sourceManagedDir, "Modified.dll", "modified");
        CreateTestFile(_destManagedDir, "Modified.dll", "original");

        CreateTestFile(_destManagedDir, "Extra.dll", "extra");

        // Act
        var result = FileSyncer.SyncDlls(_sourceDir, _destDir);

        // Assert
        result.FilesCopied.Should().Be(1); // New.dll
        result.FilesUpdated.Should().Be(1); // Modified.dll
        result.FilesSkipped.Should().Be(1); // Same.dll
    }

    [Fact]
    public void SyncDlls_WithNoDllFiles_DoesNothing()
    {
        // Arrange - no DLL files created

        // Act
        var result = FileSyncer.SyncDlls(_sourceDir, _destDir);

        // Assert
        result.FilesCopied.Should().Be(0);
        result.FilesUpdated.Should().Be(0);
        result.FilesSkipped.Should().Be(0);
    }

    [Fact]
    public void SyncDlls_WithNonDllFiles_IgnoresThem()
    {
        // Arrange
        CreateTestFile(_sourceManagedDir, "Test.txt", "text file");
        CreateTestFile(_sourceManagedDir, "Test.exe", "exe file");

        // Act
        var result = FileSyncer.SyncDlls(_sourceDir, _destDir);

        // Assert
        result.FilesCopied.Should().Be(0);
        result.FilesUpdated.Should().Be(0);
        result.FilesSkipped.Should().Be(0);

        File.Exists(Path.Combine(_destManagedDir, "Test.txt")).Should().BeFalse();
        File.Exists(Path.Combine(_destManagedDir, "Test.exe")).Should().BeFalse();
    }

    private static void CreateTestFile(string dir, string filename, string content)
    {
        var path = Path.Combine(dir, filename);
        File.WriteAllText(path, content, Encoding.UTF8);
    }
}
