// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using AwesomeAssertions;
using Xunit;

namespace LobotomyPlaywright.Tests.Infrastructure
{
    public sealed class OutputFormatterTests
    {
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
    }
}
