// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using HarmonyDebugPanel.Implementations.Collectors;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;
using Xunit;

namespace HarmonyDebugPanel.Test;

public sealed class HarmonyVersionClassifierTests
{
    [Fact]
    public void Classify_ReturnsUnknown_WhenReferencesNull()
    {
        var classifier = new HarmonyVersionClassifier();

        var version = classifier.Classify(null);

        version.Should().Be(HarmonyVersion.Unknown);
    }

    [Fact]
    public void Classify_ReturnsHarmony1_When0Harmony109ReferencePresent()
    {
        var classifier = new HarmonyVersionClassifier();

        var version = classifier.Classify(new List<AssemblyName>
        {
            new("0Harmony109"),
        });

        version.Should().Be(HarmonyVersion.Harmony1);
    }

    [Fact]
    public void Classify_ReturnsHarmony2_When0HarmonyReferencePresent()
    {
        var classifier = new HarmonyVersionClassifier();

        var version = classifier.Classify(new List<AssemblyName>
        {
            new("0Harmony"),
        });

        version.Should().Be(HarmonyVersion.Harmony2);
    }

    [Fact]
    public void Classify_PrefersHarmony1_WhenBothReferencesPresent()
    {
        var classifier = new HarmonyVersionClassifier();

        var version = classifier.Classify(new List<AssemblyName>
        {
            new("0Harmony"),
            new("0Harmony109"),
        });

        version.Should().Be(HarmonyVersion.Harmony1);
    }
}
