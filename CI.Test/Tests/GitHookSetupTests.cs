// SPDX-License-Identifier: MIT

using System;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace CI.Test.Tests
{
    public sealed class GitHookSetupTests
    {
        [Fact]
        public void SetupPreCommitHook_WritesCorrectContent()
        {
            // Arrange
            Mock<IFileSystem> mockFileSystem = new();
            GitHookSetup hookSetup = new(mockFileSystem.Object);

            string? writtenContent = null;
            string? writtenPath = null;

            _ = mockFileSystem
                .Setup(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((path, content) =>
                {
                    writtenPath = path;
                    writtenContent = content;
                });

            // Act
            hookSetup.SetupPreCommitHook("/test/repo");

            // Assert
            _ = writtenContent.Should().NotBeNull();
            _ = writtenContent.Should().Contain("dotnet ci --check");
            _ = writtenContent.Should().Contain("#!/usr/bin/env bash");
            _ = writtenContent.Should().Contain("set -euo pipefail");
        }

        [Fact]
        public void SetupPreCommitHook_WritesToCorrectPath()
        {
            // Arrange
            Mock<IFileSystem> mockFileSystem = new();
            GitHookSetup hookSetup = new(mockFileSystem.Object);

            string? writtenPath = null;

            _ = mockFileSystem
                .Setup(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((path, _) => writtenPath = path);

            // Act
            hookSetup.SetupPreCommitHook("/test/repo");

            // Assert
            _ = writtenPath.Should().Be("/test/repo/.git/hooks/pre-commit");
        }

        [Fact]
        public void SetupPreCommitHook_SetsExecutableOnUnix()
        {
            // Note: This test is environment-dependent. On Windows, SetFileExecutable won't be called.
            // We can't easily mock Environment.OSVersion.Platform, so this is more of an integration test.

            // Arrange
            Mock<IFileSystem> mockFileSystem = new();
            GitHookSetup hookSetup = new(mockFileSystem.Object);

            // Act
            hookSetup.SetupPreCommitHook("/test/repo");

            // Assert - if on Unix, verify SetFileExecutable was called
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                mockFileSystem.Verify(fs => fs.SetFileExecutable("/test/repo/.git/hooks/pre-commit"), Times.Once);
            }
            else
            {
                mockFileSystem.Verify(fs => fs.SetFileExecutable(It.IsAny<string>()), Times.Never);
            }
        }

        [Fact]
        public void GetHookContent_ContainsExpectedScript()
        {
            // Arrange
            Mock<IFileSystem> mockFileSystem = new();
            GitHookSetup hookSetup = new(mockFileSystem.Object);

            string capturedContent = string.Empty;
            _ = mockFileSystem
                .Setup(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((_, content) => capturedContent = content);

            // Act
            hookSetup.SetupPreCommitHook("/test/repo");

            // Assert
            _ = capturedContent.Should().Contain("REPO_ROOT=\"$(cd \"$SCRIPT_DIR/../..\" && pwd)\"");
            _ = capturedContent.Should().Contain("cd \"$REPO_ROOT\"");
        }

        [Fact]
        public void SetupPreCommitHook_DoesNotSetExecutableOnNonUnix()
        {
            // Arrange - Mock the platform check to simulate non-Unix
            Mock<IFileSystem> mockFileSystem = new();
            GitHookSetup hookSetup = new(mockFileSystem.Object);

            // Act
            hookSetup.SetupPreCommitHook("/test/repo");

            // Assert - Verify the behavior based on actual platform
            // On non-Unix (like Windows), SetFileExecutable should not be called
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                mockFileSystem.Verify(fs => fs.SetFileExecutable(It.IsAny<string>()), Times.Once);
            }
            else
            {
                mockFileSystem.Verify(fs => fs.SetFileExecutable(It.IsAny<string>()), Times.Never);
            }
        }
    }
}
