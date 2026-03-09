// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.Xml.Linq;
using AwesomeAssertions;
using CI;
using Moq;
using Xunit;

namespace CI.Tests;

public sealed class CoverageThresholdCheckerTests
{
    [Fact]
    public void CheckThresholds_AllThresholdsMet_ReturnsTrue()
    {
        var mockFileSystem = new Mock<IFileSystem>();
        var coverageXml = GenerateCoverageXml("TestModule", lineCovered: 90, branchCovered: 80, methodCovered: 85);
        var configJson = """{"lineThreshold": 80, "branchThreshold": 70, "methodThreshold": 75}""";

        // Mock finding test directories
        mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);

        // Mock config file read
        mockFileSystem.Setup(fs => fs.ReadAllText(It.Is<string>(p => p.EndsWith("coverlet.json"))))
            .Returns(configJson);

        var checker = new CoverageThresholdChecker(mockFileSystem.Object);

        // Write temporary coverage file
        var tempDir = Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var testDir = Path.Combine(tempDir, "TestProject.Test");
        Directory.CreateDirectory(testDir);
        var coveragePath = Path.Combine(testDir, "coverage.opencover.xml");
        File.WriteAllText(coveragePath, coverageXml);

        try
        {
            var result = checker.CheckThresholds(tempDir, out var failureMessage);

            result.Should().BeTrue();
            failureMessage.Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void CheckThresholds_AggregatesCoverageFromMultipleProjects_ReturnsCorrectOverall()
    {
        var mockFileSystem = new Mock<IFileSystem>();
        var coverageXml1 = GenerateCoverageXml("Module1", lineCovered: 100, branchCovered: 100, methodCovered: 100);
        var coverageXml2 = GenerateCoverageXml("Module2", lineCovered: 40, branchCovered: 20, methodCovered: 60);
        var configJson = """{"lineThreshold": 80, "branchThreshold": 70, "methodThreshold": 75}""";

        mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
        mockFileSystem.Setup(fs => fs.ReadAllText(It.Is<string>(p => p.EndsWith("coverlet.json"))))
            .Returns(configJson);

        var checker = new CoverageThresholdChecker(mockFileSystem.Object);

        var tempDir = Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        // Create two test projects
        var testDir1 = Path.Combine(tempDir, "TestProject1.Test");
        Directory.CreateDirectory(testDir1);
        var coveragePath1 = Path.Combine(testDir1, "coverage.opencover.xml");
        File.WriteAllText(coveragePath1, coverageXml1);

        var testDir2 = Path.Combine(tempDir, "TestProject2.Test");
        Directory.CreateDirectory(testDir2);
        var coveragePath2 = Path.Combine(testDir2, "coverage.opencover.xml");
        File.WriteAllText(coveragePath2, coverageXml2);

        try
        {
            var result = checker.CheckThresholds(tempDir, out var failureMessage);

            // Module2 should fail the thresholds
            result.Should().BeFalse();
            failureMessage.Should().Contain("Module 'Module2' line coverage");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void CheckThresholds_NoCoverageReports_ReturnsFalse()
    {
        var mockFileSystem = new Mock<IFileSystem>();

        var checker = new CoverageThresholdChecker(mockFileSystem.Object);

        var tempDir = Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var result = checker.CheckThresholds(tempDir, out var failureMessage);

            result.Should().BeFalse();
            failureMessage.Should().Contain("No coverage reports found");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void CheckThresholds_ConfigNotExists_ReturnsFalse()
    {
        var mockFileSystem = new Mock<IFileSystem>();
        var coverageXml = GenerateCoverageXml("TestModule", lineCovered: 90, branchCovered: 80, methodCovered: 85);

        mockFileSystem.Setup(fs => fs.FileExists(It.Is<string>(p => p.EndsWith("coverlet.json"))))
            .Returns(false);

        var checker = new CoverageThresholdChecker(mockFileSystem.Object);

        var tempDir = Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var testDir = Path.Combine(tempDir, "TestProject.Test");
        Directory.CreateDirectory(testDir);
        var coveragePath = Path.Combine(testDir, "coverage.opencover.xml");
        File.WriteAllText(coveragePath, coverageXml);

        try
        {
            var result = checker.CheckThresholds(tempDir, out var failureMessage);

            result.Should().BeFalse();
            failureMessage.Should().Contain("Coverlet config not found");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void CheckThresholds_MalformedXml_ReturnsFalse()
    {
        var mockFileSystem = new Mock<IFileSystem>();
        var configJson = """{"lineThreshold": 80, "branchThreshold": 70, "methodThreshold": 75}""";

        mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
        mockFileSystem.Setup(fs => fs.ReadAllText(It.Is<string>(p => p.EndsWith("coverlet.json"))))
            .Returns(configJson);

        var checker = new CoverageThresholdChecker(mockFileSystem.Object);

        var tempDir = Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var testDir = Path.Combine(tempDir, "TestProject.Test");
        Directory.CreateDirectory(testDir);
        var coveragePath = Path.Combine(testDir, "coverage.opencover.xml");
        File.WriteAllText(coveragePath, "invalid xml");

        try
        {
            var result = checker.CheckThresholds(tempDir, out var failureMessage);

            result.Should().BeFalse();
            failureMessage.Should().Contain("Error processing coverage reports");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void CheckThresholds_EdgeCase_ZeroMetrics_ReturnsFalse()
    {
        var mockFileSystem = new Mock<IFileSystem>();
        var coverageXml = GenerateCoverageXmlWithZeroMetrics();
        var configJson = """{"lineThreshold": 80, "branchThreshold": 70, "methodThreshold": 75}""";

        mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
        mockFileSystem.Setup(fs => fs.ReadAllText(It.Is<string>(p => p.EndsWith("coverlet.json"))))
            .Returns(configJson);

        var checker = new CoverageThresholdChecker(mockFileSystem.Object);

        var tempDir = Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var testDir = Path.Combine(tempDir, "TestProject.Test");
        Directory.CreateDirectory(testDir);
        var coveragePath = Path.Combine(testDir, "coverage.opencover.xml");
        File.WriteAllText(coveragePath, coverageXml);

        try
        {
            var result = checker.CheckThresholds(tempDir, out var failureMessage);

            // Zero metrics means no actual code was found to check
            result.Should().BeFalse();
            failureMessage.Should().Contain("No coverage metrics found");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static string GenerateCoverageXml(string moduleName, double lineCovered, double branchCovered, double methodCovered)
    {
        return $"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<CoverageSession>\r\n  <Modules>\r\n    <Module hash=\"test\">\r\n      <ModuleName>{moduleName}</ModuleName>\r\n      <Classes>\r\n        <Class name=\"TestClass\">\r\n          <Summary numSequencePoints=\"100\" visitedSequencePoints=\"{lineCovered}\" numBranchPoints=\"100\" visitedBranchPoints=\"{branchCovered}\" numMethods=\"100\" visitedMethods=\"{methodCovered}\" />\r\n        </Class>\r\n      </Classes>\r\n    </Module>\r\n  </Modules>\r\n</CoverageSession>";
    }

    private static string GenerateCoverageXmlWithZeroMetrics()
    {
        return "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<CoverageSession>\r\n  <Modules>\r\n    <Module hash=\"test\">\r\n      <ModuleName>TestModule</ModuleName>\r\n      <Classes>\r\n        <Class name=\"TestClass\">\r\n          <Summary numSequencePoints=\"0\" visitedSequencePoints=\"0\" numBranchPoints=\"0\" visitedBranchPoints=\"0\" numMethods=\"0\" visitedMethods=\"0\" />\r\n        </Class>\r\n      </Classes>\r\n    </Module>\r\n  </Modules>\r\n</CoverageSession>";
    }
}
