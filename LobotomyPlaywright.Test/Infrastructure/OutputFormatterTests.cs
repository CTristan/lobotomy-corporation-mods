// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyPlaywright.Infrastructure;
using Xunit;

namespace LobotomyPlaywright.Tests.Infrastructure;

public sealed class OutputFormatterTests
{
    [Fact]
    public void FormatAgent_WithJsonOutput_ReturnsJsonString()
    {
        // Arrange
        var agent = new Dictionary<string, object>
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
        var result = OutputFormatter.FormatAgent(agent, jsonOutput: true);

        // Assert
        result.Should().Contain("Test Agent");
        result.Should().Contain("100");
    }

    [Fact]
    public void FormatAgent_WithFormattedOutput_ReturnsFormattedString()
    {
        // Arrange
        var agent = new Dictionary<string, object>
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
        var result = OutputFormatter.FormatAgent(agent, jsonOutput: false);

        // Assert
        result.Should().Contain("Agent: Test Agent (ID: 1)");
        result.Should().Contain("HP: 100/100");
        result.Should().Contain("Mental: 80/100");
        result.Should().Contain("Stats: Fortitude 50, Prudence 60,");
        result.Should().Contain("State: IDLE");
        result.Should().Contain("Department: Control");
        result.Should().Contain("Status: Normal");
    }

    [Fact]
    public void FormatCreature_WithFormattedOutput_ReturnsFormattedString()
    {
        // Arrange
        var creature = new Dictionary<string, object>
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
        var result = OutputFormatter.FormatCreature(creature, jsonOutput: false);

        // Assert
        result.Should().Contain("Abnormality: Test Creature (ID: 100)");
        result.Should().Contain("Risk Level: WAW");
        result.Should().Contain("Qliphoth: 2/5");
        result.Should().Contain("Work Count: 10");
        result.Should().Contain("Status: Normal");
    }

    [Fact]
    public void FormatGameState_WithFormattedOutput_ReturnsFormattedString()
    {
        // Arrange
        var gameState = new Dictionary<string, object>
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
        var result = OutputFormatter.FormatGameState(gameState, jsonOutput: false);

        // Assert
        result.Should().Contain("Day: 5");
        result.Should().Contain("Phase: PLAYING");
        result.Should().Contain("Speed: 1x");
        result.Should().Contain("Energy: 500.0/1000.0 (50.0%)");
        result.Should().Contain("Emergency: NORMAL");
    }

    [Fact]
    public void FormatDepartment_WithFormattedOutput_ReturnsFormattedString()
    {
        // Arrange
        var department = new Dictionary<string, object>
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
        var result = OutputFormatter.FormatDepartment(department, jsonOutput: false);

        // Assert
        result.Should().Contain("Department: Control Team (CONTROL)");
        result.Should().Contain("Status: Open (Level 3)");
        result.Should().Contain("Agents: 3");
        result.Should().Contain("Creatures: 2");
        result.Should().Contain("Officers: 2");
    }
}
