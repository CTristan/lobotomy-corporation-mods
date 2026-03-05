using System;
using System.Collections.Generic;
using FluentAssertions;
using SetupExternal;
using Xunit;

namespace SetupExternal.Tests;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
#pragma warning disable CA1515 // Types need to be public for testability

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
        paths.Should().HaveCount(2);
        paths.Should().Contain("C:\\\\Program Files (x86)\\\\Steam");
        paths.Should().Contain("D:\\\\Games\\\\Steam");
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
        paths.Should().HaveCount(1);
        paths.Should().ContainSingle(p => p == "C:\\\\Program Files (x86)\\\\Steam");
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
        // Arrange
        string vdfContent = null;

        // Act
        var paths = VdfParser.ExtractLibraryPaths(vdfContent);

        // Assert
        paths.Should().BeEmpty();
    }

    [Fact]
    public void ExtractLibraryPaths_WithWhitespaceContent_ReturnsEmptyList()
    {
        // Arrange
        var vdfContent = "   \n\n   ";

        // Act
        var paths = VdfParser.ExtractLibraryPaths(vdfContent);

        // Assert
        paths.Should().BeEmpty();
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
        paths.Should().BeEmpty();
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
        paths.Should().HaveCount(1);
        paths.Should().ContainSingle(p => p == "C:\\\\Program Files (x86)\\\\Steam");
    }
}
