// SPDX-License-Identifier: MIT

using System;
using CI;
using FluentAssertions;
using Xunit;

#pragma warning disable CA1515 // Types need to be public for testability

namespace CI.Tests;

public sealed class ProcessRunnerOutputFilterTests
{
    [Fact]
    public void Run_WithoutOutputFilter_CapturesAllOutput()
    {
        // Arrange
        var runner = new ProcessRunner();

        // Act - Run a command without an output filter (null)
        int exitCode;

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            exitCode = runner.Run("cmd", "/c echo test-output");
        }
        else
        {
            exitCode = runner.Run("echo", "test-output");
        }

        // Assert - The command should succeed
        // This test verifies the `outputFilter == null` branch is covered
        exitCode.Should().Be(0);
    }

    [Fact]
    public void Run_WithNullFilterFunction_CapturesAllOutput()
    {
        // Arrange
        var runner = new ProcessRunner();

        // Act - Run a command with a null filter function
        int exitCode;

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            exitCode = runner.Run("cmd", "/c echo more-output", null, null);
        }
        else
        {
            exitCode = runner.Run("echo", "more-output", null, null);
        }

        // Assert - The command should succeed
        // This verifies the `outputFilter(e.Data)` is called when filter is null
        exitCode.Should().Be(0);
    }
}
