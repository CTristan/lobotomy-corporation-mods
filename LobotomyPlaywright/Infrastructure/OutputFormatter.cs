// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text.Json;
using LobotomyPlaywright.Interfaces.Configuration;

namespace LobotomyPlaywright.Infrastructure;

/// <summary>
/// Formats query results for human-readable output.
/// </summary>
public static class OutputFormatter
{
    /// <summary>
    /// Formats agent data for display.
    /// </summary>
    /// <param name="agent">Agent data dictionary.</param>
    /// <param name="jsonOutput">Whether to output raw JSON.</param>
    /// <returns>Formatted string.</returns>
    public static string FormatAgent(Dictionary<string, object> agent, bool jsonOutput)
    {
        ArgumentNullException.ThrowIfNull(agent, nameof(agent));
        if (jsonOutput)
        {
            return JsonSerializer.Serialize(agent, new JsonSerializerOptions { WriteIndented = true });
        }

        var name = GetValue<string>(agent, "name", "Unknown");
        var instanceId = GetValue<int>(agent, "instanceId", 0);
        var hp = GetValue<double>(agent, "hp", 0);
        var maxHp = GetValue<double>(agent, "maxHp", 0);
        var mental = GetValue<double>(agent, "mental", 0);
        var maxMental = GetValue<double>(agent, "maxMental", 0);
        var fortitude = GetValue<int>(agent, "fortitude", 0);
        var prudence = GetValue<int>(agent, "prudence", 0);
        var temperance = GetValue<int>(agent, "temperance", 0);
        var justice = GetValue<int>(agent, "justice", 0);
        var state = GetValue<string>(agent, "state", "UNKNOWN");
        var currentSefira = GetValue<string>(agent, "currentSefira", "None");
        var isDead = GetValue<bool>(agent, "isDead", false);
        var isPanicking = GetValue<bool>(agent, "isPanicking", false);
        var giftIds = GetList<int>(agent, "giftIds");

        var status = isDead ? "DEAD" : isPanicking ? "PANIC" : "Normal";

        return $$"""
            Agent: {{name}} (ID: {{instanceId}})
              HP: {{hp:F0}}/{{maxHp:F0}}
              Mental: {{mental:F0}}/{{maxMental:F0}}
              Stats: Fortitude {{fortitude}}, Prudence {{prudence}},
                      Temperance {{temperance}}, Justice {{justice}}
              State: {{state}}
              Department: {{currentSefira}}
              Gifts: {{giftIds.Count}} equipped
              Status: {{status}}
            """.Trim();
    }

    /// <summary>
    /// Formats creature data for display.
    /// </summary>
    /// <param name="creature">Creature data dictionary.</param>
    /// <param name="jsonOutput">Whether to output raw JSON.</param>
    /// <returns>Formatted string.</returns>
    public static string FormatCreature(Dictionary<string, object> creature, bool jsonOutput)
    {
        ArgumentNullException.ThrowIfNull(creature, nameof(creature));
        if (jsonOutput)
        {
            return JsonSerializer.Serialize(creature, new JsonSerializerOptions { WriteIndented = true });
        }

        var name = GetValue<string>(creature, "name", "Unknown");
        var instanceId = GetValue<int>(creature, "instanceId", 0);
        var riskLevel = GetValue<string>(creature, "riskLevel", "UNKNOWN");
        var state = GetValue<string>(creature, "state", "UNKNOWN");
        var qliphoth = GetValue<int>(creature, "qliphothCounter", 0);
        var maxQliphoth = GetValue<int>(creature, "maxQliphothCounter", 0);
        var feelingState = GetValue<string>(creature, "feelingState", "UNKNOWN");
        var currentSefira = GetValue<string>(creature, "currentSefira", "None");
        var workCount = GetValue<int>(creature, "workCount", 0);
        var isEscaping = GetValue<bool>(creature, "isEscaping", false);
        var isSuppressed = GetValue<bool>(creature, "isSuppressed", false);

        var status = isEscaping ? "ESCAPING" : isSuppressed ? "SUPPRESSED" : "Normal";

        return $$"""
            Abnormality: {{name}} (ID: {{instanceId}})
              Risk Level: {{riskLevel}}
              State: {{state}}
              Qliphoth: {{qliphoth}}/{{maxQliphoth}}
              Feeling: {{feelingState}}
              Department: {{currentSefira}}
              Work Count: {{workCount}}
              Status: {{status}}
            """.Trim();
    }

    /// <summary>
    /// Formats game state for display.
    /// </summary>
    /// <param name="state">Game state dictionary.</param>
    /// <param name="jsonOutput">Whether to output raw JSON.</param>
    /// <returns>Formatted string.</returns>
    public static string FormatGameState(Dictionary<string, object> state, bool jsonOutput)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        if (jsonOutput)
        {
            return JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
        }

        var day = GetValue<int>(state, "day", 0);
        var gameState = GetValue<string>(state, "gameState", "UNKNOWN");
        var gameSpeed = GetValue<int>(state, "gameSpeed", 1);
        var energy = GetValue<double>(state, "energy", 0);
        var energyQuota = GetValue<double>(state, "energyQuota", 0);
        var emergencyLevel = GetValue<string>(state, "emergencyLevel", "NORMAL");
        var managementStarted = GetValue<bool>(state, "managementStarted", false);
        var isPaused = GetValue<bool>(state, "isPaused", false);
        var playTime = GetValue<double>(state, "playTime", 0);
        var lobPoints = GetValue<double>(state, "lobPoints", 0);

        var energyPct = energyQuota > 0 ? (energy / energyQuota) * 100 : 0;

        return $$"""
            Game State:
              Day: {{day}}
              Phase: {{gameState}}
              Speed: {{gameSpeed}}x
              Energy: {{energy:F1}}/{{energyQuota:F1}} ({{energyPct:F1}}%)
              Emergency: {{emergencyLevel}}
              Management Started: {{managementStarted}}
              Paused: {{isPaused}}
              Play Time: {{playTime:F1}}s
              LOB Points: {{lobPoints:F0}}
            """.Trim();
    }

    /// <summary>
    /// Formats department data for display.
    /// </summary>
    /// <param name="sefira">Department data dictionary.</param>
    /// <param name="jsonOutput">Whether to output raw JSON.</param>
    /// <returns>Formatted string.</returns>
    public static string FormatDepartment(Dictionary<string, object> sefira, bool jsonOutput)
    {
        ArgumentNullException.ThrowIfNull(sefira, nameof(sefira));
        if (jsonOutput)
        {
            return JsonSerializer.Serialize(sefira, new JsonSerializerOptions { WriteIndented = true });
        }

        var name = GetValue<string>(sefira, "name", "Unknown");
        var sefiraEnum = GetValue<string>(sefira, "sefiraEnum", "Unknown");
        var isOpen = GetValue<bool>(sefira, "isOpen", false);
        var openLevel = GetValue<int>(sefira, "openLevel", 0);
        var agentIds = GetList<int>(sefira, "agentIds");
        var creatureIds = GetList<int>(sefira, "creatureIds");
        var officerCount = GetValue<int>(sefira, "officerCount", 0);

        var status = isOpen ? $"Open (Level {openLevel})" : "Closed";

        return $$"""
            Department: {{name}} ({{sefiraEnum}})
              Status: {{status}}
              Agents: {{agentIds.Count}}
              Creatures: {{creatureIds.Count}}
              Officers: {{officerCount}}
            """.Trim();
    }

    /// <summary>
    /// Formats UI state for display.
    /// </summary>
    /// <param name="uiState">UI state dictionary.</param>
    /// <returns>Formatted string.</returns>
    public static string FormatUiState(Dictionary<string, object> uiState)
    {
        ArgumentNullException.ThrowIfNull(uiState, nameof(uiState));

        var windows = GetList<Dictionary<string, object>>(uiState, "windows");
        var activatedSlots = GetList<string>(uiState, "activatedSlots");
        var modElements = GetList<Dictionary<string, object>>(uiState, "modElements");

        var result = new System.Text.StringBuilder();
        result.AppendLine("UI Accessibility Tree:");
        result.AppendLine();

        // Format windows
        result.AppendLine($"Windows ({windows.Count}):");
        foreach (var window in windows)
        {
            var name = GetValue<string>(window, "name", "Unknown");
            var isOpen = GetValue<bool>(window, "isOpen", false);
            var windowType = GetValue<string>(window, "windowType", "Unknown");
            var children = GetList<Dictionary<string, object>>(window, "children");

            var status = isOpen ? "OPEN" : "CLOSED";
            result.AppendLine($"  [{status}] {name} ({windowType})");

            if (isOpen && children.Count > 0)
            {
                var displayChildren = children.Count > 10 ? children.GetRange(0, 10) : children;
                foreach (var child in displayChildren)
                {
                    var path = GetValue<string>(child, "path", "");
                    var type = GetValue<string>(child, "type", "other");
                    var value = GetValue<string>(child, "value", "");
                    var interactable = GetValue<bool>(child, "interactable", false);

                    var interactableSymbol = interactable ? "[*]" : "[ ]";
                    var displayValue = value.Length > 30 ? string.Concat(value.AsSpan(0, 27), "...") : value;

                    if (!string.IsNullOrEmpty(value))
                    {
                        result.AppendLine($"    {interactableSymbol} {type}: {path} = \"{displayValue}\"");
                    }
                    else
                    {
                        result.AppendLine($"    {interactableSymbol} {type}: {path}");
                    }
                }

                if (children.Count > 10)
                {
                    result.AppendLine($"    ... and {children.Count - 10} more elements");
                }
            }
        }

        result.AppendLine();

        // Format activated slots
        result.AppendLine("Activated Slots (0-4):");
        for (var i = 0; i < 5; i++)
        {
            var slot = i < activatedSlots.Count ? activatedSlots[i] : null;
            var slotDisplay = slot ?? "Empty";
            result.AppendLine($"  Slot {i}: {slotDisplay}");
        }

        result.AppendLine();

        // Format mod elements
        if (modElements.Count > 0)
        {
            result.AppendLine($"Mod Elements ({modElements.Count}):");
            foreach (var mod in modElements)
            {
                var path = GetValue<string>(mod, "path", "");
                var type = GetValue<string>(mod, "type", "other");
                var value = GetValue<string>(mod, "value", "");

                if (!string.IsNullOrEmpty(value))
                {
                    result.AppendLine($"  {type}: {path} = \"{value}\"");
                }
                else
                {
                    result.AppendLine($"  {type}: {path}");
                }
            }
        }
        else
        {
            result.AppendLine("Mod Elements: None detected");
        }

        return result.ToString().TrimEnd();
    }

    private static T GetValue<T>(Dictionary<string, object> dict, string key, T defaultValue)
    {
        if (dict.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }

        return defaultValue;
    }

    private static List<T> GetList<T>(Dictionary<string, object> dict, string key)
    {
        if (dict.TryGetValue(key, out var value) && value is List<T> list)
        {
            return list;
        }

        if (dict.TryGetValue(key, out var arrValue) && arrValue is object[] arr)
        {
            var result = new List<T>();
            foreach (var item in arr)
            {
                if (item is T typedItem)
                {
                    result.Add(typedItem);
                }
            }
            return result;
        }

        return new List<T>();
    }
}
