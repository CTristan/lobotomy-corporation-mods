// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using AwesomeAssertions;
using LobotomyPlaywright.Commands;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.Network;
using Moq;
using Xunit;
using Config = LobotomyPlaywright.Interfaces.Configuration.Config;

namespace LobotomyPlaywright.Tests.Commands
{
    public sealed class QueryCommandGameObjectsTests
    {
        private readonly Mock<ITcpClient> _mockTcpClient;
        private readonly Mock<IConfigManager> _mockConfigManager;
        private readonly QueryCommand _queryCommand;

        public QueryCommandGameObjectsTests()
        {
            _mockTcpClient = new Mock<ITcpClient>();
            var mockTcpClientFactory = new Mock<Func<ITcpClient>>();

            _mockConfigManager = new Mock<IConfigManager>();

            _ = mockTcpClientFactory.Setup(f => f()).Returns(_mockTcpClient.Object);

            Config config = new()
            {
                GamePath = "/test/game/path",
                CrossoverBottle = "TestBottle",
                TcpPort = 8484,
                LaunchTimeoutSeconds = 120,
                ShutdownTimeoutSeconds = 10
            };
            _ = _mockConfigManager.Setup(c => c.Load()).Returns(config);

            _queryCommand = new QueryCommand(_mockConfigManager.Object, mockTcpClientFactory.Object);
        }

        [Fact]
        public void Run_DiscoverDefault_QueriesGameObjectsWithDefaultParams()
        {
            // Arrange
            Dictionary<string, object> responseData = new()
            {
                { "roots", new List<Dictionary<string, object>>() },
                { "rootCount", 0 }
            };
            _ = _mockTcpClient.Setup(c => c.Query("gameobjects", It.IsAny<Dictionary<string, object>>()))
                .Returns(responseData);

            // Act
            int result = _queryCommand.Run(["gameobjects"]);

            // Assert
            _ = result.Should().Be(0);
            _mockTcpClient.Verify(c => c.Query("gameobjects",
                It.Is<Dictionary<string, object>>(p =>
                    p.ContainsKey("mode") && p["mode"].ToString() == "discover")), Times.Once);
        }

        [Fact]
        public void Run_DiscoverWithDepth_QueriesGameObjectsWithCustomDepth()
        {
            // Arrange
            Dictionary<string, object> responseData = new()
            {
                { "roots", new List<Dictionary<string, object>>() },
                { "rootCount", 0 }
            };
            _ = _mockTcpClient.Setup(c => c.Query("gameobjects", It.IsAny<Dictionary<string, object>>()))
                .Returns(responseData);

            // Act
            int result = _queryCommand.Run(["gameobjects", "--depth", "5"]);

            // Assert
            _ = result.Should().Be(0);
            _mockTcpClient.Verify(c => c.Query("gameobjects",
                It.Is<Dictionary<string, object>>(p =>
                    p.ContainsKey("depth") && (double)p["depth"] == 5.0)), Times.Once);
        }

        [Fact]
        public void Run_InspectPath_QueriesGameObjectsInInspectMode()
        {
            // Arrange
            Dictionary<string, object> responseData = new()
            {
                { "name", "Window" },
                { "path", "Canvas/Window" },
                { "active", true },
                { "tag", "Untagged" },
                { "layer", 5 },
                { "components", new List<Dictionary<string, object>>() }
            };
            _ = _mockTcpClient.Setup(c => c.Query("gameobjects", It.IsAny<Dictionary<string, object>>()))
                .Returns(responseData);

            // Act
            int result = _queryCommand.Run(["gameobjects", "--inspect", "Canvas/Window"]);

            // Assert
            _ = result.Should().Be(0);
            _mockTcpClient.Verify(c => c.Query("gameobjects",
                It.Is<Dictionary<string, object>>(p =>
                    p["mode"].ToString() == "inspect" &&
                    p["path"].ToString() == "Canvas/Window" &&
                    p["detail"].ToString() == "summary")), Times.Once);
        }

        [Fact]
        public void Run_InspectWithDetailFull_QueriesWithFullDetail()
        {
            // Arrange
            Dictionary<string, object> responseData = new()
            {
                { "name", "Window" },
                { "path", "Canvas/Window" },
                { "active", true },
                { "tag", "Untagged" },
                { "layer", 5 },
                { "components", new List<Dictionary<string, object>>
                    {
                        new()
                        {
                            { "typeName", "UnityEngine.RectTransform" },
                            { "fields", new List<Dictionary<string, object>>
                                {
                                    new()
                                    {
                                        { "name", "anchorMin" },
                                        { "type", "Vector2" },
                                        { "value", "(0.0, 0.0)" }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            _ = _mockTcpClient.Setup(c => c.Query("gameobjects", It.IsAny<Dictionary<string, object>>()))
                .Returns(responseData);

            // Act
            int result = _queryCommand.Run(["gameobjects", "--inspect", "Canvas/Window", "--detail", "full"]);

            // Assert
            _ = result.Should().Be(0);
            _mockTcpClient.Verify(c => c.Query("gameobjects",
                It.Is<Dictionary<string, object>>(p =>
                    p["detail"].ToString() == "full")), Times.Once);
        }

        [Fact]
        public void Run_SearchByName_QueriesGameObjectsInSearchMode()
        {
            // Arrange
            Dictionary<string, object> responseData = new()
            {
                { "query", "name contains \"Button\"" },
                { "resultCount", 2 },
                { "results", new List<Dictionary<string, object>>() }
            };
            _ = _mockTcpClient.Setup(c => c.Query("gameobjects", It.IsAny<Dictionary<string, object>>()))
                .Returns(responseData);

            // Act
            int result = _queryCommand.Run(["gameobjects", "--search", "--name", "Button"]);

            // Assert
            _ = result.Should().Be(0);
            _mockTcpClient.Verify(c => c.Query("gameobjects",
                It.Is<Dictionary<string, object>>(p =>
                    p["mode"].ToString() == "search" &&
                    p["name"].ToString() == "Button")), Times.Once);
        }

        [Fact]
        public void Run_SearchExactName_SetsNameMatchExact()
        {
            // Arrange
            Dictionary<string, object> responseData = new()
            {
                { "query", "name exact \"Canvas\"" },
                { "resultCount", 1 },
                { "results", new List<Dictionary<string, object>>() }
            };
            _ = _mockTcpClient.Setup(c => c.Query("gameobjects", It.IsAny<Dictionary<string, object>>()))
                .Returns(responseData);

            // Act
            int result = _queryCommand.Run(["gameobjects", "--search", "--name", "Canvas", "--exact"]);

            // Assert
            _ = result.Should().Be(0);
            _mockTcpClient.Verify(c => c.Query("gameobjects",
                It.Is<Dictionary<string, object>>(p =>
                    p["nameMatch"].ToString() == "exact")), Times.Once);
        }

        [Fact]
        public void Run_SearchByComponent_SetsComponentFilter()
        {
            // Arrange
            Dictionary<string, object> responseData = new()
            {
                { "query", "component=Text" },
                { "resultCount", 5 },
                { "results", new List<Dictionary<string, object>>() }
            };
            _ = _mockTcpClient.Setup(c => c.Query("gameobjects", It.IsAny<Dictionary<string, object>>()))
                .Returns(responseData);

            // Act
            int result = _queryCommand.Run(["gameobjects", "--search", "--component", "Text"]);

            // Assert
            _ = result.Should().Be(0);
            _mockTcpClient.Verify(c => c.Query("gameobjects",
                It.Is<Dictionary<string, object>>(p =>
                    p["component"].ToString() == "Text")), Times.Once);
        }

        [Fact]
        public void Run_SearchByTag_SetsTagFilter()
        {
            // Arrange
            Dictionary<string, object> responseData = new()
            {
                { "query", "tag=MainCamera" },
                { "resultCount", 1 },
                { "results", new List<Dictionary<string, object>>() }
            };
            _ = _mockTcpClient.Setup(c => c.Query("gameobjects", It.IsAny<Dictionary<string, object>>()))
                .Returns(responseData);

            // Act
            int result = _queryCommand.Run(["gameobjects", "--search", "--tag", "MainCamera"]);

            // Assert
            _ = result.Should().Be(0);
            _mockTcpClient.Verify(c => c.Query("gameobjects",
                It.Is<Dictionary<string, object>>(p =>
                    p["tag"].ToString() == "MainCamera")), Times.Once);
        }

        [Fact]
        public void Run_SearchActiveOnly_SetsActiveOnlyFlag()
        {
            // Arrange
            Dictionary<string, object> responseData = new()
            {
                { "query", "activeOnly" },
                { "resultCount", 10 },
                { "results", new List<Dictionary<string, object>>() }
            };
            _ = _mockTcpClient.Setup(c => c.Query("gameobjects", It.IsAny<Dictionary<string, object>>()))
                .Returns(responseData);

            // Act
            int result = _queryCommand.Run(["gameobjects", "--search", "--active-only"]);

            // Assert
            _ = result.Should().Be(0);
            _mockTcpClient.Verify(c => c.Query("gameobjects",
                It.Is<Dictionary<string, object>>(p =>
                    p.ContainsKey("activeOnly") && (bool)p["activeOnly"])), Times.Once);
        }

        [Fact]
        public void Run_SearchCombinedFlags_SetsAllFilters()
        {
            // Arrange
            Dictionary<string, object> responseData = new()
            {
                { "query", "name contains \"Panel\", component=Image, tag=UI, activeOnly" },
                { "resultCount", 3 },
                { "results", new List<Dictionary<string, object>>() }
            };
            _ = _mockTcpClient.Setup(c => c.Query("gameobjects", It.IsAny<Dictionary<string, object>>()))
                .Returns(responseData);

            // Act
            int result = _queryCommand.Run(["gameobjects", "--search", "--name", "Panel", "--component", "Image", "--tag", "UI", "--active-only"]);

            // Assert
            _ = result.Should().Be(0);
            _mockTcpClient.Verify(c => c.Query("gameobjects",
                It.Is<Dictionary<string, object>>(p =>
                    p["mode"].ToString() == "search" &&
                    p["name"].ToString() == "Panel" &&
                    p["component"].ToString() == "Image" &&
                    p["tag"].ToString() == "UI" &&
                    (bool)p["activeOnly"])), Times.Once);
        }

        [Fact]
        public void Run_DumpFileNotFound_ReturnsError()
        {
            // Act
            int result = _queryCommand.Run(["gameobjects", "--dump"]);

            // Assert
            _ = result.Should().NotBe(0);
        }

        [Fact]
        public void Run_DumpFileExists_ReadsAndDisplaysContent()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), "QueryCommandGameObjectsTests_" + Guid.NewGuid().ToString("N"));
            var playwrightDir = Path.Combine(tempDir, ".lobotomy-playwright");
            Directory.CreateDirectory(playwrightDir);
            var dumpPath = Path.Combine(playwrightDir, "gameobject_dump.json");
            File.WriteAllText(dumpPath, GetSampleDumpContent());

            Config config = new()
            {
                GamePath = tempDir,
                CrossoverBottle = "TestBottle",
                TcpPort = 8484,
                LaunchTimeoutSeconds = 120,
                ShutdownTimeoutSeconds = 10
            };
            _ = _mockConfigManager.Setup(c => c.Load()).Returns(config);

            try
            {
                // Act
                int result = _queryCommand.Run(["gameobjects", "--dump"]);

                // Assert
                _ = result.Should().Be(0);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void Run_JsonOutput_PassesThroughJsonFlag()
        {
            // Arrange
            Dictionary<string, object> responseData = new()
            {
                { "roots", new List<Dictionary<string, object>>() },
                { "rootCount", 0 }
            };
            _ = _mockTcpClient.Setup(c => c.Query("gameobjects", It.IsAny<Dictionary<string, object>>()))
                .Returns(responseData);

            // Act
            int result = _queryCommand.Run(["gameobjects", "--json"]);

            // Assert
            _ = result.Should().Be(0);
        }

        [Fact]
        public void Run_GoAlias_QueriesGameObjects()
        {
            // Arrange
            Dictionary<string, object> responseData = new()
            {
                { "roots", new List<Dictionary<string, object>>() },
                { "rootCount", 0 }
            };
            _ = _mockTcpClient.Setup(c => c.Query("gameobjects", It.IsAny<Dictionary<string, object>>()))
                .Returns(responseData);

            // Act
            int result = _queryCommand.Run(["go"]);

            // Assert
            _ = result.Should().Be(0);
        }

        [Fact]
        public void Run_GameobjectAlias_QueriesGameObjects()
        {
            // Arrange
            Dictionary<string, object> responseData = new()
            {
                { "roots", new List<Dictionary<string, object>>() },
                { "rootCount", 0 }
            };
            _ = _mockTcpClient.Setup(c => c.Query("gameobjects", It.IsAny<Dictionary<string, object>>()))
                .Returns(responseData);

            // Act
            int result = _queryCommand.Run(["gameobject"]);

            // Assert
            _ = result.Should().Be(0);
        }
        private static string GetSampleDumpContent()
        {
            return new System.Text.StringBuilder()
                .Append('[')
                .Append(/*lang=json,strict*/ "{\"name\":\"Root\"}")
                .Append(']')
                .ToString();
        }
    }
}
