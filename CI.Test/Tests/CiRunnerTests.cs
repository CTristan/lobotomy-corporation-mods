// SPDX-License-Identifier: MIT

using System;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace CI.Test.Tests
{
    public sealed class CiRunnerTests
    {
        private static ProcessResult Success()
        {
            return new ProcessResult { ExitCode = 0 };
        }

        private static ProcessResult Failure(params string[] errorLines)
        {
            return new ProcessResult { ExitCode = 1, ErrorLines = [.. errorLines] };
        }

        [Fact]
        public void Run_CheckMode_RunsDotnetFormatWithVerifyFlag()
        {
            Mock<IProcessRunner> mockProcessRunner = new();
            Mock<IFileSystem> mockFileSystem = new();
            Mock<IGitHookSetup> mockGitHookSetup = new();
            Mock<ICoverageThresholdChecker> mockCoverageThresholdChecker = new();

            _ = mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
            _ = mockProcessRunner
                .Setup(pr => pr.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string?, bool>>()))
                .Returns(Success());
            _ = mockCoverageThresholdChecker.Setup(c => c.CheckThresholds(It.IsAny<string>(), out It.Ref<string>.IsAny))
                .Returns(true);

            CiRunner runner = new(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object, mockCoverageThresholdChecker.Object, null);

            _ = runner.Run(checkMode: true);

            mockProcessRunner.Verify(
                pr => pr.Run(
                    "dotnet",
                    It.Is<string>(s => s.Contains("format --verify-no-changes", StringComparison.Ordinal)),
                    It.IsAny<string>(),
                    It.IsAny<Func<string?, bool>>()),
                Times.Once);
        }

        [Fact]
        public void Run_FixMode_RunsDotnetFormatWithoutVerifyFlag()
        {
            Mock<IProcessRunner> mockProcessRunner = new();
            Mock<IFileSystem> mockFileSystem = new();
            Mock<IGitHookSetup> mockGitHookSetup = new();
            Mock<ICoverageThresholdChecker> mockCoverageThresholdChecker = new();

            _ = mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
            _ = mockProcessRunner
                .Setup(pr => pr.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string?, bool>>()))
                .Returns(Success());
            _ = mockCoverageThresholdChecker.Setup(c => c.CheckThresholds(It.IsAny<string>(), out It.Ref<string>.IsAny))
                .Returns(true);

            CiRunner runner = new(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object, mockCoverageThresholdChecker.Object, null);

            _ = runner.Run(checkMode: false);

            mockProcessRunner.Verify(
                pr => pr.Run(
                    "dotnet",
                    It.Is<string>(s => s.Contains("format", StringComparison.Ordinal) && !s.Contains("--verify-no-changes", StringComparison.Ordinal)),
                    It.IsAny<string>(),
                    It.IsAny<Func<string?, bool>>()),
                Times.Once);
        }

        [Fact]
        public void Run_BothModes_RunsDotnetTest()
        {
            Mock<IProcessRunner> mockProcessRunner = new();
            Mock<IFileSystem> mockFileSystem = new();
            Mock<IGitHookSetup> mockGitHookSetup = new();
            Mock<ICoverageThresholdChecker> mockCoverageThresholdChecker = new();

            _ = mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
            _ = mockProcessRunner
                .Setup(pr => pr.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string?, bool>>()))
                .Returns(Success());
            _ = mockCoverageThresholdChecker.Setup(c => c.CheckThresholds(It.IsAny<string>(), out It.Ref<string>.IsAny))
                .Returns(true);

            CiRunner runner = new(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object, mockCoverageThresholdChecker.Object, null);

            _ = runner.Run(checkMode: true);

            mockProcessRunner.Verify(
                pr => pr.Run("dotnet", It.Is<string>(s => s.Contains("test", StringComparison.Ordinal)), It.IsAny<string>(), It.IsAny<Func<string?, bool>>()),
                Times.Once);
        }

        [Fact]
        public void Run_FormatFails_ReturnsNonZeroExitCode()
        {
            Mock<IProcessRunner> mockProcessRunner = new();
            Mock<IFileSystem> mockFileSystem = new();
            Mock<IGitHookSetup> mockGitHookSetup = new();
            Mock<ICoverageThresholdChecker> mockCoverageThresholdChecker = new();

            _ = mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
            _ = mockProcessRunner
                .Setup(pr => pr.Run("dotnet", It.Is<string>(s => s.Contains("format", StringComparison.Ordinal)), It.IsAny<string>(), It.IsAny<Func<string?, bool>>()))
                .Returns(Failure("error: formatting issue"));

            CiRunner runner = new(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object, mockCoverageThresholdChecker.Object, null);

            var exitCode = runner.Run(checkMode: true);

            _ = exitCode.Should().Be(1);
            mockProcessRunner.Verify(
                pr => pr.Run("dotnet", It.Is<string>(s => s.Contains("test", StringComparison.Ordinal)), It.IsAny<string>(), It.IsAny<Func<string?, bool>>()),
                Times.Never);
        }

        [Fact]
        public void Run_TestFails_ReturnsNonZeroExitCode()
        {
            Mock<IProcessRunner> mockProcessRunner = new();
            Mock<IFileSystem> mockFileSystem = new();
            Mock<IGitHookSetup> mockGitHookSetup = new();
            Mock<ICoverageThresholdChecker> mockCoverageThresholdChecker = new();

            _ = mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
            _ = mockProcessRunner
                .SetupSequence(pr => pr.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string?, bool>>()))
                .Returns(Success())
                .Returns(Failure("error CS1234: some build error"));

            CiRunner runner = new(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object, mockCoverageThresholdChecker.Object, null);

            var exitCode = runner.Run(checkMode: true);

            _ = exitCode.Should().Be(1);
        }

        [Fact]
        public void Run_NoGitDirectory_ReturnsNonZeroExitCode()
        {
            Mock<IProcessRunner> mockProcessRunner = new();
            Mock<IFileSystem> mockFileSystem = new();
            Mock<IGitHookSetup> mockGitHookSetup = new();
            Mock<ICoverageThresholdChecker> mockCoverageThresholdChecker = new();

            _ = mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(false);

            CiRunner runner = new(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object, mockCoverageThresholdChecker.Object, null);

            var exitCode = runner.Run(checkMode: true);

            _ = exitCode.Should().Be(1);
            mockProcessRunner.Verify(
                pr => pr.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string?, bool>>()),
                Times.Never);
        }

        [Fact]
        public void Run_ThresholdsMet_ReturnsZeroExitCode()
        {
            Mock<IProcessRunner> mockProcessRunner = new();
            Mock<IFileSystem> mockFileSystem = new();
            Mock<IGitHookSetup> mockGitHookSetup = new();
            Mock<ICoverageThresholdChecker> mockCoverageThresholdChecker = new();

            _ = mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
            _ = mockProcessRunner
                .Setup(pr => pr.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string?, bool>>()))
                .Returns(Success());
            mockCoverageThresholdChecker.Setup(c => c.CheckThresholds(It.IsAny<string>(), out It.Ref<string>.IsAny))
                .Returns(true)
                .Verifiable();

            CiRunner runner = new(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object, mockCoverageThresholdChecker.Object, null);

            var exitCode = runner.Run(checkMode: true);

            _ = exitCode.Should().Be(0);
            mockCoverageThresholdChecker.Verify();
        }

        [Fact]
        public void Run_ThresholdsNotMet_ReturnsNonZeroExitCode()
        {
            Mock<IProcessRunner> mockProcessRunner = new();
            Mock<IFileSystem> mockFileSystem = new();
            Mock<IGitHookSetup> mockGitHookSetup = new();
            Mock<ICoverageThresholdChecker> mockCoverageThresholdChecker = new();

            _ = mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
            _ = mockProcessRunner
                .Setup(pr => pr.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string?, bool>>()))
                .Returns(Success());
            mockCoverageThresholdChecker.Setup(c => c.CheckThresholds(It.IsAny<string>(), out It.Ref<string>.IsAny))
                .Returns(false)
                .Verifiable();

            CiRunner runner = new(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object, mockCoverageThresholdChecker.Object, null);

            var exitCode = runner.Run(checkMode: true);

            _ = exitCode.Should().Be(1);
            mockCoverageThresholdChecker.Verify();
        }

        [Fact]
        public void SetupHooks_CallsGitHookSetup()
        {
            Mock<IProcessRunner> mockProcessRunner = new();
            Mock<IFileSystem> mockFileSystem = new();
            Mock<IGitHookSetup> mockGitHookSetup = new();
            Mock<ICoverageThresholdChecker> mockCoverageThresholdChecker = new();

            _ = mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

            CiRunner runner = new(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object, mockCoverageThresholdChecker.Object, null);

            runner.SetupHooks();

            mockGitHookSetup.Verify(ghs => ghs.SetupPreCommitHook(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void SetupHooks_NoGitDirectory_ExitsWithCode1()
        {
            Mock<IProcessRunner> mockProcessRunner = new();
            Mock<IFileSystem> mockFileSystem = new();
            Mock<IGitHookSetup> mockGitHookSetup = new();
            Mock<ICoverageThresholdChecker> mockCoverageThresholdChecker = new();

            _ = mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(false);

            CiRunner runner = new(mockProcessRunner.Object, mockGitHookSetup.Object, mockFileSystem.Object, mockCoverageThresholdChecker.Object, null);

            Action action = runner.SetupHooks;
            _ = action.Should().Throw<InvalidOperationException>();
        }
    }
}
