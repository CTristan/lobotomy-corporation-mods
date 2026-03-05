// SPDX-License-Identifier: MIT

using System;
using CI;
using FluentAssertions;
using Moq;
using Xunit;

#pragma warning disable CA1515 // Types need to be public for testability

namespace CI.Tests;

public sealed class CoverageConfigReaderTests
{
    [Fact]
    public void ReadConfig_ConfigFileExists_ReturnsConfig()
    {
        var mockFileSystem = new Mock<IFileSystem>();
        var configJson = """
            {
              "lineThreshold": 85,
              "branchThreshold": 80,
              "methodThreshold": 90
            }
            """;

        mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
        mockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).Returns(configJson);

        var reader = new CoverageConfigReader(mockFileSystem.Object);
        var config = reader.ReadConfig("/repo");

        config.Should().NotBeNull();
        config!.LineThreshold.Should().Be(85);
        config.BranchThreshold.Should().Be(80);
        config.MethodThreshold.Should().Be(90);
    }

    [Fact]
    public void ReadConfig_ConfigFileNotExists_ReturnsNull()
    {
        var mockFileSystem = new Mock<IFileSystem>();

        mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

        var reader = new CoverageConfigReader(mockFileSystem.Object);
        var config = reader.ReadConfig("/repo");

        config.Should().BeNull();
    }

    [Fact]
    public void ReadConfig_InvalidJson_ReturnsNull()
    {
        var mockFileSystem = new Mock<IFileSystem>();

        mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
        mockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).Returns("invalid json");

        var reader = new CoverageConfigReader(mockFileSystem.Object);
        var config = reader.ReadConfig("/repo");

        config.Should().BeNull();
    }

    [Fact]
    public void ReadConfig_ReadAllTextReturnsNull_ReturnsNull()
    {
        var mockFileSystem = new Mock<IFileSystem>();

        mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
        mockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).Returns((string)null);

        var reader = new CoverageConfigReader(mockFileSystem.Object);
        var config = reader.ReadConfig("/repo");

        config.Should().BeNull();
    }

    [Fact]
    public void ReadConfig_PartialConfig_UsesDefaultsForMissingValues()
    {
        var mockFileSystem = new Mock<IFileSystem>();
        var configJson = """
            {
              "lineThreshold": 90
            }
            """;

        mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
        mockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).Returns(configJson);

        var reader = new CoverageConfigReader(mockFileSystem.Object);
        var config = reader.ReadConfig("/repo");

        config.Should().NotBeNull();
        config!.LineThreshold.Should().Be(90);
        config.BranchThreshold.Should().Be(70); // default
        config.MethodThreshold.Should().Be(75); // default
    }

    [Fact]
    public void ReadConfig_EmptyConfig_UsesDefaults()
    {
        var mockFileSystem = new Mock<IFileSystem>();
        var configJson = "{}";

        mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
        mockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).Returns(configJson);

        var reader = new CoverageConfigReader(mockFileSystem.Object);
        var config = reader.ReadConfig("/repo");

        config.Should().NotBeNull();
        config!.LineThreshold.Should().Be(80); // default
        config.BranchThreshold.Should().Be(70); // default
        config.MethodThreshold.Should().Be(75); // default
    }
}
