// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.DebugPanel.Implementations;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.DebugPanel.JsonModels;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class JsonKnownIssuesDatabaseTests
    {
        private readonly Mock<IFileSystemScanner> _mockScanner;
        private readonly Mock<IJsonParser> _mockParser;

        public JsonKnownIssuesDatabaseTests()
        {
            _mockScanner = new Mock<IFileSystemScanner>();
            _mockParser = new Mock<IJsonParser>();
            _mockScanner.Setup(s => s.GetExternalDataPath()).Returns("/game/ExternalData");
        }

        [Fact]
        public void Constructor_throws_when_scanner_is_null()
        {
            Action act = () => _ = new JsonKnownIssuesDatabase(null, _mockParser.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("scanner");
        }

        [Fact]
        public void Constructor_throws_when_jsonParser_is_null()
        {
            Action act = () => _ = new JsonKnownIssuesDatabase(_mockScanner.Object, null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("jsonParser");
        }

        [Fact]
        public void GetKnownIssues_returns_empty_when_file_does_not_exist()
        {
            _mockScanner.Setup(s => s.FileExists(It.IsAny<string>())).Returns(false);
            var db = new JsonKnownIssuesDatabase(_mockScanner.Object, _mockParser.Object);

            var result = db.GetKnownIssues();

            result.Should().BeEmpty();
        }

        [Fact]
        public void GetKnownIssues_returns_parsed_issues()
        {
            _mockScanner.Setup(s => s.FileExists(It.IsAny<string>())).Returns(true);
            _mockScanner.Setup(s => s.ReadAllText(It.IsAny<string>())).Returns("{json}");
            var data = new KnownIssuesData
            {
                version = "1.0",
                issues = new[] { new KnownIssueItem { modName = "TestMod" } },
            };
            _mockParser.Setup(p => p.FromJson<KnownIssuesData>("{json}")).Returns(data);
            var db = new JsonKnownIssuesDatabase(_mockScanner.Object, _mockParser.Object);

            var result = db.GetKnownIssues();

            result.Should().HaveCount(1);
            result[0].ModName.Should().Be("TestMod");
        }

        [Fact]
        public void DatabaseVersion_returns_version_from_parsed_data()
        {
            _mockScanner.Setup(s => s.FileExists(It.IsAny<string>())).Returns(true);
            _mockScanner.Setup(s => s.ReadAllText(It.IsAny<string>())).Returns("{json}");
            var data = new KnownIssuesData { version = "2.0" };
            _mockParser.Setup(p => p.FromJson<KnownIssuesData>("{json}")).Returns(data);
            var db = new JsonKnownIssuesDatabase(_mockScanner.Object, _mockParser.Object);

            db.DatabaseVersion.Should().Be("2.0");
        }

        [Fact]
        public void DatabaseVersion_returns_empty_when_file_does_not_exist()
        {
            _mockScanner.Setup(s => s.FileExists(It.IsAny<string>())).Returns(false);
            var db = new JsonKnownIssuesDatabase(_mockScanner.Object, _mockParser.Object);

            db.DatabaseVersion.Should().BeEmpty();
        }

        [Fact]
        public void GetKnownIssues_handles_parser_exception_gracefully()
        {
            _mockScanner.Setup(s => s.FileExists(It.IsAny<string>())).Returns(true);
            _mockScanner.Setup(s => s.ReadAllText(It.IsAny<string>())).Returns("{bad}");
            _mockParser.Setup(p => p.FromJson<KnownIssuesData>(It.IsAny<string>())).Throws(new InvalidOperationException("parse error"));
            var db = new JsonKnownIssuesDatabase(_mockScanner.Object, _mockParser.Object);

            var result = db.GetKnownIssues();

            result.Should().BeEmpty();
        }
    }
}
