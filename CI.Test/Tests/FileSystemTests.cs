// SPDX-License-Identifier: MIT

using System;
using System.IO;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace CI.Test.Tests
{
    public sealed class FileSystemTests
    {
        [Fact]
        public void WriteAllText_WritesToCorrectPath()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.txt");
            FileSystem fileSystem = new();

            try
            {
                // Act
                fileSystem.WriteAllText(tempPath, "test content");

                // Assert
                _ = File.Exists(tempPath).Should().BeTrue();
                _ = File.ReadAllText(tempPath).Should().Be("test content");
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
            FileSystem fileSystem = new();

            try
            {
                File.WriteAllText(tempPath, "test content");

                // Act
                var content = fileSystem.ReadAllText(tempPath);

                // Assert
                _ = content.Should().Be("test content");
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
            FileSystem fileSystem = new();

            // Ensure file doesn't exist
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            // Act
            var content = fileSystem.ReadAllText(tempPath);

            // Assert
            _ = content.Should().BeNull();
        }

        [Fact]
        public void DirectoryExists_DirectoryExists_ReturnsTrue()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}");
            FileSystem fileSystem = new();

            try
            {
                _ = Directory.CreateDirectory(tempDir);

                // Act
                var exists = fileSystem.DirectoryExists(tempDir);

                // Assert
                _ = exists.Should().BeTrue();
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
            FileSystem fileSystem = new();

            // Ensure directory doesn't exist
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir);
            }

            // Act
            var exists = fileSystem.DirectoryExists(tempDir);

            // Assert
            _ = exists.Should().BeFalse();
        }

        [Fact]
        public void FileExists_FileExists_ReturnsTrue()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.txt");
            FileSystem fileSystem = new();

            try
            {
                File.WriteAllText(tempPath, "test");

                // Act
                var exists = fileSystem.FileExists(tempPath);

                // Assert
                _ = exists.Should().BeTrue();
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
            FileSystem fileSystem = new();

            // Ensure file doesn't exist
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            // Act
            var exists = fileSystem.FileExists(tempPath);

            // Assert
            _ = exists.Should().BeFalse();
        }

        [Fact]
        public void SetFileExecutable_WithMockFileSystem_VerifiesCall()
        {
            // Arrange
            Mock<IFileSystem> mockFileSystem = new();
            GitHookSetup hookSetup = new(mockFileSystem.Object);

            // Act
            hookSetup.SetupPreCommitHook("/test/repo");

            // Assert - On Unix, SetFileExecutable should be called; on Windows, it should not
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                mockFileSystem.Verify(fs => fs.SetFileExecutable(It.IsAny<string>()), Times.Once);
            }
        }
    }
}
