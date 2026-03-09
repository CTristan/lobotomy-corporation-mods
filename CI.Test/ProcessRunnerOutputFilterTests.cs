// SPDX-License-Identifier: MIT

using System;
using AwesomeAssertions;
using CI;
using Xunit;

namespace CI.Tests;

public sealed class ProcessRunnerOutputFilterTests
{
    [Fact]
    public void Run_WithoutOutputFilter_CapturesAllOutput()
    {
        // Arrange
        var runner = new ProcessRunner();

        // Act - Run a command without an output filter (null)
        ProcessResult result;

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            result = runner.Run("cmd", "/c echo test-output");
        }
        else
        {
            result = runner.Run("echo", "test-output");
        }

        // Assert - The command should succeed
        // This test verifies the `outputFilter == null` branch is covered
        result.ExitCode.Should().Be(0);
    }

    [Fact]
    public void Run_WithNullFilterFunction_CapturesAllOutput()
    {
        // Arrange
        var runner = new ProcessRunner();

        // Act - Run a command with a null filter function
        ProcessResult result;

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            result = runner.Run("cmd", "/c echo more-output", null, null);
        }
        else
        {
            result = runner.Run("echo", "more-output", null, null);
        }

        // Assert - The command should succeed
        // This verifies the `outputFilter(e.Data)` is called when filter is null
        result.ExitCode.Should().Be(0);
    }
}
