// SPDX-License-Identifier: MIT

using AwesomeAssertions;
using Xunit;

namespace SetupExternal.Tests
{
    public sealed class ProgramTests
    {
        [Fact]
        public void ParseArguments_WithPathArgument_ReturnsPath()
        {
            // Arrange
            string[] args = ["--path", "/path/to/game"];

            // Act
            (string? Path, _, _) = Program.ParseArguments(args);

            // Assert
            _ = Path.Should().Be("/path/to/game");
        }

        [Fact]
        public void ParseArguments_WithWindowsPath_ReturnsPath()
        {
            // Arrange
            string[] args = ["--path", @"C:\Games\Lobotomy Corporation"];

            // Act
            (string? Path, _, _) = Program.ParseArguments(args);

            // Assert
            _ = Path.Should().Be(@"C:\Games\Lobotomy Corporation");
        }

        [Fact]
        public void ParseArguments_WithoutPathArgument_ReturnsNull()
        {
            // Arrange
            string[] args = [];

            // Act
            (string? Path, _, _) = Program.ParseArguments(args);

            // Assert
            _ = Path.Should().BeNull();
        }

        [Fact]
        public void ParseArguments_WithOtherArguments_ReturnsNull()
        {
            // Arrange
            string[] args = ["--other", "value"];

            // Act
            (string? Path, _, _) = Program.ParseArguments(args);

            // Assert
            _ = Path.Should().BeNull();
        }

        [Fact]
        public void ParseArguments_WithPathOnly_ReturnsNull()
        {
            // Arrange
            string[] args = ["--path"];

            // Act
            (string? Path, _, _) = Program.ParseArguments(args);

            // Assert
            _ = Path.Should().BeNull();
        }

        [Fact]
        public void ParseArguments_WithPathBeforeOther_ReturnsPath()
        {
            // Arrange
            string[] args = ["--path", "/path", "--other", "value"];

            // Act
            (string? Path, _, _) = Program.ParseArguments(args);

            // Assert
            _ = Path.Should().Be("/path");
        }

        [Fact]
        public void ParseArguments_WithPathAfterOther_ReturnsPath()
        {
            // Arrange
            string[] args = ["--other", "value", "--path", "/path"];

            // Act
            (string? Path, _, _) = Program.ParseArguments(args);

            // Assert
            _ = Path.Should().Be("/path");
        }

        [Fact]
        public void ParseArguments_WithMultiplePathArguments_ReturnsFirst()
        {
            // Arrange
            string[] args = ["--path", "/first", "--path", "/second"];

            // Act
            (string? Path, _, _) = Program.ParseArguments(args);

            // Assert
            _ = Path.Should().Be("/first");
        }

        [Fact]
        public void ParseArguments_WithCaseInsensitivePath_ReturnsPath()
        {
            // Arrange
            string[] args = ["--PATH", "/path"];

            // Act
            (string? Path, _, _) = Program.ParseArguments(args);

            // Assert
            _ = Path.Should().Be("/path");
        }
    }
}
