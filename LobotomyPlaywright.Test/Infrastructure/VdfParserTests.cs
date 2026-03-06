// SPDX-License-Identifier: MIT

#nullable enable
#pragma warning disable CA1515 // Test classes must be public for xUnit

using FluentAssertions;
using LobotomyPlaywright.Infrastructure;
using Xunit;

namespace LobotomyPlaywright.Tests.Infrastructure;

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
        var paths = VdfParser.ExtractLibraryPaths(vdfContent);

        // Assert
        paths.Should().HaveCount(2);
        paths.Should().Contain(@"C:\Program Files (x86)\Steam");
        paths.Should().Contain(@"D:\Games\Steam");
    }

    [Fact]
    public void ExtractLibraryPaths_WithEmptyContent_ReturnsEmptyList()
    {
        // Arrange
        var vdfContent = "";

        // Act
        var paths = VdfParser.ExtractLibraryPaths(vdfContent);

        // Assert
        paths.Should().BeEmpty();
    }

    [Fact]
    public void ExtractLibraryPaths_WithNullContent_ReturnsEmptyList()
    {
        // Act
        var paths = VdfParser.ExtractLibraryPaths(null!);

        // Assert
        paths.Should().BeEmpty();
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
        var paths = VdfParser.ExtractLibraryPaths(vdfContent);

        // Assert
        paths.Should().BeEmpty();
    }
}
