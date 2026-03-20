// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AutoFixture;
using AutoFixture.AutoMoq;
using AwesomeAssertions;
using DebugPanel.Common.Enums.Diagnostics;
using DebugPanel.Common.Models.Diagnostics;
using DebugPanel.Implementations;
using DebugPanel.Interfaces;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class DependencyCheckerTests
    {
        private readonly Mock<IFileSystemScanner> _mockScanner;

        public DependencyCheckerTests()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _mockScanner = fixture.Freeze<Mock<IFileSystemScanner>>();
            _mockScanner.Setup(s => s.GetBaseModsPath()).Returns("/basemods");
            _mockScanner.Setup(s => s.FileExists(It.IsAny<string>())).Returns(false);
        }

        [Fact]
        public void Constructor_throws_when_scanner_is_null()
        {
            Action act = () => _ = new DependencyChecker(null, [], []);

            act.Should().Throw<ArgumentNullException>().WithParameterName("scanner");
        }

        [Fact]
        public void Constructor_throws_when_detectedMods_is_null()
        {
            Action act = () => _ = new DependencyChecker(_mockScanner.Object, null, []);

            act.Should().Throw<ArgumentNullException>().WithParameterName("detectedMods");
        }

        [Fact]
        public void Constructor_throws_when_loadedAssemblies_is_null()
        {
            Action act = () => _ = new DependencyChecker(_mockScanner.Object, [], null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("loadedAssemblies");
        }

        [Fact]
        public void Collect_reports_missing_12harmony_when_referenced()
        {
            _mockScanner.Setup(s => s.FileExists("/basemods/BaseModList_v2.xml")).Returns(true);
            var assemblies = new List<AssemblyInfo> { new("12Harmony", "1.0", "/path", false, []) };
            var checker = new DependencyChecker(_mockScanner.Object, [], assemblies);

            var result = checker.Collect();

            result.Issues.Should().HaveCount(1);
            result.Issues[0].Severity.Should().Be(FindingSeverity.Error);
            result.Issues[0].Description.Should().Contain("12Harmony.dll");
        }

        [Fact]
        public void Collect_does_not_report_12harmony_when_present()
        {
            _mockScanner.Setup(s => s.FileExists("/basemods/12Harmony.dll")).Returns(true);
            _mockScanner.Setup(s => s.FileExists("/basemods/BaseModList_v2.xml")).Returns(true);
            var assemblies = new List<AssemblyInfo> { new("12Harmony", "1.0", "/path", false, []) };
            var checker = new DependencyChecker(_mockScanner.Object, [], assemblies);

            var result = checker.Collect();

            result.Issues.Should().NotContain(i => i.Description.Contains("12Harmony"));
        }

        [Fact]
        public void Collect_reports_missing_basemodlist()
        {
            var checker = new DependencyChecker(_mockScanner.Object, [], []);

            var result = checker.Collect();

            result.Issues.Should().Contain(i => i.Description.Contains("BaseModList_v2.xml"));
            result.BaseModListExists.Should().BeFalse();
        }

        [Fact]
        public void Collect_does_not_report_basemodlist_when_present()
        {
            _mockScanner.Setup(s => s.FileExists("/basemods/BaseModList_v2.xml")).Returns(true);
            var checker = new DependencyChecker(_mockScanner.Object, [], []);

            var result = checker.Collect();

            result.Issues.Should().NotContain(i => i.Description.Contains("BaseModList_v2.xml"));
            result.BaseModListExists.Should().BeTrue();
        }

        [Fact]
        public void Collect_detects_basemod_version()
        {
            _mockScanner.Setup(s => s.FileExists("/basemods/BaseModList_v2.xml")).Returns(true);
            var assemblies = new List<AssemblyInfo> { new("LobotomyBaseModLib", "3.1.0", "/path", false, []) };
            var checker = new DependencyChecker(_mockScanner.Object, [], assemblies);

            var result = checker.Collect();

            result.BaseModVersion.Should().Be("3.1.0");
        }

        [Fact]
        public void Collect_returns_empty_version_when_basemod_not_loaded()
        {
            _mockScanner.Setup(s => s.FileExists("/basemods/BaseModList_v2.xml")).Returns(true);
            var checker = new DependencyChecker(_mockScanner.Object, [], []);

            var result = checker.Collect();

            result.BaseModVersion.Should().BeEmpty();
        }
    }
}
