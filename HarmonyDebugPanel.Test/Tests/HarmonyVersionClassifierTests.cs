// SPDX-License-Identifier: MIT

using AwesomeAssertions;
using HarmonyDebugPanel.Implementations.Collectors;
using HarmonyDebugPanel.Models;
using Xunit;

namespace HarmonyDebugPanel.Test.Tests
{
    public sealed class HarmonyVersionClassifierTests
    {
        [Fact]
        public void Classify_ReturnsUnknown_WhenReferencesNull()
        {
            HarmonyVersionClassifier classifier = new();

            var version = classifier.Classify(null);

            _ = version.Should().Be(HarmonyVersion.Unknown);
        }

        [Fact]
        public void Classify_ReturnsHarmony1_When0Harmony109ReferencePresent()
        {
            HarmonyVersionClassifier classifier = new();

            var version = classifier.Classify(
            [
                new("0Harmony109"),
            ]);

            _ = version.Should().Be(HarmonyVersion.Harmony1);
        }

        [Fact]
        public void Classify_ReturnsHarmony2_When0HarmonyReferencePresent()
        {
            HarmonyVersionClassifier classifier = new();

            var version = classifier.Classify(
            [
                new("0Harmony"),
            ]);

            _ = version.Should().Be(HarmonyVersion.Harmony2);
        }

        [Fact]
        public void Classify_PrefersHarmony1_WhenBothReferencesPresent()
        {
            HarmonyVersionClassifier classifier = new();

            var version = classifier.Classify(
            [
                new("0Harmony"),
                new("0Harmony109"),
            ]);

            _ = version.Should().Be(HarmonyVersion.Harmony1);
        }
    }
}
