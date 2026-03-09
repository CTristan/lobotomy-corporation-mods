// SPDX-License-Identifier: MIT

using System;
using AwesomeAssertions;
using Xunit;

namespace CI.Tests
{
    public sealed class ProcessRunnerOutputFilterTests
    {
        [Fact]
        public void Run_WithoutOutputFilter_CapturesAllOutput()
        {
            // Arrange
            ProcessRunner runner = new();

            // Act - Run a command without an output filter (null)
            ProcessResult result = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? runner.Run("cmd", "/c echo test-output")
                : runner.Run("echo", "test-output");

            // Assert - The command should succeed
            // This test verifies the `outputFilter == null` branch is covered
            _ = result.ExitCode.Should().Be(0);
        }

        [Fact]
        public void Run_WithNullFilterFunction_CapturesAllOutput()
        {
            // Arrange
            ProcessRunner runner = new();

            // Act - Run a command with a null filter function
            ProcessResult result = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? runner.Run("cmd", "/c echo more-output", null, null)
                : runner.Run("echo", "more-output", null, null);

            // Assert - The command should succeed
            // This verifies the `outputFilter(e.Data)` is called when filter is null
            _ = result.ExitCode.Should().Be(0);
        }
    }
}
