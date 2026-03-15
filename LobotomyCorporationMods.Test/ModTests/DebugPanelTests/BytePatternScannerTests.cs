// SPDX-License-Identifier: MIT

#region

using System;
using System.Text;
using AwesomeAssertions;
using LobotomyCorporationMods.DebugPanel.Implementations;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class BytePatternScannerTests
    {
        private readonly BytePatternScanner _scanner;

        public BytePatternScannerTests()
        {
            _scanner = new BytePatternScanner();
        }

        [Fact]
        public void FindHarmonyReferences_throws_when_dllBytes_is_null()
        {
            Action act = () => _scanner.FindHarmonyReferences(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("dllBytes");
        }

        [Fact]
        public void FindHarmonyReferences_returns_empty_list_for_empty_byte_array()
        {
            var result = _scanner.FindHarmonyReferences([]);

            result.Should().BeEmpty();
        }

        [Fact]
        public void FindHarmonyReferences_detects_0Harmony_reference()
        {
            var bytes = Encoding.UTF8.GetBytes("some prefix 0Harmony some suffix");

            var result = _scanner.FindHarmonyReferences(bytes);

            result.Should().ContainSingle().Which.Should().Be("0Harmony");
        }

        [Fact]
        public void FindHarmonyReferences_detects_0Harmony109_reference()
        {
            var bytes = Encoding.UTF8.GetBytes("some prefix 0Harmony109 some suffix");

            var result = _scanner.FindHarmonyReferences(bytes);

            result.Should().ContainSingle().Which.Should().Be("0Harmony109");
        }

        [Fact]
        public void FindHarmonyReferences_detects_0Harmony12_reference()
        {
            var bytes = Encoding.UTF8.GetBytes("some prefix 0Harmony12 some suffix");

            var result = _scanner.FindHarmonyReferences(bytes);

            result.Should().ContainSingle().Which.Should().Be("0Harmony12");
        }

        [Fact]
        public void FindHarmonyReferences_detects_12Harmony_reference()
        {
            var bytes = Encoding.UTF8.GetBytes("some prefix 12Harmony some suffix");

            var result = _scanner.FindHarmonyReferences(bytes);

            result.Should().ContainSingle().Which.Should().Be("12Harmony");
        }

        [Fact]
        public void FindHarmonyReferences_matches_longer_pattern_first_preventing_false_0Harmony_match_from_0Harmony109()
        {
            var bytes = Encoding.UTF8.GetBytes("ref 0Harmony109 end");

            var result = _scanner.FindHarmonyReferences(bytes);

            result.Should().ContainSingle().Which.Should().Be("0Harmony109");
        }

        [Fact]
        public void FindHarmonyReferences_matches_longer_pattern_first_preventing_false_0Harmony_match_from_0Harmony12()
        {
            var bytes = Encoding.UTF8.GetBytes("ref 0Harmony12 end");

            var result = _scanner.FindHarmonyReferences(bytes);

            result.Should().ContainSingle().Which.Should().Be("0Harmony12");
        }

        [Fact]
        public void FindHarmonyReferences_detects_multiple_distinct_references()
        {
            var bytes = Encoding.UTF8.GetBytes("first 0Harmony109 middle 0Harmony end");

            var result = _scanner.FindHarmonyReferences(bytes);

            result.Should().HaveCount(2);
            result.Should().Contain("0Harmony109");
            result.Should().Contain("0Harmony");
        }

        [Fact]
        public void FindHarmonyReferences_returns_empty_list_when_no_patterns_found()
        {
            var bytes = Encoding.UTF8.GetBytes("this dll has no harmony references at all");

            var result = _scanner.FindHarmonyReferences(bytes);

            result.Should().BeEmpty();
        }

        [Fact]
        public void FindHarmonyReferences_reports_pattern_only_once_even_with_multiple_occurrences()
        {
            var bytes = Encoding.UTF8.GetBytes("first 0Harmony then another 0Harmony end");

            var result = _scanner.FindHarmonyReferences(bytes);

            result.Should().ContainSingle().Which.Should().Be("0Harmony");
        }

        [Fact]
        public void FindHarmonyReferences_returns_empty_for_bytes_shorter_than_shortest_pattern()
        {
            var bytes = "ABC"u8.ToArray();

            var result = _scanner.FindHarmonyReferences(bytes);

            result.Should().BeEmpty();
        }

        [Fact]
        public void FindHarmonyReferences_detects_pattern_at_start_of_bytes()
        {
            var bytes = Encoding.UTF8.GetBytes("0Harmony rest of data");

            var result = _scanner.FindHarmonyReferences(bytes);

            result.Should().ContainSingle().Which.Should().Be("0Harmony");
        }

        [Fact]
        public void FindHarmonyReferences_detects_pattern_at_end_of_bytes()
        {
            var bytes = Encoding.UTF8.GetBytes("some data 0Harmony");

            var result = _scanner.FindHarmonyReferences(bytes);

            result.Should().ContainSingle().Which.Should().Be("0Harmony");
        }
    }
}
