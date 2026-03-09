// SPDX-License-Identifier: MIT

using System;
using AwesomeAssertions;
using SetupExternal;
using Xunit;

namespace SetupExternal.Tests;

public sealed class ProgramTests
{
    [Fact]
    public void ParseArguments_WithPathArgument_ReturnsPath()
    {
        // Arrange
        var args = new[] { "--path", "/path/to/game" };

        // Act
        var result = Program.ParseArguments(args);

        // Assert
        result.Path.Should().Be("/path/to/game");
    }

    [Fact]
    public void ParseArguments_WithWindowsPath_ReturnsPath()
    {
        // Arrange
        var args = new[] { "--path", @"C:\Games\Lobotomy Corporation" };

        // Act
        var result = Program.ParseArguments(args);

        // Assert
        result.Path.Should().Be(@"C:\Games\Lobotomy Corporation");
    }

    [Fact]
    public void ParseArguments_WithoutPathArgument_ReturnsNull()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var result = Program.ParseArguments(args);

        // Assert
        result.Path.Should().BeNull();
    }

    [Fact]
    public void ParseArguments_WithOtherArguments_ReturnsNull()
    {
        // Arrange
        var args = new[] { "--other", "value" };

        // Act
        var result = Program.ParseArguments(args);

        // Assert
        result.Path.Should().BeNull();
    }

    [Fact]
    public void ParseArguments_WithPathOnly_ReturnsNull()
    {
        // Arrange
        var args = new[] { "--path" };

        // Act
        var result = Program.ParseArguments(args);

        // Assert
        result.Path.Should().BeNull();
    }

    [Fact]
    public void ParseArguments_WithPathBeforeOther_ReturnsPath()
    {
        // Arrange
        var args = new[] { "--path", "/path", "--other", "value" };

        // Act
        var result = Program.ParseArguments(args);

        // Assert
        result.Path.Should().Be("/path");
    }

    [Fact]
    public void ParseArguments_WithPathAfterOther_ReturnsPath()
    {
        // Arrange
        var args = new[] { "--other", "value", "--path", "/path" };

        // Act
        var result = Program.ParseArguments(args);

        // Assert
        result.Path.Should().Be("/path");
    }

    [Fact]
    public void ParseArguments_WithMultiplePathArguments_ReturnsFirst()
    {
        // Arrange
        var args = new[] { "--path", "/first", "--path", "/second" };

        // Act
        var result = Program.ParseArguments(args);

        // Assert
        result.Path.Should().Be("/first");
    }

    [Fact]
    public void ParseArguments_WithCaseInsensitivePath_ReturnsPath()
    {
        // Arrange
        var args = new[] { "--PATH", "/path" };

        // Act
        var result = Program.ParseArguments(args);

        // Assert
        result.Path.Should().Be("/path");
    }
}
