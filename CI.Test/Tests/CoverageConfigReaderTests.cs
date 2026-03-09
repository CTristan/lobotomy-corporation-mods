// SPDX-License-Identifier: MIT

using AwesomeAssertions;
using Moq;
using Xunit;

namespace CI.Test.Tests
{
    public sealed class CoverageConfigReaderTests
    {
        [Fact]
        public void ReadConfig_ConfigFileExists_ReturnsConfig()
        {
            Mock<IFileSystem> mockFileSystem = new();
            string configJson = /*lang=json,strict*/ """
            {
              "lineThreshold": 85,
              "branchThreshold": 80,
              "methodThreshold": 90
            }
            """;

            _ = mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
            _ = mockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).Returns(configJson);

            CoverageConfigReader reader = new(mockFileSystem.Object);
            CoverageConfig? config = reader.ReadConfig("/repo");

            _ = config.Should().NotBeNull();
            _ = config!.LineThreshold.Should().Be(85);
            _ = config.BranchThreshold.Should().Be(80);
            _ = config.MethodThreshold.Should().Be(90);
        }

        [Fact]
        public void ReadConfig_ConfigFileNotExists_ReturnsNull()
        {
            Mock<IFileSystem> mockFileSystem = new();

            _ = mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

            CoverageConfigReader reader = new(mockFileSystem.Object);
            CoverageConfig? config = reader.ReadConfig("/repo");

            _ = config.Should().BeNull();
        }

        [Fact]
        public void ReadConfig_InvalidJson_ReturnsNull()
        {
            Mock<IFileSystem> mockFileSystem = new();

            _ = mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
            _ = mockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).Returns("invalid json");

            CoverageConfigReader reader = new(mockFileSystem.Object);
            CoverageConfig? config = reader.ReadConfig("/repo");

            _ = config.Should().BeNull();
        }

        [Fact]
        public void ReadConfig_ReadAllTextReturnsNull_ReturnsNull()
        {
            Mock<IFileSystem> mockFileSystem = new();

            _ = mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
            _ = mockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).Returns((string)null);

            CoverageConfigReader reader = new(mockFileSystem.Object);
            CoverageConfig? config = reader.ReadConfig("/repo");

            _ = config.Should().BeNull();
        }

        [Fact]
        public void ReadConfig_PartialConfig_UsesDefaultsForMissingValues()
        {
            Mock<IFileSystem> mockFileSystem = new();
            string configJson = /*lang=json,strict*/ """
            {
              "lineThreshold": 90
            }
            """;

            _ = mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
            _ = mockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).Returns(configJson);

            CoverageConfigReader reader = new(mockFileSystem.Object);
            CoverageConfig? config = reader.ReadConfig("/repo");

            _ = config.Should().NotBeNull();
            _ = config!.LineThreshold.Should().Be(90);
            _ = config.BranchThreshold.Should().Be(70); // default
            _ = config.MethodThreshold.Should().Be(75); // default
        }

        [Fact]
        public void ReadConfig_EmptyConfig_UsesDefaults()
        {
            Mock<IFileSystem> mockFileSystem = new();
            string configJson = "{}";

            _ = mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
            _ = mockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).Returns(configJson);

            CoverageConfigReader reader = new(mockFileSystem.Object);
            CoverageConfig? config = reader.ReadConfig("/repo");

            _ = config.Should().NotBeNull();
            _ = config!.LineThreshold.Should().Be(80); // default
            _ = config.BranchThreshold.Should().Be(70); // default
            _ = config.MethodThreshold.Should().Be(75); // default
        }
    }
}
