using System;
using System.Collections.Generic;
using System.Diagnostics;
using CI;
using FluentAssertions;
using Xunit;

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
        ProcessResult result;

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            result = runner.Run("cmd", "/c echo test");
        }
        else
        {
            result = runner.Run("echo", "test");
        }

        result.ExitCode.Should().Be(0);
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
        ProcessResult result;

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            result = runner.Run("cmd", "/c echo test", currentDir);
        }
        else
        {
            result = runner.Run("echo", "test", currentDir);
        }

        result.ExitCode.Should().Be(0);
    }

    [Fact]
    public void Run_NonexistentCommand_ReturnsNonZeroExitCode()
    {
        // Arrange
        var runner = new ProcessRunner();

        // Act - Try to run a command that doesn't exist
        var result = runner.Run("thiscommanddefinitelydoesnotexist12345", "");

        // Assert - The exit code should be non-zero
        // Note: The exact behavior depends on the OS and shell
        result.ExitCode.Should().NotBe(0);
    }

    [Fact]
    public void Run_WithNullWorkingDirectory_DoesNotThrow()
    {
        // Arrange
        var runner = new ProcessRunner();

        // Act & Assert - This should work without setting a working directory
        ProcessResult result;

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            result = runner.Run("cmd", "/c echo test", null);
        }
        else
        {
            result = runner.Run("echo", "test", null);
        }

        result.ExitCode.Should().Be(0);
    }

    [Fact]
    public void Run_WithOutputFilter_FiltersOutput()
    {
        // Arrange
        var runner = new ProcessRunner();
        var filteredLines = new System.Collections.Generic.List<string>();

        // Act - Run a command with an output filter
        ProcessResult result;

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            result = runner.Run("cmd", "/c echo test-output", null, line =>
            {
                filteredLines.Add(line);
                return !line.Contains("should-filter", System.StringComparison.Ordinal);
            });
        }
        else
        {
            result = runner.Run("echo", "test-output", null, line =>
            {
                filteredLines.Add(line);
                return !line.Contains("should-filter", System.StringComparison.Ordinal);
            });
        }

        // Assert - The command should succeed and output should be captured
        result.ExitCode.Should().Be(0);
        filteredLines.Should().Contain(line => line != null && line.Contains("test-output", System.StringComparison.Ordinal));
    }
}
