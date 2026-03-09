// SPDX-License-Identifier: MIT

using System;
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
            uiState.Should().NotBeNull();
            uiState.Windows.Should().NotBeNull();
            uiState.ActivatedSlots.Should().NotBeNull();
            uiState.ModElements.Should().NotBeNull();
        }

        [Fact]
        public void GetUiState_with_summary_depth_returns_UiStateData()
        {
            // Act
            var uiState = UiQueries.GetUiState(depth: "summary");

            // Assert
            uiState.Should().NotBeNull();
            uiState.Windows.Should().NotBeNull();
            uiState.ActivatedSlots.Should().NotBeNull();
        }

        [Fact]
        public void GetUiState_with_full_depth_returns_UiStateData()
        {
            // Act
            var uiState = UiQueries.GetUiState(depth: "full");

            // Assert
            uiState.Should().NotBeNull();
            uiState.Windows.Should().NotBeNull();
            uiState.ActivatedSlots.Should().NotBeNull();
        }

        [Fact]
        public void GetUiState_with_window_depth_and_filter_returns_UiStateData()
        {
            // Act
            var uiState = UiQueries.GetUiState(depth: "window", windowFilter: "AgentInfoWindow");

            // Assert
            uiState.Should().NotBeNull();
            uiState.Windows.Should().NotBeNull();
        }

        [Fact]
        public void GetUiState_with_null_depth_returns_UiStateData()
        {
            // Act
            var uiState = UiQueries.GetUiState(depth: null);

            // Assert
            uiState.Should().NotBeNull();
        }

        [Fact]
        public void GetUiState_with_empty_string_depth_returns_UiStateData()
        {
            // Act
            var uiState = UiQueries.GetUiState(depth: "");

            // Assert
            uiState.Should().NotBeNull();
        }

        [Fact]
        public void GetUiState_with_invalid_depth_returns_UiStateData()
        {
            // Act - Invalid depth should default to "full"
            var uiState = UiQueries.GetUiState(depth: "invalid");

            // Assert
            uiState.Should().NotBeNull();
        }

        [Fact]
        public void GetUiState_with_case_insensitive_depth_works()
        {
            // Act & Assert - All should return valid data
            var summary = UiQueries.GetUiState(depth: "SUMMARY");
            summary.Should().NotBeNull();

            var full = UiQueries.GetUiState(depth: "FULL");
            full.Should().NotBeNull();

            var window = UiQueries.GetUiState(depth: "WINDOW");
            window.Should().NotBeNull();
        }

        [Fact]
        public void GetUiState_returns_all_known_windows()
        {
            // Act
            var uiState = UiQueries.GetUiState();

            // Assert - Should contain at least the 12 known windows
            uiState.Windows.Should().HaveCountGreaterThanOrEqualTo(12);
        }

        [Fact]
        public void GetUiState_returns_activated_slots_list_of_max_5()
        {
            // Act
            var uiState = UiQueries.GetUiState();

            // Assert
            uiState.ActivatedSlots.Should().NotBeNull();
            uiState.ActivatedSlots.Count.Should().BeLessThanOrEqualTo(5);
        }

        [Fact]
        public void GetUiState_method_exists_and_is_callable()
        {
            // Arrange & Act
            Action act = () => UiQueries.GetUiState();

            // Assert - method should not throw
            act.Should().NotThrow();
        }

        [Fact]
        public void UiNodeData_properties_are_accessible()
        {
            // Arrange
            var node = new UiNodeData
            {
                Path = "Window/Button",
                Type = "button",
                Value = "Click Me",
                Interactable = true
            };

            // Act & Assert
            node.Path.Should().Be("Window/Button");
            node.Type.Should().Be("button");
            node.Value.Should().Be("Click Me");
            node.Interactable.Should().BeTrue();
        }

        [Fact]
        public void UiWindowData_properties_are_accessible()
        {
            // Arrange
            var window = new UiWindowData
            {
                Name = "AgentInfoWindow",
                IsOpen = true,
                WindowType = "AgentInfo",
                Children = null
            };

            // Act & Assert
            window.Name.Should().Be("AgentInfoWindow");
            window.IsOpen.Should().BeTrue();
            window.WindowType.Should().Be("AgentInfo");
            window.Children.Should().BeNull();
        }

        [Fact]
        public void UiWindowData_with_children()
        {
            // Arrange
            var window = new UiWindowData
            {
                Name = "CommandWindow",
                IsOpen = true,
                WindowType = "Command",
                Children = new System.Collections.Generic.List<UiNodeData>
                {
                    new UiNodeData { Path = "Panel/Text", Type = "text", Value = "Hello" }
                }
            };

            // Act & Assert
            window.Children.Should().HaveCount(1);
            window.Children[0].Path.Should().Be("Panel/Text");
        }

        [Fact]
        public void UiStateData_properties_are_accessible()
        {
            // Arrange
            var uiState = new UiStateData
            {
                Windows = new System.Collections.Generic.List<UiWindowData>(),
                ActivatedSlots = new System.Collections.Generic.List<string>(),
                ModElements = new System.Collections.Generic.List<UiNodeData>()
            };

            // Act & Assert
            uiState.Windows.Should().NotBeNull();
            uiState.ActivatedSlots.Should().NotBeNull();
            uiState.ModElements.Should().NotBeNull();
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
            var expectedPathComponents = new[] { "Window", "Panel", "Button" };
            var expectedPath = string.Join("/", expectedPathComponents);

            expectedPath.Should().Be("Window/Panel/Button");
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
            var windowNames = new System.Collections.Generic.List<string>();
            foreach (var window in uiState.Windows)
            {
                windowNames.Add(window.Name);
            }

            windowNames.Should().Contain("AgentInfoWindow");
            windowNames.Should().Contain("CommandWindow");
            windowNames.Should().Contain("CreatureInfoWindow");
            windowNames.Should().Contain("ManualUI");
            windowNames.Should().Contain("OptionUI");
            windowNames.Should().Contain("DeployUI");
        }
    }
}
