// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using AwesomeAssertions;
using Xunit;

namespace SetupExternal.Test.Tests
{
    public sealed class VdfParserTests
    {
        [Fact]
        public void ExtractLibraryPaths_WithValidVdf_ReturnsPaths()
        {
            // Arrange
            var vdfContent = """
            "libraryfolders"
            {
                "0"
                {
                    "path"		"C:\\Program Files (x86)\\Steam"
                    "label"		""
                    "contentid"		"123456789"
                }
                "1"
                {
                    "path"		"D:\\Games\\Steam"
                    "label"		"Games"
                    "contentid"		"987654321"
                }
            }
            """;

            // Act
            var paths = VdfParser.ExtractLibraryPaths(vdfContent);

            // Assert
            _ = paths.Should().HaveCount(2);
            _ = paths.Should().Contain("C:\\\\Program Files (x86)\\\\Steam");
            _ = paths.Should().Contain("D:\\\\Games\\\\Steam");
        }

        [Fact]
        public void ExtractLibraryPaths_WithSinglePath_ReturnsOnePath()
        {
            // Arrange
            var vdfContent = """
            "libraryfolders"
            {
                "0"
                {
                    "path"		"C:\\Program Files (x86)\\Steam"
                    "label"		""
                }
            }
            """;

            // Act
            var paths = VdfParser.ExtractLibraryPaths(vdfContent);

            // Assert
            _ = paths.Should().HaveCount(1);
            _ = paths.Should().ContainSingle(p => p == "C:\\\\Program Files (x86)\\\\Steam");
        }

        [Fact]
        public void ExtractLibraryPaths_WithEmptyContent_ReturnsEmptyList()
        {
            // Arrange
            var vdfContent = "";

            // Act
            var paths = VdfParser.ExtractLibraryPaths(vdfContent);

            // Assert
            _ = paths.Should().BeEmpty();
        }

        [Fact]
        public void ExtractLibraryPaths_WithNullContent_ReturnsEmptyList()
        {
            // Arrange
            string? vdfContent = null;

            // Act
            var paths = VdfParser.ExtractLibraryPaths(vdfContent);

            // Assert
            _ = paths.Should().BeEmpty();
        }

        [Fact]
        public void ExtractLibraryPaths_WithWhitespaceContent_ReturnsEmptyList()
        {
            // Arrange
            var vdfContent = "   \n\n   ";

            // Act
            var paths = VdfParser.ExtractLibraryPaths(vdfContent);

            // Assert
            _ = paths.Should().BeEmpty();
        }

        [Fact]
        public void ExtractLibraryPaths_WithNoPaths_ReturnsEmptyList()
        {
            // Arrange
            var vdfContent = """
            "libraryfolders"
            {
                "0"
                {
                    "label"		"Steam"
                    "contentid"		"123456789"
                }
            }
            """;

            // Act
            var paths = VdfParser.ExtractLibraryPaths(vdfContent);

            // Assert
            _ = paths.Should().BeEmpty();
        }

        [Fact]
        public void ExtractLibraryPaths_WithMalformedPath_SkipsMalformed()
        {
            // Arrange
            var vdfContent = """
            "libraryfolders"
            {
                "0"
                {
                    "path"		"C:\\Program Files (x86)\\Steam"
                    "label"		""
                }
                "1"
                {
                    "notpath"		"D:\\Games\\Steam"
                }
                "2"
                {
                    "path"		""
                }
            }
            """;

            // Act
            var paths = VdfParser.ExtractLibraryPaths(vdfContent);

            // Assert
            _ = paths.Should().HaveCount(1);
            _ = paths.Should().ContainSingle(p => p == "C:\\\\Program Files (x86)\\\\Steam");
        }
    }
}
