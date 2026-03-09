// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using CI;
using Moq;
using Xunit;

namespace CI.Tests;

public sealed class CiRunnerTests
{
    private static ProcessResult Success()
    {
        return new ProcessResult { ExitCode = 0 };
    }

    private static ProcessResult Failure(params string[] errorLines)
    {
        return new ProcessResult { ExitCode = 1, ErrorLines = new List<string>(errorLines) };
    }

    [Fact]
    public void Run_CheckMode_RunsDotnetFormatWithVerifyFlag()
    {
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();
        var mockCoverageConfigReader = new Mock<ICoverageConfigReader>();
        var mockCoverageThresholdChecker = new Mock<ICoverageThresholdChecker>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockProcessRunner
            .Setup(pr => pr.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string, bool>>()))
            .Returns(Success());
        mockCoverageThresholdChecker.Setup(c => c.CheckThresholds(It.IsAny<string>(), out It.Ref<string>.IsAny))
            .Returns(true);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object, mockCoverageConfigReader.Object, mockCoverageThresholdChecker.Object);

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
        var mockCoverageConfigReader = new Mock<ICoverageConfigReader>();
        var mockCoverageThresholdChecker = new Mock<ICoverageThresholdChecker>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockProcessRunner
            .Setup(pr => pr.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string, bool>>()))
            .Returns(Success());
        mockCoverageThresholdChecker.Setup(c => c.CheckThresholds(It.IsAny<string>(), out It.Ref<string>.IsAny))
            .Returns(true);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object, mockCoverageConfigReader.Object, mockCoverageThresholdChecker.Object);

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
        var mockCoverageConfigReader = new Mock<ICoverageConfigReader>();
        var mockCoverageThresholdChecker = new Mock<ICoverageThresholdChecker>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockProcessRunner
            .Setup(pr => pr.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string, bool>>()))
            .Returns(Success());
        mockCoverageThresholdChecker.Setup(c => c.CheckThresholds(It.IsAny<string>(), out It.Ref<string>.IsAny))
            .Returns(true);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object, mockCoverageConfigReader.Object, mockCoverageThresholdChecker.Object);

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
        var mockCoverageConfigReader = new Mock<ICoverageConfigReader>();
        var mockCoverageThresholdChecker = new Mock<ICoverageThresholdChecker>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockProcessRunner
            .Setup(pr => pr.Run("dotnet", It.Is<string>(s => s.Contains("format", StringComparison.Ordinal)), It.IsAny<string>(), It.IsAny<Func<string, bool>>()))
            .Returns(Failure("error: formatting issue"));

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object, mockCoverageConfigReader.Object, mockCoverageThresholdChecker.Object);

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
        var mockCoverageConfigReader = new Mock<ICoverageConfigReader>();
        var mockCoverageThresholdChecker = new Mock<ICoverageThresholdChecker>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockProcessRunner
            .SetupSequence(pr => pr.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string, bool>>()))
            .Returns(Success())
            .Returns(Failure("error CS1234: some build error"));

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object, mockCoverageConfigReader.Object, mockCoverageThresholdChecker.Object);

        var exitCode = runner.Run(checkMode: true);

        exitCode.Should().Be(1);
    }

    [Fact]
    public void Run_NoGitDirectory_ReturnsNonZeroExitCode()
    {
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();
        var mockCoverageConfigReader = new Mock<ICoverageConfigReader>();
        var mockCoverageThresholdChecker = new Mock<ICoverageThresholdChecker>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(false);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object, mockCoverageConfigReader.Object, mockCoverageThresholdChecker.Object);

        var exitCode = runner.Run(checkMode: true);

        exitCode.Should().Be(1);
        mockProcessRunner.Verify(
            pr => pr.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string, bool>>()),
            Times.Never);
    }

    [Fact]
    public void Run_ThresholdsMet_ReturnsZeroExitCode()
    {
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();
        var mockCoverageConfigReader = new Mock<ICoverageConfigReader>();
        var mockCoverageThresholdChecker = new Mock<ICoverageThresholdChecker>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockProcessRunner
            .Setup(pr => pr.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string, bool>>()))
            .Returns(Success());
        mockCoverageThresholdChecker.Setup(c => c.CheckThresholds(It.IsAny<string>(), out It.Ref<string>.IsAny))
            .Returns(true)
            .Verifiable();

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object, mockCoverageConfigReader.Object, mockCoverageThresholdChecker.Object);

        var exitCode = runner.Run(checkMode: true);

        exitCode.Should().Be(0);
        mockCoverageThresholdChecker.Verify();
    }

    [Fact]
    public void Run_ThresholdsNotMet_ReturnsNonZeroExitCode()
    {
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();
        var mockCoverageConfigReader = new Mock<ICoverageConfigReader>();
        var mockCoverageThresholdChecker = new Mock<ICoverageThresholdChecker>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockProcessRunner
            .Setup(pr => pr.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string, bool>>()))
            .Returns(Success());
        mockCoverageThresholdChecker.Setup(c => c.CheckThresholds(It.IsAny<string>(), out It.Ref<string>.IsAny))
            .Returns(false)
            .Verifiable();

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object, mockCoverageConfigReader.Object, mockCoverageThresholdChecker.Object);

        var exitCode = runner.Run(checkMode: true);

        exitCode.Should().Be(1);
        mockCoverageThresholdChecker.Verify();
    }

    [Fact]
    public void SetupHooks_CallsGitHookSetup()
    {
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();
        var mockCoverageConfigReader = new Mock<ICoverageConfigReader>();
        var mockCoverageThresholdChecker = new Mock<ICoverageThresholdChecker>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object, mockCoverageConfigReader.Object, mockCoverageThresholdChecker.Object);

        runner.SetupHooks();

        mockGitHookSetup.Verify(ghs => ghs.SetupPreCommitHook(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void SetupHooks_NoGitDirectory_ExitsWithCode1()
    {
        var mockProcessRunner = new Mock<IProcessRunner>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockGitHookSetup = new Mock<IGitHookSetup>();
        var mockCoverageConfigReader = new Mock<ICoverageConfigReader>();
        var mockCoverageThresholdChecker = new Mock<ICoverageThresholdChecker>();

        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(false);

        var runner = new CiRunner(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object, mockCoverageConfigReader.Object, mockCoverageThresholdChecker.Object);

        Action action = () => runner.SetupHooks();
        action.Should().Throw<InvalidOperationException>();
    }
}
