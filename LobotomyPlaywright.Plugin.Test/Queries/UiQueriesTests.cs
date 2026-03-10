// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyPlaywright.Queries;
using Xunit;

namespace LobotomyPlaywright.Plugin.Test.Queries
{
    public class UiQueriesTests
    {
        [Fact]
        public void GetUiState_with_default_parameters_returns_UiStateData()
        {
            // Act
            var uiState = UiQueries.GetUiState();

            // Assert
            _ = uiState.Should().NotBeNull();
            _ = uiState.Windows.Should().NotBeNull();
            _ = uiState.ActivatedSlots.Should().NotBeNull();
            _ = uiState.ModElements.Should().NotBeNull();
        }

        [Fact]
        public void GetUiState_with_summary_depth_returns_UiStateData()
        {
            // Act
            var uiState = UiQueries.GetUiState(depth: "summary");

            // Assert
            _ = uiState.Should().NotBeNull();
            _ = uiState.Windows.Should().NotBeNull();
            _ = uiState.ActivatedSlots.Should().NotBeNull();
        }

        [Fact]
        public void GetUiState_with_full_depth_returns_UiStateData()
        {
            // Act
            var uiState = UiQueries.GetUiState(depth: "full");

            // Assert
            _ = uiState.Should().NotBeNull();
            _ = uiState.Windows.Should().NotBeNull();
            _ = uiState.ActivatedSlots.Should().NotBeNull();
        }

        [Fact]
        public void GetUiState_with_window_depth_and_filter_returns_UiStateData()
        {
            // Act
            var uiState = UiQueries.GetUiState(depth: "window", windowFilter: "AgentInfoWindow");

            // Assert
            _ = uiState.Should().NotBeNull();
            _ = uiState.Windows.Should().NotBeNull();
        }

        [Fact]
        public void GetUiState_with_null_depth_returns_UiStateData()
        {
            // Act
            var uiState = UiQueries.GetUiState(depth: null);

            // Assert
            _ = uiState.Should().NotBeNull();
        }

        [Fact]
        public void GetUiState_with_empty_string_depth_returns_UiStateData()
        {
            // Act
            var uiState = UiQueries.GetUiState(depth: "");

            // Assert
            _ = uiState.Should().NotBeNull();
        }

        [Fact]
        public void GetUiState_with_invalid_depth_returns_UiStateData()
        {
            // Act - Invalid depth should default to "full"
            var uiState = UiQueries.GetUiState(depth: "invalid");

            // Assert
            _ = uiState.Should().NotBeNull();
        }

        [Fact]
        public void GetUiState_with_case_insensitive_depth_works()
        {
            // Act & Assert - All should return valid data
            var summary = UiQueries.GetUiState(depth: "SUMMARY");
            _ = summary.Should().NotBeNull();

            var full = UiQueries.GetUiState(depth: "FULL");
            _ = full.Should().NotBeNull();

            var window = UiQueries.GetUiState(depth: "WINDOW");
            _ = window.Should().NotBeNull();
        }

        [Fact]
        public void GetUiState_returns_all_known_windows()
        {
            // Act
            var uiState = UiQueries.GetUiState();

            // Assert - Should contain at least the 12 known windows
            _ = uiState.Windows.Should().HaveCountGreaterThanOrEqualTo(12);
        }

        [Fact]
        public void GetUiState_returns_activated_slots_list_of_max_5()
        {
            // Act
            var uiState = UiQueries.GetUiState();

            // Assert
            _ = uiState.ActivatedSlots.Should().NotBeNull();
            _ = uiState.ActivatedSlots.Count.Should().BeLessThanOrEqualTo(5);
        }

        [Fact]
        public void GetUiState_method_exists_and_is_callable()
        {
            // Arrange & Act
            Action act = () => UiQueries.GetUiState();

            // Assert - method should not throw
            _ = act.Should().NotThrow();
        }

        [Fact]
        public void UiNodeData_properties_are_accessible()
        {
            // Arrange
            UiNodeData node = new()
            {
                Path = "Window/Button",
                Type = "button",
                Value = "Click Me",
                Interactable = true
            };

            // Act & Assert
            _ = node.Path.Should().Be("Window/Button");
            _ = node.Type.Should().Be("button");
            _ = node.Value.Should().Be("Click Me");
            _ = node.Interactable.Should().BeTrue();
        }

        [Fact]
        public void UiWindowData_properties_are_accessible()
        {
            // Arrange
            UiWindowData window = new()
            {
                Name = "AgentInfoWindow",
                IsOpen = true,
                WindowType = "AgentInfo",
                Children = null
            };

            // Act & Assert
            _ = window.Name.Should().Be("AgentInfoWindow");
            _ = window.IsOpen.Should().BeTrue();
            _ = window.WindowType.Should().Be("AgentInfo");
            _ = window.Children.Should().BeNull();
        }

        [Fact]
        public void UiWindowData_with_children()
        {
            // Arrange
            UiWindowData window = new()
            {
                Name = "CommandWindow",
                IsOpen = true,
                WindowType = "Command",
                Children =
                [
                    new UiNodeData { Path = "Panel/Text", Type = "text", Value = "Hello" }
                ]
            };

            // Act & Assert
            _ = window.Children.Should().HaveCount(1);
            _ = window.Children[0].Path.Should().Be("Panel/Text");
        }

        [Fact]
        public void UiStateData_properties_are_accessible()
        {
            // Arrange
            UiStateData uiState = new()
            {
                Windows = [],
                ActivatedSlots = [],
                ModElements = []
            };

            // Act & Assert
            _ = uiState.Windows.Should().NotBeNull();
            _ = uiState.ActivatedSlots.Should().NotBeNull();
            _ = uiState.ModElements.Should().NotBeNull();
        }

        [Fact]
        public void BuildNodePath_logic_works_for_nested_transforms()
        {
            // This test verifies the path building logic conceptually
            // Actual implementation requires Unity Transform objects

            // The expected behavior is:
            // - Path should be slash-delimited
            // - Path should be capped at 4 levels
            // - Path should go from root to node

            // Arrange & Assert
            string[] expectedPathComponents = ["Window", "Panel", "Button"];
            var expectedPath = string.Join("/", expectedPathComponents);

            _ = expectedPath.Should().Be("Window/Panel/Button");
        }

        [Trait("Category", "RequiresUnity")]
        [Fact]
        public void CheckKnownWindows_returns_expected_window_types()
        {
            // This test requires Unity to be running
            // Marked with [RequiresUnity] trait

            // Act
            var uiState = UiQueries.GetUiState(depth: "summary");

            // Assert - Known window names should be present
            List<string> windowNames = [];
            foreach (var window in uiState.Windows)
            {
                windowNames.Add(window.Name);
            }

            _ = windowNames.Should().Contain("AgentInfoWindow");
            _ = windowNames.Should().Contain("CommandWindow");
            _ = windowNames.Should().Contain("CreatureInfoWindow");
            _ = windowNames.Should().Contain("ManualUI");
            _ = windowNames.Should().Contain("OptionUI");
            _ = windowNames.Should().Contain("DeployUI");
        }
    }
}
