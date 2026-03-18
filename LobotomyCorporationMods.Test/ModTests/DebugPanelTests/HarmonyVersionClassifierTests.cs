// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Reflection;
using AwesomeAssertions;
using Hemocode.DebugPanel.Implementations;
using Hemocode.Common.Enums.Diagnostics;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class HarmonyVersionClassifierTests
    {
        [Fact]
        public void Classify_returns_Unknown_when_references_is_null()
        {
            var classifier = new HarmonyVersionClassifier();

            var result = classifier.Classify(null);

            result.Should().Be(HarmonyVersion.Unknown);
        }

        [Fact]
        public void Classify_returns_Unknown_when_references_is_empty()
        {
            var classifier = new HarmonyVersionClassifier();

            var result = classifier.Classify([]);

            result.Should().Be(HarmonyVersion.Unknown);
        }

        [Fact]
        public void Classify_returns_Harmony1_when_references_contain_0Harmony109()
        {
            var classifier = new HarmonyVersionClassifier();
            var references = new List<AssemblyName> { new("0Harmony109") };

            var result = classifier.Classify(references);

            result.Should().Be(HarmonyVersion.Harmony1);
        }

        [Fact]
        public void Classify_returns_Harmony1_when_references_contain_12Harmony()
        {
            var classifier = new HarmonyVersionClassifier();
            var references = new List<AssemblyName> { new("12Harmony") };

            var result = classifier.Classify(references);

            result.Should().Be(HarmonyVersion.Harmony1);
        }

        [Fact]
        public void Classify_returns_Harmony1_when_references_contain_0Harmony12()
        {
            var classifier = new HarmonyVersionClassifier();
            var references = new List<AssemblyName> { new("0Harmony12") };

            var result = classifier.Classify(references);

            result.Should().Be(HarmonyVersion.Harmony1);
        }

        [Fact]
        public void Classify_returns_Harmony2_when_references_contain_0Harmony()
        {
            var classifier = new HarmonyVersionClassifier();
            var references = new List<AssemblyName> { new("0Harmony") };

            var result = classifier.Classify(references);

            result.Should().Be(HarmonyVersion.Harmony2);
        }

        [Fact]
        public void Classify_returns_Harmony1_when_both_Harmony1_and_Harmony2_references_are_present()
        {
            var classifier = new HarmonyVersionClassifier();
            var references = new List<AssemblyName>
            {
                new("0Harmony109"),
                new("0Harmony"),
            };

            var result = classifier.Classify(references);

            result.Should().Be(HarmonyVersion.Harmony1);
        }

        [Fact]
        public void Classify_skips_null_entries_in_references()
        {
            var classifier = new HarmonyVersionClassifier();
            var references = new List<AssemblyName>
            {
                null!,
                new("0Harmony"),
            };

            var result = classifier.Classify(references);

            result.Should().Be(HarmonyVersion.Harmony2);
        }
    }
}
