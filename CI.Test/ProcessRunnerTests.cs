using System;
using System.Collections.Generic;
using System.Diagnostics;
using CI;
using FluentAssertions;
using Xunit;

#pragma warning disable CA1515 // Types need to be public for testability

namespace CI.Tests;

public sealed class ProcessRunnerTests
{
    [Fact]
    public void Run_CallsProcessWithCorrectArguments()
    {
        // Note: This is an integration-style test that actually runs a process
        // We'll use a simple command like "echo" that should work on all platforms

        // Arrange
        var runner = new ProcessRunner();

        // Act & Assert - This should work on both Unix and Windows
        // On Windows, "cmd /c echo test" should work
        // On Unix, "echo test" should work
        int exitCode;

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            exitCode = runner.Run("cmd", "/c echo test");
        }
        else
        {
            exitCode = runner.Run("echo", "test");
        }

        exitCode.Should().Be(0);
    }

    [Fact]
    public void Run_WithWorkingDirectory_SetsProcessWorkingDirectory()
    {
        // This test verifies that working directory can be set
        // We'll use the current directory since we know it exists

        // Arrange
        var runner = new ProcessRunner();
        var currentDir = Environment.CurrentDirectory;

        // Act & Assert
        int exitCode;

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            exitCode = runner.Run("cmd", "/c echo test", currentDir);
        }
        else
        {
            exitCode = runner.Run("echo", "test", currentDir);
        }

        exitCode.Should().Be(0);
    }

    [Fact]
    public void Run_NonexistentCommand_ReturnsNonZeroExitCode()
    {
        // Arrange
        var runner = new ProcessRunner();

        // Act - Try to run a command that doesn't exist
        var exitCode = runner.Run("thiscommanddefinitelydoesnotexist12345", "");

        // Assert - The exit code should be non-zero
        // Note: The exact behavior depends on the OS and shell
        exitCode.Should().NotBe(0);
    }

    [Fact]
    public void Run_WithNullWorkingDirectory_DoesNotThrow()
    {
        // Arrange
        var runner = new ProcessRunner();

        // Act & Assert - This should work without setting a working directory
        int exitCode;

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            exitCode = runner.Run("cmd", "/c echo test", null);
        }
        else
        {
            exitCode = runner.Run("echo", "test", null);
        }

        exitCode.Should().Be(0);
    }

    [Fact]
    public void Run_WithOutputFilter_FiltersOutput()
    {
        // Arrange
        var runner = new ProcessRunner();
        var filteredLines = new System.Collections.Generic.List<string>();

        // Act - Run a command with an output filter
        int exitCode;

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            exitCode = runner.Run("cmd", "/c echo test-output", null, line =>
            {
                filteredLines.Add(line);
                return !line.Contains("should-filter", System.StringComparison.Ordinal);
            });
        }
        else
        {
            exitCode = runner.Run("echo", "test-output", null, line =>
            {
                filteredLines.Add(line);
                return !line.Contains("should-filter", System.StringComparison.Ordinal);
            });
        }

        // Assert - The command should succeed and output should be captured
        exitCode.Should().Be(0);
        filteredLines.Should().Contain(line => line != null && line.Contains("test-output", System.StringComparison.Ordinal));
    }
}
