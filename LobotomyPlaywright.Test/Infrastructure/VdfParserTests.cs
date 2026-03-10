// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyPlaywright.Infrastructure;
using Xunit;

namespace LobotomyPlaywright.Tests.Infrastructure
{
    public sealed class VdfParserTests
    {
        [Fact]
        public void ExtractLibraryPaths_WithValidVDF_ReturnsPaths()
        {
            // Arrange
            var vdfContent = """
"libraryfolders"
{
            "0"
  {
                "path"      "C:\Program Files (x86)\Steam"
    "label"     ""
    "contentid"     "123456789"
  }
            "1"
  {
                "path"      "D:\Games\Steam"
    "label"     "Steam Library"
  }
        }
""";

            // Act
            List<string> paths = VdfParser.ExtractLibraryPaths(vdfContent);

            // Assert
            _ = paths.Should().HaveCount(2);
            _ = paths.Should().Contain(@"C:\Program Files (x86)\Steam");
            _ = paths.Should().Contain(@"D:\Games\Steam");
        }

        [Fact]
        public void ExtractLibraryPaths_WithEmptyContent_ReturnsEmptyList()
        {
            // Arrange
            var vdfContent = "";

            // Act
            List<string> paths = VdfParser.ExtractLibraryPaths(vdfContent);

            // Assert
            _ = paths.Should().BeEmpty();
        }

        [Fact]
        public void ExtractLibraryPaths_WithNullContent_ReturnsEmptyList()
        {
            // Act
            List<string> paths = VdfParser.ExtractLibraryPaths(null!);

            // Assert
            _ = paths.Should().BeEmpty();
        }

        [Fact]
        public void ExtractLibraryPaths_WithoutPathEntries_ReturnsEmptyList()
        {
            // Arrange
            var vdfContent = """
"libraryfolders"
{
  "0"
  {
    "label"		"Steam Library"
  }
}
""";

            // Act
            List<string> paths = VdfParser.ExtractLibraryPaths(vdfContent);

            // Assert
            _ = paths.Should().BeEmpty();
        }
    }
}
