// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using AwesomeAssertions;
using Xunit;

namespace SetupExternal.Tests
{
    public sealed class VdfParserTests
    {
        [Fact]
        public void ExtractLibraryPaths_WithValidVdf_ReturnsPaths()
        {
            // Arrange
            string vdfContent = """
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
            List<string> paths = VdfParser.ExtractLibraryPaths(vdfContent);

            // Assert
            _ = paths.Should().HaveCount(2);
            _ = paths.Should().Contain("C:\\\\Program Files (x86)\\\\Steam");
            _ = paths.Should().Contain("D:\\\\Games\\\\Steam");
        }

        [Fact]
        public void ExtractLibraryPaths_WithSinglePath_ReturnsOnePath()
        {
            // Arrange
            string vdfContent = """
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
            List<string> paths = VdfParser.ExtractLibraryPaths(vdfContent);

            // Assert
            _ = paths.Should().HaveCount(1);
            _ = paths.Should().ContainSingle(p => p == "C:\\\\Program Files (x86)\\\\Steam");
        }

        [Fact]
        public void ExtractLibraryPaths_WithEmptyContent_ReturnsEmptyList()
        {
            // Arrange
            string vdfContent = "";

            // Act
            List<string> paths = VdfParser.ExtractLibraryPaths(vdfContent);

            // Assert
            _ = paths.Should().BeEmpty();
        }

        [Fact]
        public void ExtractLibraryPaths_WithNullContent_ReturnsEmptyList()
        {
            // Arrange
            string? vdfContent = null;

            // Act
            List<string> paths = VdfParser.ExtractLibraryPaths(vdfContent);

            // Assert
            _ = paths.Should().BeEmpty();
        }

        [Fact]
        public void ExtractLibraryPaths_WithWhitespaceContent_ReturnsEmptyList()
        {
            // Arrange
            string vdfContent = "   \n\n   ";

            // Act
            List<string> paths = VdfParser.ExtractLibraryPaths(vdfContent);

            // Assert
            _ = paths.Should().BeEmpty();
        }

        [Fact]
        public void ExtractLibraryPaths_WithNoPaths_ReturnsEmptyList()
        {
            // Arrange
            string vdfContent = """
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
            List<string> paths = VdfParser.ExtractLibraryPaths(vdfContent);

            // Assert
            _ = paths.Should().BeEmpty();
        }

        [Fact]
        public void ExtractLibraryPaths_WithMalformedPath_SkipsMalformed()
        {
            // Arrange
            string vdfContent = """
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
            List<string> paths = VdfParser.ExtractLibraryPaths(vdfContent);

            // Assert
            _ = paths.Should().HaveCount(1);
            _ = paths.Should().ContainSingle(p => p == "C:\\\\Program Files (x86)\\\\Steam");
        }
    }
}
