// SPDX-License-Identifier: MIT

using System;
using System.IO;
using CI;
using FluentAssertions;
using Moq;
using Xunit;

#pragma warning disable CA1515 // Types need to be public for testability

namespace CI.Tests;

public sealed class FileSystemTests
{
    [Fact]
    public void WriteAllText_WritesToCorrectPath()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.txt");
        var fileSystem = new FileSystem();

        try
        {
            // Act
            fileSystem.WriteAllText(tempPath, "test content");

            // Assert
            File.Exists(tempPath).Should().BeTrue();
            File.ReadAllText(tempPath).Should().Be("test content");
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Fact]
    public void ReadAllText_FileExists_ReturnsContent()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.txt");
        var fileSystem = new FileSystem();

        try
        {
            File.WriteAllText(tempPath, "test content");

            // Act
            var content = fileSystem.ReadAllText(tempPath);

            // Assert
            content.Should().Be("test content");
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Fact]
    public void ReadAllText_FileDoesNotExist_ReturnsNull()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.txt");
        var fileSystem = new FileSystem();

        // Ensure file doesn't exist
        if (File.Exists(tempPath))
        {
            File.Delete(tempPath);
        }

        // Act
        var content = fileSystem.ReadAllText(tempPath);

        // Assert
        content.Should().BeNull();
    }

    [Fact]
    public void DirectoryExists_DirectoryExists_ReturnsTrue()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}");
        var fileSystem = new FileSystem();

        try
        {
            Directory.CreateDirectory(tempDir);

            // Act
            var exists = fileSystem.DirectoryExists(tempDir);

            // Assert
            exists.Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir);
            }
        }
    }

    [Fact]
    public void DirectoryExists_DirectoryDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}");
        var fileSystem = new FileSystem();

        // Ensure directory doesn't exist
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir);
        }

        // Act
        var exists = fileSystem.DirectoryExists(tempDir);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public void FileExists_FileExists_ReturnsTrue()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.txt");
        var fileSystem = new FileSystem();

        try
        {
            File.WriteAllText(tempPath, "test");

            // Act
            var exists = fileSystem.FileExists(tempPath);

            // Assert
            exists.Should().BeTrue();
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Fact]
    public void FileExists_FileDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.txt");
        var fileSystem = new FileSystem();

        // Ensure file doesn't exist
        if (File.Exists(tempPath))
        {
            File.Delete(tempPath);
        }

        // Act
        var exists = fileSystem.FileExists(tempPath);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public void SetFileExecutable_WithMockFileSystem_VerifiesCall()
    {
        // Arrange
        var mockFileSystem = new Mock<IFileSystem>();
        var hookSetup = new GitHookSetup(mockFileSystem.Object);

        // Act
        hookSetup.SetupPreCommitHook("/test/repo");

        // Assert - On Unix, SetFileExecutable should be called; on Windows, it should not
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            mockFileSystem.Verify(fs => fs.SetFileExecutable(It.IsAny<string>()), Times.Once);
        }
    }
}
