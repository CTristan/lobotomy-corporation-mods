// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace LobotomyPlaywright.Infrastructure
{
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

            var name = GetValue(agent, "name", "Unknown");
            var instanceId = GetValue(agent, "instanceId", 0);
            var hp = GetValue<double>(agent, "hp", 0);
            var maxHp = GetValue<double>(agent, "maxHp", 0);
            var mental = GetValue<double>(agent, "mental", 0);
            var maxMental = GetValue<double>(agent, "maxMental", 0);
            var fortitude = GetValue(agent, "fortitude", 0);
            var prudence = GetValue(agent, "prudence", 0);
            var temperance = GetValue(agent, "temperance", 0);
            var justice = GetValue(agent, "justice", 0);
            var state = GetValue(agent, "state", "UNKNOWN");
            var currentSefira = GetValue(agent, "currentSefira", "None");
            var isDead = GetValue(agent, "isDead", false);
            var isPanicking = GetValue(agent, "isPanicking", false);
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

            var name = GetValue(creature, "name", "Unknown");
            var instanceId = GetValue(creature, "instanceId", 0);
            var riskLevel = GetValue(creature, "riskLevel", "UNKNOWN");
            var state = GetValue(creature, "state", "UNKNOWN");
            var qliphoth = GetValue(creature, "qliphothCounter", 0);
            var maxQliphoth = GetValue(creature, "maxQliphothCounter", 0);
            var feelingState = GetValue(creature, "feelingState", "UNKNOWN");
            var currentSefira = GetValue(creature, "currentSefira", "None");
            var workCount = GetValue(creature, "workCount", 0);
            var isEscaping = GetValue(creature, "isEscaping", false);
            var isSuppressed = GetValue(creature, "isSuppressed", false);

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

            var day = GetValue(state, "day", 0);
            var gameState = GetValue(state, "gameState", "UNKNOWN");
            var gameSpeed = GetValue(state, "gameSpeed", 1);
            var energy = GetValue<double>(state, "energy", 0);
            var energyQuota = GetValue<double>(state, "energyQuota", 0);
            var emergencyLevel = GetValue(state, "emergencyLevel", "NORMAL");
            var managementStarted = GetValue(state, "managementStarted", false);
            var isPaused = GetValue(state, "isPaused", false);
            var playTime = GetValue<double>(state, "playTime", 0);
            var lobPoints = GetValue<double>(state, "lobPoints", 0);

            var energyPct = energyQuota > 0 ? energy / energyQuota * 100 : 0;

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

            var name = GetValue(sefira, "name", "Unknown");
            var sefiraEnum = GetValue(sefira, "sefiraEnum", "Unknown");
            var isOpen = GetValue(sefira, "isOpen", false);
            var openLevel = GetValue(sefira, "openLevel", 0);
            var agentIds = GetList<int>(sefira, "agentIds");
            var creatureIds = GetList<int>(sefira, "creatureIds");
            var officerCount = GetValue(sefira, "officerCount", 0);

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
            _ = result.AppendLine("UI Accessibility Tree:");
            _ = result.AppendLine();

            // Format windows
            _ = result.AppendLine($"Windows ({windows.Count}):");
            foreach (var window in windows)
            {
                var name = GetValue(window, "name", "Unknown");
                var isOpen = GetValue(window, "isOpen", false);
                var windowType = GetValue(window, "windowType", "Unknown");
                var children = GetList<Dictionary<string, object>>(window, "children");

                var status = isOpen ? "OPEN" : "CLOSED";
                _ = result.AppendLine($"  [{status}] {name} ({windowType})");

                if (isOpen && children.Count > 0)
                {
                    var displayChildren = children.Count > 10 ? children.GetRange(0, 10) : children;
                    foreach (var child in displayChildren)
                    {
                        var path = GetValue(child, "path", "");
                        var type = GetValue(child, "type", "other");
                        var value = GetValue(child, "value", "");
                        var interactable = GetValue(child, "interactable", false);

                        var interactableSymbol = interactable ? "[*]" : "[ ]";
                        var displayValue = value.Length > 30 ? string.Concat(value.AsSpan(0, 27), "...") : value;

                        if (!string.IsNullOrEmpty(value))
                        {
                            _ = result.AppendLine($"    {interactableSymbol} {type}: {path} = \"{displayValue}\"");
                        }
                        else
                        {
                            _ = result.AppendLine($"    {interactableSymbol} {type}: {path}");
                        }
                    }

                    if (children.Count > 10)
                    {
                        _ = result.AppendLine($"    ... and {children.Count - 10} more elements");
                    }
                }
            }

            _ = result.AppendLine();

            // Format activated slots
            _ = result.AppendLine("Activated Slots (0-4):");
            for (var i = 0; i < 5; i++)
            {
                var slot = i < activatedSlots.Count ? activatedSlots[i] : null;
                var slotDisplay = slot ?? "Empty";
                _ = result.AppendLine($"  Slot {i}: {slotDisplay}");
            }

            _ = result.AppendLine();

            // Format mod elements
            if (modElements.Count > 0)
            {
                _ = result.AppendLine($"Mod Elements ({modElements.Count}):");
                foreach (var mod in modElements)
                {
                    var path = GetValue(mod, "path", "");
                    var type = GetValue(mod, "type", "other");
                    var value = GetValue(mod, "value", "");

                    if (!string.IsNullOrEmpty(value))
                    {
                        _ = result.AppendLine($"  {type}: {path} = \"{value}\"");
                    }
                    else
                    {
                        _ = result.AppendLine($"  {type}: {path}");
                    }
                }
            }
            else
            {
                _ = result.AppendLine("Mod Elements: None detected");
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

            return [];
        }
    }
}
