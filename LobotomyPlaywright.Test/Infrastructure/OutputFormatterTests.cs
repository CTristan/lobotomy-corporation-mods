// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyPlaywright.Infrastructure;
using Xunit;

namespace LobotomyPlaywright.Tests.Infrastructure
{
    /// <summary>
    /// Tests for OutputFormatter.
    /// </summary>
    public sealed class OutputFormatterTests
    {
        /// <summary>
        /// Tests FormatAgent with JSON output returns JSON string.
        /// </summary>
        [Fact]
        public void FormatAgent_WithJsonOutput_ReturnsJsonString()
        {
            // Arrange
            Dictionary<string, object> agent = new()
            {
                { "name", "Test Agent" },
                { "instanceId", 1 },
                { "hp", 100.0 },
                { "maxHp", 100.0 },
                { "mental", 80.0 },
                { "maxMental", 100.0 },
                { "fortitude", 50 },
                { "prudence", 60 },
                { "temperance", 70 },
                { "justice", 80 },
                { "state", "IDLE" },
                { "currentSefira", "Control" },
                { "giftIds", new List<int>() },
                { "isDead", false },
                { "isPanicking", false }
            };

            // Act
            string result = OutputFormatter.FormatAgent(agent, jsonOutput: true);

            // Assert
            _ = result.Should().Contain("Test Agent");
            _ = result.Should().Contain("100");
        }

        /// <summary>
        /// Tests FormatAgent with formatted output returns formatted string.
        /// </summary>
        [Fact]
        public void FormatAgent_WithFormattedOutput_ReturnsFormattedString()
        {
            // Arrange
            Dictionary<string, object> agent = new()
            {
                { "name", "Test Agent" },
                { "instanceId", 1 },
                { "hp", 100.0 },
                { "maxHp", 100.0 },
                { "mental", 80.0 },
                { "maxMental", 100.0 },
                { "fortitude", 50 },
                { "prudence", 60 },
                { "temperance", 70 },
                { "justice", 80 },
                { "state", "IDLE" },
                { "currentSefira", "Control" },
                { "giftIds", new List<int>() },
                { "isDead", false },
                { "isPanicking", false }
            };

            // Act
            string result = OutputFormatter.FormatAgent(agent, jsonOutput: false);

            // Assert
            _ = result.Should().Contain("Agent: Test Agent (ID: 1)");
            _ = result.Should().Contain("HP: 100/100");
            _ = result.Should().Contain("Mental: 80/100");
            _ = result.Should().Contain("Stats: Fortitude 50, Prudence 60,");
            _ = result.Should().Contain("State: IDLE");
            _ = result.Should().Contain("Department: Control");
            _ = result.Should().Contain("Status: Normal");
        }

        /// <summary>
        /// Tests FormatCreature with formatted output returns formatted string.
        /// </summary>
        [Fact]
        public void FormatCreature_WithFormattedOutput_ReturnsFormattedString()
        {
            // Arrange
            Dictionary<string, object> creature = new()
            {
                { "name", "Test Creature" },
                { "instanceId", 100 },
                { "riskLevel", "WAW" },
                { "state", "IDLE" },
                { "qliphothCounter", 2 },
                { "maxQliphothCounter", 5 },
                { "feelingState", "GOOD" },
                { "currentSefira", "Information" },
                { "workCount", 10 },
                { "isEscaping", false },
                { "isSuppressed", false }
            };

            // Act
            string result = OutputFormatter.FormatCreature(creature, jsonOutput: false);

            // Assert
            _ = result.Should().Contain("Abnormality: Test Creature (ID: 100)");
            _ = result.Should().Contain("Risk Level: WAW");
            _ = result.Should().Contain("Qliphoth: 2/5");
            _ = result.Should().Contain("Work Count: 10");
            _ = result.Should().Contain("Status: Normal");
        }

        /// <summary>
        /// Tests FormatGameState with formatted output returns formatted string.
        /// </summary>
        [Fact]
        public void FormatGameState_WithFormattedOutput_ReturnsFormattedString()
        {
            // Arrange
            Dictionary<string, object> gameState = new()
            {
                { "day", 5 },
                { "gameState", "PLAYING" },
                { "gameSpeed", 1 },
                { "energy", 500.0 },
                { "energyQuota", 1000.0 },
                { "emergencyLevel", "NORMAL" },
                { "managementStarted", true },
                { "isPaused", false },
                { "playTime", 3600.0 },
                { "lobPoints", 1000.0 }
            };

            // Act
            string result = OutputFormatter.FormatGameState(gameState, jsonOutput: false);

            // Assert
            _ = result.Should().Contain("Day: 5");
            _ = result.Should().Contain("Phase: PLAYING");
            _ = result.Should().Contain("Speed: 1x");
            _ = result.Should().Contain("Energy: 500.0/1000.0 (50.0%)");
            _ = result.Should().Contain("Emergency: NORMAL");
        }

        /// <summary>
        /// Tests FormatDepartment with formatted output returns formatted string.
        /// </summary>
        [Fact]
        public void FormatDepartment_WithFormattedOutput_ReturnsFormattedString()
        {
            // Arrange
            Dictionary<string, object> department = new()
            {
                { "name", "Control Team" },
                { "sefiraEnum", "CONTROL" },
                { "isOpen", true },
                { "openLevel", 3 },
                { "agentIds", new List<int> { 1, 2, 3 } },
                { "creatureIds", new List<int> { 100, 101 } },
                { "officerCount", 2 }
            };

            // Act
            string result = OutputFormatter.FormatDepartment(department, jsonOutput: false);

            // Assert
            _ = result.Should().Contain("Department: Control Team (CONTROL)");
            _ = result.Should().Contain("Status: Open (Level 3)");
            _ = result.Should().Contain("Agents: 3");
            _ = result.Should().Contain("Creatures: 2");
            _ = result.Should().Contain("Officers: 2");
        }
        [Fact]
        public void FormatGameObjectTree_WithNestedHierarchy_ReturnsFormattedTree()
        {
            // Arrange
            Dictionary<string, object> data = new()
            {
                { "rootCount", 1 },
                { "roots", new List<Dictionary<string, object>>
                    {
                        new()
                        {
                            { "name", "Canvas" },
                            { "active", true },
                            { "tag", "Untagged" },
                            { "components", new List<string> { "Canvas", "CanvasScaler", "GraphicRaycaster" } },
                            { "children", new List<Dictionary<string, object>>
                                {
                                    new()
                                    {
                                        { "name", "Panel" },
                                        { "active", true },
                                        { "tag", "Untagged" },
                                        { "components", new List<string> { "RectTransform", "Image" } },
                                        { "children", new List<Dictionary<string, object>>() }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            string result = OutputFormatter.FormatGameObjectTree(data, jsonOutput: false);

            // Assert
            _ = result.Should().Contain("GameObject Tree (1 roots):");
            _ = result.Should().Contain("[+] Canvas (3 components)");
            _ = result.Should().Contain("[+] Panel (2 components)");
        }

        [Fact]
        public void FormatGameObjectTree_EmptyTree_ShowsZeroRoots()
        {
            // Arrange
            Dictionary<string, object> data = new()
            {
                { "rootCount", 0 },
                { "roots", new List<Dictionary<string, object>>() }
            };

            // Act
            string result = OutputFormatter.FormatGameObjectTree(data, jsonOutput: false);

            // Assert
            _ = result.Should().Contain("GameObject Tree (0 roots):");
        }

        [Fact]
        public void FormatGameObjectTree_JsonMode_ReturnsJson()
        {
            // Arrange
            Dictionary<string, object> data = new()
            {
                { "rootCount", 0 },
                { "roots", new List<Dictionary<string, object>>() }
            };

            // Act
            string result = OutputFormatter.FormatGameObjectTree(data, jsonOutput: true);

            // Assert
            _ = result.Should().Contain("\"rootCount\"");
        }

        [Fact]
        public void FormatGameObjectInspect_SummaryMode_ShowsComponentNames()
        {
            // Arrange
            Dictionary<string, object> data = new()
            {
                { "name", "MainCamera" },
                { "path", "MainCamera" },
                { "active", true },
                { "tag", "MainCamera" },
                { "layer", 0 },
                { "components", new List<Dictionary<string, object>>
                    {
                        new()
                        {
                            { "typeName", "UnityEngine.Camera" },
                            { "fields", new List<Dictionary<string, object>>() }
                        },
                        new()
                        {
                            { "typeName", "UnityEngine.AudioListener" },
                            { "fields", new List<Dictionary<string, object>>() }
                        }
                    }
                }
            };

            // Act
            string result = OutputFormatter.FormatGameObjectInspect(data, jsonOutput: false);

            // Assert
            _ = result.Should().Contain("GameObject: MainCamera");
            _ = result.Should().Contain("Path: MainCamera");
            _ = result.Should().Contain("Status: Active");
            _ = result.Should().Contain("Tag: MainCamera");
            _ = result.Should().Contain("Components (2):");
            _ = result.Should().Contain("UnityEngine.Camera");
            _ = result.Should().Contain("UnityEngine.AudioListener");
        }

        [Fact]
        public void FormatGameObjectInspect_FullMode_ShowsFields()
        {
            // Arrange
            Dictionary<string, object> data = new()
            {
                { "name", "Panel" },
                { "path", "Canvas/Panel" },
                { "active", true },
                { "tag", "Untagged" },
                { "layer", 5 },
                { "components", new List<Dictionary<string, object>>
                    {
                        new()
                        {
                            { "typeName", "UnityEngine.UI.Image" },
                            { "fields", new List<Dictionary<string, object>>
                                {
                                    new()
                                    {
                                        { "name", "m_Color" },
                                        { "type", "Color" },
                                        { "value", "RGBA(1.000, 1.000, 1.000, 1.000)" }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            string result = OutputFormatter.FormatGameObjectInspect(data, jsonOutput: false);

            // Assert
            _ = result.Should().Contain("UnityEngine.UI.Image:");
            _ = result.Should().Contain("m_Color (Color) = RGBA(1.000, 1.000, 1.000, 1.000)");
        }

        [Fact]
        public void FormatGameObjectInspect_JsonMode_ReturnsJson()
        {
            // Arrange
            Dictionary<string, object> data = new()
            {
                { "name", "Panel" },
                { "path", "Canvas/Panel" },
                { "active", true },
                { "tag", "Untagged" },
                { "layer", 5 },
                { "components", new List<Dictionary<string, object>>() }
            };

            // Act
            string result = OutputFormatter.FormatGameObjectInspect(data, jsonOutput: true);

            // Assert
            _ = result.Should().Contain("\"name\"");
            _ = result.Should().Contain("\"Panel\"");
        }

        [Fact]
        public void FormatGameObjectSearch_WithResults_ShowsNumberedList()
        {
            // Arrange
            Dictionary<string, object> data = new()
            {
                { "query", "name contains \"Button\"" },
                { "resultCount", 2 },
                { "results", new List<Dictionary<string, object>>
                    {
                        new()
                        {
                            { "name", "StartButton" },
                            { "path", "Canvas/StartButton" },
                            { "active", true },
                            { "tag", "Untagged" },
                            { "components", new List<string> { "Button", "Image", "Text" } }
                        },
                        new()
                        {
                            { "name", "QuitButton" },
                            { "path", "Canvas/QuitButton" },
                            { "active", false },
                            { "tag", "UI" },
                            { "components", new List<string> { "Button", "Image" } }
                        }
                    }
                }
            };

            // Act
            string result = OutputFormatter.FormatGameObjectSearch(data, jsonOutput: false);

            // Assert
            _ = result.Should().Contain("Search: name contains \"Button\"");
            _ = result.Should().Contain("Results: 2");
            _ = result.Should().Contain("1. [+] StartButton");
            _ = result.Should().Contain("Path: Canvas/StartButton");
            _ = result.Should().Contain("Components: Button, Image, Text");
            _ = result.Should().Contain("2. [-] QuitButton");
            _ = result.Should().Contain("Tag: UI");
        }

        [Fact]
        public void FormatGameObjectSearch_ZeroResults_ShowsZeroCount()
        {
            // Arrange
            Dictionary<string, object> data = new()
            {
                { "query", "name contains \"NonExistent\"" },
                { "resultCount", 0 },
                { "results", new List<Dictionary<string, object>>() }
            };

            // Act
            string result = OutputFormatter.FormatGameObjectSearch(data, jsonOutput: false);

            // Assert
            _ = result.Should().Contain("Results: 0");
        }

        [Fact]
        public void FormatGameObjectSearch_JsonMode_ReturnsJson()
        {
            // Arrange
            Dictionary<string, object> data = new()
            {
                { "query", "all" },
                { "resultCount", 0 },
                { "results", new List<Dictionary<string, object>>() }
            };

            // Act
            string result = OutputFormatter.FormatGameObjectSearch(data, jsonOutput: true);

            // Assert
            _ = result.Should().Contain("\"query\"");
            _ = result.Should().Contain("\"resultCount\"");
        }
    }
}
