using System;
using CI;
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
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object);

        _ = runner.Run(checkMode: true);

        mockProcessRunner.Verify(
            pr => pr.Run(
                "dotnet",
                It.Is<string>(s => s.Contains("format --verify-no-changes", StringComparison.Ordinal)),
                It.IsAny<string>(),
                It.IsAny<Func<string, bool>>()),
            Times.Once);
    }

    [Fact]
    public void Run_FixMode_RunsDotnetFormatWithoutVerifyFlag()
    {
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object);

        _ = runner.Run(checkMode: false);

        mockProcessRunner.Verify(
            pr => pr.Run(
                "dotnet",
                It.Is<string>(s => s.Contains("format", StringComparison.Ordinal) && !s.Contains("--verify-no-changes", StringComparison.Ordinal)),
                It.IsAny<string>(),
                It.IsAny<Func<string, bool>>()),
            Times.Once);
    }

    [Fact]
    public void Run_BothModes_RunsDotnetTest()
    {
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockProcessRunner
            .Setup(pr => pr.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string, bool>>()))
            .Returns(0);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object);

        _ = runner.Run(checkMode: true);

        mockProcessRunner.Verify(
            pr => pr.Run("dotnet", It.Is<string>(s => s.Contains("test", StringComparison.Ordinal)), It.IsAny<string>(), It.IsAny<Func<string, bool>>()),
            Times.Once);
    }

    [Fact]
    public void Run_FormatFails_ReturnsNonZeroExitCode()
    {
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockProcessRunner
            .Setup(pr => pr.Run("dotnet", It.Is<string>(s => s.Contains("format", StringComparison.Ordinal)), It.IsAny<string>(), It.IsAny<Func<string, bool>>()))
            .Returns(1);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object);

        var exitCode = runner.Run(checkMode: true);

        exitCode.Should().Be(1);
        mockProcessRunner.Verify(
            pr => pr.Run("dotnet", It.Is<string>(s => s.Contains("test", StringComparison.Ordinal)), It.IsAny<string>(), It.IsAny<Func<string, bool>>()),
            Times.Never);
    }

    [Fact]
    public void Run_TestFails_ReturnsNonZeroExitCode()
    {
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockProcessRunner
            .SetupSequence(pr => pr.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string, bool>>()))
            .Returns(0)
            .Returns(1);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object);

        var exitCode = runner.Run(checkMode: true);

        exitCode.Should().Be(1);
    }

    [Fact]
    public void Run_NoGitDirectory_ReturnsNonZeroExitCode()
    {
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(false);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object);

        var exitCode = runner.Run(checkMode: true);

        exitCode.Should().Be(1);
        mockProcessRunner.Verify(
            pr => pr.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string, bool>>()),
            Times.Never);
    }

    [Fact]
    public void SetupHooks_CallsGitHookSetup()
    {
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object);

        runner.SetupHooks();

        mockGitHookSetup.Verify(ghs => ghs.SetupPreCommitHook(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void SetupHooks_NoGitDirectory_ExitsWithCode1()
    {
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(false);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object);

        Action action = () => runner.SetupHooks();
        action.Should().Throw<InvalidOperationException>();
    }
}
