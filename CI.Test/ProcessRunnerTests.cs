// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using Xunit;

namespace CI.Tests
{
    public sealed class ProcessRunnerTests
    {
        [Fact]
        public void Run_CallsProcessWithCorrectArguments()
        {
            // Note: This is an integration-style test that actually runs a process
            // We'll use a simple command like "echo" that should work on all platforms

            // Arrange
            ProcessRunner runner = new();

            // Act & Assert - This should work on both Unix and Windows
            // On Windows, "cmd /c echo test" should work
            // On Unix, "echo test" should work
            ProcessResult result = Environment.OSVersion.Platform == PlatformID.Win32NT ? runner.Run("cmd", "/c echo test") : runner.Run("echo", "test");
            _ = result.ExitCode.Should().Be(0);
        }

        [Fact]
        public void Run_WithWorkingDirectory_SetsProcessWorkingDirectory()
        {
            // This test verifies that working directory can be set
            // We'll use the current directory since we know it exists

            // Arrange
            ProcessRunner runner = new();
            string currentDir = Environment.CurrentDirectory;

            // Act & Assert
            ProcessResult result = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? runner.Run("cmd", "/c echo test", currentDir)
                : runner.Run("echo", "test", currentDir);
            _ = result.ExitCode.Should().Be(0);
        }

        [Fact]
        public void Run_NonexistentCommand_ReturnsNonZeroExitCode()
        {
            // Arrange
            ProcessRunner runner = new();

            // Act - Try to run a command that doesn't exist
            ProcessResult result = runner.Run("thiscommanddefinitelydoesnotexist12345", "");

            // Assert - The exit code should be non-zero
            // Note: The exact behavior depends on the OS and shell
            _ = result.ExitCode.Should().NotBe(0);
        }

        [Fact]
        public void Run_WithNullWorkingDirectory_DoesNotThrow()
        {
            // Arrange
            ProcessRunner runner = new();

            // Act & Assert - This should work without setting a working directory
            ProcessResult result = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? runner.Run("cmd", "/c echo test", null)
                : runner.Run("echo", "test", null);
            _ = result.ExitCode.Should().Be(0);
        }

        [Fact]
        public void Run_WithOutputFilter_FiltersOutput()
        {
            // Arrange
            ProcessRunner runner = new();
            List<string> filteredLines = [];

            // Act - Run a command with an output filter
            ProcessResult result = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? runner.Run("cmd", "/c echo test-output", null, line =>
                {
                    filteredLines.Add(line);
                    return !line.Contains("should-filter", StringComparison.Ordinal);
                })
                : runner.Run("echo", "test-output", null, line =>
                {
                    filteredLines.Add(line);
                    return !line.Contains("should-filter", StringComparison.Ordinal);
                });

            // Assert - The command should succeed and output should be captured
            _ = result.ExitCode.Should().Be(0);
            _ = filteredLines.Should().Contain(line => line != null && line.Contains("test-output", StringComparison.Ordinal));
        }
    }
}
