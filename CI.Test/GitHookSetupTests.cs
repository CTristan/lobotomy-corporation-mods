#nullable enable
using System;
using CI;
using FluentAssertions;
using Moq;
using Xunit;

#pragma warning disable CA1515 // Types need to be public for testability

namespace CI.Tests;

public sealed class GitHookSetupTests
{
    [Fact]
    public void SetupPreCommitHook_WritesCorrectContent()
    {
        // Arrange
        var mockFileSystem = new Mock<IFileSystem>();
        var hookSetup = new GitHookSetup(mockFileSystem.Object);

        string? writtenContent = null;
        string? writtenPath = null;

        mockFileSystem
            .Setup(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((path, content) =>
            {
                writtenPath = path;
                writtenContent = content;
            });

        // Act
        hookSetup.SetupPreCommitHook("/test/repo");

        // Assert
        writtenContent.Should().NotBeNull();
        writtenContent.Should().Contain("dotnet ci --check");
        writtenContent.Should().Contain("#!/usr/bin/env bash");
        writtenContent.Should().Contain("set -euo pipefail");
    }

    [Fact]
    public void SetupPreCommitHook_WritesToCorrectPath()
    {
        // Arrange
        var mockFileSystem = new Mock<IFileSystem>();
        var hookSetup = new GitHookSetup(mockFileSystem.Object);

        string? writtenPath = null;

        mockFileSystem
            .Setup(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((path, _) => writtenPath = path);

        // Act
        hookSetup.SetupPreCommitHook("/test/repo");

        // Assert
        writtenPath.Should().Be("/test/repo/.git/hooks/pre-commit");
    }

    [Fact]
    public void SetupPreCommitHook_SetsExecutableOnUnix()
    {
        // Note: This test is environment-dependent. On Windows, SetFileExecutable won't be called.
        // We can't easily mock Environment.OSVersion.Platform, so this is more of an integration test.

        // Arrange
        var mockFileSystem = new Mock<IFileSystem>();
        var hookSetup = new GitHookSetup(mockFileSystem.Object);

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
        var mockFileSystem = new Mock<IFileSystem>();
        var hookSetup = new GitHookSetup(mockFileSystem.Object);

        string capturedContent = string.Empty;
        mockFileSystem
            .Setup(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((_, content) => capturedContent = content);

        // Act
        hookSetup.SetupPreCommitHook("/test/repo");

        // Assert
        capturedContent.Should().Contain("REPO_ROOT=\"$(cd \"$SCRIPT_DIR/../..\" && pwd)\"");
        capturedContent.Should().Contain("cd \"$REPO_ROOT\"");
    }
}
