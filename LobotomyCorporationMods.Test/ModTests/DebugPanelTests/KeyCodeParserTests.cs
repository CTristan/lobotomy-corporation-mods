// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using Hemocode.DebugPanel.Implementations;
using UnityEngine;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class KeyCodeParserTests
    {
        [Fact]
        public void Parse_returns_F9_for_F9_string()
        {
            var result = KeyCodeParser.Parse("F9");

            _ = result.Should().Be(KeyCode.F9);
        }

        [Fact]
        public void Parse_returns_F10_for_F10_string()
        {
            var result = KeyCodeParser.Parse("F10");

            _ = result.Should().Be(KeyCode.F10);
        }

        [Fact]
        public void Parse_returns_F9_default_for_null_input()
        {
            var result = KeyCodeParser.Parse(null);

            _ = result.Should().Be(KeyCode.F9);
        }

        [Fact]
        public void Parse_returns_F9_default_for_empty_input()
        {
            var result = KeyCodeParser.Parse(string.Empty);

            _ = result.Should().Be(KeyCode.F9);
        }

        [Fact]
        public void Parse_returns_F9_default_for_whitespace_input()
        {
            var result = KeyCodeParser.Parse("   ");

            _ = result.Should().Be(KeyCode.F9);
        }

        [Fact]
        public void Parse_returns_F9_default_for_unrecognized_input()
        {
            var result = KeyCodeParser.Parse("UnknownKey");

            _ = result.Should().Be(KeyCode.F9);
        }

        [Fact]
        public void Parse_is_case_insensitive()
        {
            var result = KeyCodeParser.Parse("f10");

            _ = result.Should().Be(KeyCode.F10);
        }

        [Theory]
        [InlineData("F1", KeyCode.F1)]
        [InlineData("F2", KeyCode.F2)]
        [InlineData("F3", KeyCode.F3)]
        [InlineData("F4", KeyCode.F4)]
        [InlineData("F5", KeyCode.F5)]
        [InlineData("F6", KeyCode.F6)]
        [InlineData("F7", KeyCode.F7)]
        [InlineData("F8", KeyCode.F8)]
        [InlineData("F9", KeyCode.F9)]
        [InlineData("F10", KeyCode.F10)]
        [InlineData("F11", KeyCode.F11)]
        [InlineData("F12", KeyCode.F12)]
        public void Parse_returns_correct_value_for_function_keys(string keyName, KeyCode expected)
        {
            var result = KeyCodeParser.Parse(keyName);

            _ = result.Should().Be(expected);
        }

        [Theory]
        [InlineData("Space", KeyCode.Space)]
        [InlineData("Return", KeyCode.Return)]
        [InlineData("Escape", KeyCode.Escape)]
        [InlineData("Tab", KeyCode.Tab)]
        [InlineData("Backspace", KeyCode.Backspace)]
        [InlineData("Delete", KeyCode.Delete)]
        [InlineData("Home", KeyCode.Home)]
        [InlineData("End", KeyCode.End)]
        [InlineData("PageUp", KeyCode.PageUp)]
        [InlineData("PageDown", KeyCode.PageDown)]
        public void Parse_returns_correct_value_for_common_keys(string keyName, KeyCode expected)
        {
            var result = KeyCodeParser.Parse(keyName);

            _ = result.Should().Be(expected);
        }

        [Theory]
        [InlineData("Alpha0", KeyCode.Alpha0)]
        [InlineData("Alpha1", KeyCode.Alpha1)]
        [InlineData("Alpha9", KeyCode.Alpha9)]
        public void Parse_returns_correct_value_for_alpha_keys(string keyName, KeyCode expected)
        {
            var result = KeyCodeParser.Parse(keyName);

            _ = result.Should().Be(expected);
        }

        [Theory]
        [InlineData("UpArrow", KeyCode.UpArrow)]
        [InlineData("DownArrow", KeyCode.DownArrow)]
        [InlineData("LeftArrow", KeyCode.LeftArrow)]
        [InlineData("RightArrow", KeyCode.RightArrow)]
        public void Parse_returns_correct_value_for_arrow_keys(string keyName, KeyCode expected)
        {
            var result = KeyCodeParser.Parse(keyName);

            _ = result.Should().Be(expected);
        }
    }
}
