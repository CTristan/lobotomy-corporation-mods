// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyPlaywright.Infrastructure;
using Xunit;

namespace LobotomyPlaywright.Tests.Infrastructure
{
    /// <summary>
    /// Tests for VdfParser.
    /// </summary>
    public sealed class VdfParserTests
    {
        /// <summary>
        /// Tests ExtractLibraryPaths with valid VDF returns paths.
        /// </summary>
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
            IReadOnlyList<string> paths = VdfParser.ExtractLibraryPaths(vdfContent);

            // Assert
            _ = paths.Should().HaveCount(2);
            _ = paths.Should().Contain(@"C:\Program Files (x86)\Steam");
            _ = paths.Should().Contain(@"D:\Games\Steam");
        }

        /// <summary>
        /// Tests ExtractLibraryPaths with empty content returns empty list.
        /// </summary>
        [Fact]
        public void ExtractLibraryPaths_WithEmptyContent_ReturnsEmptyList()
        {
            // Arrange
            var vdfContent = "";

            // Act
            IReadOnlyList<string> paths = VdfParser.ExtractLibraryPaths(vdfContent);

            // Assert
            _ = paths.Should().BeEmpty();
        }

        /// <summary>
        /// Tests ExtractLibraryPaths with null content returns empty list.
        /// </summary>
        [Fact]
        public void ExtractLibraryPaths_WithNullContent_ReturnsEmptyList()
        {
            // Act
            IReadOnlyList<string> paths = VdfParser.ExtractLibraryPaths(null!);

            // Assert
            _ = paths.Should().BeEmpty();
        }

        /// <summary>
        /// Tests ExtractLibraryPaths without path entries returns empty list.
        /// </summary>
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
            IReadOnlyList<string> paths = VdfParser.ExtractLibraryPaths(vdfContent);

            // Assert
            _ = paths.Should().BeEmpty();
        }
    }
}
