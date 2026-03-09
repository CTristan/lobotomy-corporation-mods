// SPDX-License-Identifier: MIT

using System;
using System.IO;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace CI.Tests
{
    public sealed class CoverageThresholdCheckerTests
    {
        [Fact]
        public void CheckThresholds_AllThresholdsMet_ReturnsTrue()
        {
            Mock<IFileSystem> mockFileSystem = new();
            string coverageXml = GenerateCoverageXml("TestModule", lineCovered: 90, branchCovered: 80, methodCovered: 85);
            string configJson = /*lang=json,strict*/ """{"lineThreshold": 80, "branchThreshold": 70, "methodThreshold": 75}""";

            // Mock finding test directories
            _ = mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
            _ = mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);

            // Mock config file read
            _ = mockFileSystem.Setup(fs => fs.ReadAllText(It.Is<string>(p => p.EndsWith("coverlet.json"))))
                .Returns(configJson);

            CoverageThresholdChecker checker = new(mockFileSystem.Object);

            // Write temporary coverage file
            string tempDir = Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}");
            _ = Directory.CreateDirectory(tempDir);
            string testDir = Path.Combine(tempDir, "TestProject.Test");
            _ = Directory.CreateDirectory(testDir);
            string coveragePath = Path.Combine(testDir, "coverage.opencover.xml");
            File.WriteAllText(coveragePath, coverageXml);

            try
            {
                bool result = checker.CheckThresholds(tempDir, out string? failureMessage);

                _ = result.Should().BeTrue();
                _ = failureMessage.Should().BeEmpty();
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void CheckThresholds_AggregatesCoverageFromMultipleProjects_ReturnsCorrectOverall()
        {
            Mock<IFileSystem> mockFileSystem = new();
            string coverageXml1 = GenerateCoverageXml("Module1", lineCovered: 100, branchCovered: 100, methodCovered: 100);
            string coverageXml2 = GenerateCoverageXml("Module2", lineCovered: 40, branchCovered: 20, methodCovered: 60);
            string configJson = /*lang=json,strict*/ """{"lineThreshold": 80, "branchThreshold": 70, "methodThreshold": 75}""";

            _ = mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
            _ = mockFileSystem.Setup(fs => fs.ReadAllText(It.Is<string>(p => p.EndsWith("coverlet.json"))))
                .Returns(configJson);

            CoverageThresholdChecker checker = new(mockFileSystem.Object);

            string tempDir = Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}");
            _ = Directory.CreateDirectory(tempDir);

            // Create two test projects
            string testDir1 = Path.Combine(tempDir, "TestProject1.Test");
            _ = Directory.CreateDirectory(testDir1);
            string coveragePath1 = Path.Combine(testDir1, "coverage.opencover.xml");
            File.WriteAllText(coveragePath1, coverageXml1);

            string testDir2 = Path.Combine(tempDir, "TestProject2.Test");
            _ = Directory.CreateDirectory(testDir2);
            string coveragePath2 = Path.Combine(testDir2, "coverage.opencover.xml");
            File.WriteAllText(coveragePath2, coverageXml2);

            try
            {
                bool result = checker.CheckThresholds(tempDir, out string? failureMessage);

                // Module2 should fail the thresholds
                _ = result.Should().BeFalse();
                _ = failureMessage.Should().Contain("Module 'Module2' line coverage");
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void CheckThresholds_NoCoverageReports_ReturnsFalse()
        {
            Mock<IFileSystem> mockFileSystem = new();

            CoverageThresholdChecker checker = new(mockFileSystem.Object);

            string tempDir = Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}");
            _ = Directory.CreateDirectory(tempDir);

            try
            {
                bool result = checker.CheckThresholds(tempDir, out string? failureMessage);

                _ = result.Should().BeFalse();
                _ = failureMessage.Should().Contain("No coverage reports found");
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void CheckThresholds_ConfigNotExists_ReturnsFalse()
        {
            Mock<IFileSystem> mockFileSystem = new();
            string coverageXml = GenerateCoverageXml("TestModule", lineCovered: 90, branchCovered: 80, methodCovered: 85);

            _ = mockFileSystem.Setup(fs => fs.FileExists(It.Is<string>(p => p.EndsWith("coverlet.json"))))
                .Returns(false);

            CoverageThresholdChecker checker = new(mockFileSystem.Object);

            string tempDir = Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}");
            _ = Directory.CreateDirectory(tempDir);
            string testDir = Path.Combine(tempDir, "TestProject.Test");
            _ = Directory.CreateDirectory(testDir);
            string coveragePath = Path.Combine(testDir, "coverage.opencover.xml");
            File.WriteAllText(coveragePath, coverageXml);

            try
            {
                bool result = checker.CheckThresholds(tempDir, out string? failureMessage);

                _ = result.Should().BeFalse();
                _ = failureMessage.Should().Contain("Coverlet config not found");
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void CheckThresholds_MalformedXml_ReturnsFalse()
        {
            Mock<IFileSystem> mockFileSystem = new();
            string configJson = /*lang=json,strict*/ """{"lineThreshold": 80, "branchThreshold": 70, "methodThreshold": 75}""";

            _ = mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
            _ = mockFileSystem.Setup(fs => fs.ReadAllText(It.Is<string>(p => p.EndsWith("coverlet.json"))))
                .Returns(configJson);

            CoverageThresholdChecker checker = new(mockFileSystem.Object);

            string tempDir = Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}");
            _ = Directory.CreateDirectory(tempDir);
            string testDir = Path.Combine(tempDir, "TestProject.Test");
            _ = Directory.CreateDirectory(testDir);
            string coveragePath = Path.Combine(testDir, "coverage.opencover.xml");
            File.WriteAllText(coveragePath, "invalid xml");

            try
            {
                bool result = checker.CheckThresholds(tempDir, out string? failureMessage);

                _ = result.Should().BeFalse();
                _ = failureMessage.Should().Contain("Error processing coverage reports");
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void CheckThresholds_EdgeCase_ZeroMetrics_ReturnsFalse()
        {
            Mock<IFileSystem> mockFileSystem = new();
            string coverageXml = GenerateCoverageXmlWithZeroMetrics();
            string configJson = /*lang=json,strict*/ """{"lineThreshold": 80, "branchThreshold": 70, "methodThreshold": 75}""";

            _ = mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
            _ = mockFileSystem.Setup(fs => fs.ReadAllText(It.Is<string>(p => p.EndsWith("coverlet.json"))))
                .Returns(configJson);

            CoverageThresholdChecker checker = new(mockFileSystem.Object);

            string tempDir = Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}");
            _ = Directory.CreateDirectory(tempDir);
            string testDir = Path.Combine(tempDir, "TestProject.Test");
            _ = Directory.CreateDirectory(testDir);
            string coveragePath = Path.Combine(testDir, "coverage.opencover.xml");
            File.WriteAllText(coveragePath, coverageXml);

            try
            {
                bool result = checker.CheckThresholds(tempDir, out string? failureMessage);

                // Zero metrics means no actual code was found to check
                _ = result.Should().BeFalse();
                _ = failureMessage.Should().Contain("No coverage metrics found");
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
}
