using System;
using System.IO;
using FluentAssertions;
using Moq;
using Xunit;

#pragma warning disable CA1515 // Types need to be public for testability

namespace CI.Tests;

public sealed class CiRunnerTests
{
    [Fact]
    public void Run_CheckMode_RunsDotnetFormatWithVerifyFlag()
    {
        // Arrange
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object);

        // Act
        _ = runner.Run(checkMode: true);

        // Assert
        mockProcessRunner.Verify(
            pr => pr.Run("dotnet", It.Is<string>(s => s.Contains("format --verify-no-changes")), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public void Run_FixMode_RunsDotnetFormatWithoutVerifyFlag()
    {
        // Arrange
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object);

        // Act
        _ = runner.Run(checkMode: false);

        // Assert
        mockProcessRunner.Verify(
            pr => pr.Run("dotnet", It.Is<string>(s => s.Contains("format") && !s.Contains("--verify-no-changes")), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public void Run_BothModes_RunsDotnetTest()
    {
        // Arrange
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockProcessRunner.Setup(pr => pr.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(0);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object);

        // Act
        _ = runner.Run(checkMode: true);

        // Assert
        mockProcessRunner.Verify(
            pr => pr.Run("dotnet", It.Is<string>(s => s.Contains("test")), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public void Run_FormatFails_ReturnsNonZeroExitCode()
    {
        // Arrange
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockProcessRunner.Setup(pr => pr.Run("dotnet", It.Is<string>(s => s.Contains("format")), It.IsAny<string>())).Returns(1);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object);

        // Act
        var exitCode = runner.Run(checkMode: true);

        // Assert
        exitCode.Should().Be(1);
        mockProcessRunner.Verify(
            pr => pr.Run("dotnet", It.Is<string>(s => s.Contains("test")), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public void Run_TestFails_ReturnsNonZeroExitCode()
    {
        // Arrange
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockProcessRunner.SetupSequence(pr => pr.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(0) // Format succeeds
            .Returns(1); // Test fails

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object);

        // Act
        var exitCode = runner.Run(checkMode: true);

        // Assert
        exitCode.Should().Be(1);
    }

    [Fact]
    public void Run_NoGitDirectory_ReturnsNonZeroExitCode()
    {
        // Arrange
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(false);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object);

        // Act
        var exitCode = runner.Run(checkMode: true);

        // Assert
        exitCode.Should().Be(1);
        mockProcessRunner.Verify(
            pr => pr.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public void SetupHooks_CallsGitHookSetup()
    {
        // Arrange
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object);

        // Act
        runner.SetupHooks();

        // Assert
        mockGitHookSetup.Verify(ghs => ghs.SetupPreCommitHook(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void SetupHooks_NoGitDirectory_ExitsWithCode1()
    {
        // Arrange
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(false);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object);

        // Act & Assert
        Action action = () => runner.SetupHooks();
        action.Should().Throw<InvalidOperationException>();
    }
}
