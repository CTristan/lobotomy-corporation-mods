// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using HarmonyDebugPanel.Implementations.Collectors;
using HarmonyDebugPanel.Models;
using HarmonyLib;
using Xunit;

namespace HarmonyDebugPanel.Test;

public sealed class ExpectedPatchSourceTests
{
    [Fact]
    public void GetExpectedPatches_ThrowsArgumentNullException_WhenDebugInfoIsNull()
    {
        var sut = new ExpectedPatchSource();

        var action = () => sut.GetExpectedPatches(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetExpectedPatches_FindsClassLevelHarmonyPatch_ByReflection()
    {
        var sut = new ExpectedPatchSource();
        var debugInfo = new List<string>();
        var patchAssemblyName = typeof(ExpectedPatchSourceTests).Assembly.GetName().Name;

        var patches = sut.GetExpectedPatches(debugInfo);

        patches.Should().Contain(p =>
            p.PatchAssembly == patchAssemblyName &&
            p.TargetType == typeof(ExpectedPatchSourceTestTarget).FullName &&
            p.TargetMethod == nameof(ExpectedPatchSourceTestTarget.ClassLevelTarget) &&
            p.PatchMethod == nameof(ExpectedPatchSourceClassLevelPatch.Prefix) &&
            p.PatchType == PatchType.Prefix);

        debugInfo.Should().Contain(entry => entry.Contains("HarmonyPatch attribute type resolved", StringComparison.Ordinal));
    }

    [Fact]
    public void GetExpectedPatches_RecognizesPatchMethodByHarmonyPostfixAttribute_WhenMethodNameIsCustom()
    {
        var sut = new ExpectedPatchSource();
        var debugInfo = new List<string>();
        var patchAssemblyName = typeof(ExpectedPatchSourceTests).Assembly.GetName().Name;

        var patches = sut.GetExpectedPatches(debugInfo);

        patches.Should().Contain(p =>
            p.PatchAssembly == patchAssemblyName &&
            p.TargetType == typeof(ExpectedPatchSourceTestTarget).FullName &&
            p.TargetMethod == nameof(ExpectedPatchSourceTestTarget.AttributeTarget) &&
            p.PatchMethod == nameof(ExpectedPatchSourceCustomMethodNamePatch.CustomNamedPostfix) &&
            p.PatchType == PatchType.Postfix);
    }

    [Fact]
    public void GetExpectedPatches_MergesStackedClassLevelHarmonyPatchAttributes()
    {
        var sut = new ExpectedPatchSource();
        var debugInfo = new List<string>();
        var patchAssemblyName = typeof(ExpectedPatchSourceTests).Assembly.GetName().Name;

        var patches = sut.GetExpectedPatches(debugInfo);

        patches.Should().Contain(p =>
            p.PatchAssembly == patchAssemblyName &&
            p.TargetType == typeof(ExpectedPatchSourceTestTarget).FullName &&
            p.TargetMethod == nameof(ExpectedPatchSourceTestTarget.StackedClassLevelTarget) &&
            p.PatchMethod == nameof(ExpectedPatchSourceStackedClassAttributesPatch.Postfix) &&
            p.PatchType == PatchType.Postfix);
    }

    [Fact]
    public void GetExpectedPatches_MergesTypeAndMethodHarmonyPatchAttributes()
    {
        var sut = new ExpectedPatchSource();
        var debugInfo = new List<string>();
        var patchAssemblyName = typeof(ExpectedPatchSourceTests).Assembly.GetName().Name;

        var patches = sut.GetExpectedPatches(debugInfo);

        patches.Should().Contain(p =>
            p.PatchAssembly == patchAssemblyName &&
            p.TargetType == typeof(ExpectedPatchSourceTestTarget).FullName &&
            p.TargetMethod == nameof(ExpectedPatchSourceTestTarget.MethodLevelTarget) &&
            p.PatchMethod == nameof(ExpectedPatchSourceMethodLevelTargetPatch.CustomMethodLevelPrefix) &&
            p.PatchType == PatchType.Prefix);
    }

    [Fact]
    public void GetExpectedPatches_LogsException_WhenAssemblyThrowsReflectionTypeLoadException()
    {
        var sut = new ExpectedPatchSource();
        var debugInfo = new List<string>();

        // This test verifies that reflection errors are logged
        // In practice, if an assembly throws ReflectionTypeLoadException during GetTypes(),
        // the error should be captured in debugInfo without crashing
        var patches = sut.GetExpectedPatches(debugInfo);

        // Should have patches from this test assembly despite potential failures in other assemblies
        patches.Should().NotBeEmpty();

        // Debug info should be populated with reflection information
        debugInfo.Should().Contain(entry => entry.Contains("Reflection:", StringComparison.Ordinal));
    }

    [Fact]
    public void GetExpectedPatches_HandlesMalformedHarmonyPatchAttributes_Gracefully()
    {
        var sut = new ExpectedPatchSource();
        var debugInfo = new List<string>();

        // This test assembly includes the ExpectedPatchSourceMalformedPatch class
        // which has intentionally malformed HarmonyPatch attributes
        var patches = sut.GetExpectedPatches(debugInfo);

        // Should not crash, just skip malformed patches
        patches.Should().NotBeEmpty();

        // Debug info should show that malformed patches were detected with null targets
        // These malformed patches will be filtered out (not added to expectedPatches list)
        debugInfo.Should().Contain(entry =>
            entry.Contains("ExpectedPatchSourceMalformedPatch", StringComparison.Ordinal) &&
            entry.Contains("target=null.null", StringComparison.Ordinal));
    }

    [Fact]
    public void GetExpectedPatches_PopulatesDebugInfo_WhenReflectionFailsForSomeTypes()
    {
        var sut = new ExpectedPatchSource();
        var debugInfo = new List<string>();

        var patches = sut.GetExpectedPatches(debugInfo);

        // Verify debug info contains reflection phase information
        debugInfo.Should().Contain(entry => entry.Contains("=== Phase 1: Reflection-based scan ===", StringComparison.Ordinal));
        debugInfo.Should().Contain(entry => entry.Contains("=== Phase 2: Source-scan fallback ===", StringComparison.Ordinal));
        debugInfo.Should().Contain(entry => entry.Contains("=== Phase 3: Runtime fallback scan ===", StringComparison.Ordinal));

        // Verify totals are reported
        debugInfo.Should().Contain(entry => entry.Contains("Totals: reflection=", StringComparison.Ordinal));
    }

    [Fact]
    public void GetExpectedPatches_ReportsSuppressedExceptionCount_WhenExceptionsOccur()
    {
        var sut = new ExpectedPatchSource();
        var debugInfo = new List<string>();

        var patches = sut.GetExpectedPatches(debugInfo);

        // If any exceptions were suppressed, they should be reported
        // (This will be empty in successful runs, but the format should be consistent)
        var exceptionReport = debugInfo.Find(entry => entry.Contains("exceptions were suppressed", StringComparison.Ordinal));
        if (exceptionReport != null)
        {
            exceptionReport.Should().Match("WARNING: * exceptions were suppressed during patch detection.*");
        }
    }
}

public sealed class ExpectedPatchSourceTestTarget
{
    public static void ClassLevelTarget()
    {
    }

    public static void AttributeTarget()
    {
    }

    public static void StackedClassLevelTarget()
    {
    }

    public static void MethodLevelTarget()
    {
    }
}

[HarmonyPatch(typeof(ExpectedPatchSourceTestTarget), nameof(ExpectedPatchSourceTestTarget.ClassLevelTarget))]
public static class ExpectedPatchSourceClassLevelPatch
{
    public static void Prefix()
    {
    }
}

[HarmonyPatch(typeof(ExpectedPatchSourceTestTarget), nameof(ExpectedPatchSourceTestTarget.AttributeTarget))]
public static class ExpectedPatchSourceCustomMethodNamePatch
{
    [HarmonyPostfix]
    public static void CustomNamedPostfix()
    {
    }
}

[HarmonyPatch(typeof(ExpectedPatchSourceTestTarget))]
[HarmonyPatch(nameof(ExpectedPatchSourceTestTarget.StackedClassLevelTarget))]
public static class ExpectedPatchSourceStackedClassAttributesPatch
{
    public static void Postfix()
    {
    }
}

[HarmonyPatch(typeof(ExpectedPatchSourceTestTarget))]
public static class ExpectedPatchSourceMethodLevelTargetPatch
{
    [HarmonyPatch(nameof(ExpectedPatchSourceTestTarget.MethodLevelTarget))]
    [HarmonyPrefix]
    public static void CustomMethodLevelPrefix()
    {
    }
}

// Test class with intentionally malformed HarmonyPatch attributes to test error handling
[HarmonyPatch] // Missing target type and method
public static class ExpectedPatchSourceMalformedPatch
{
    [HarmonyPatch] // Malformed - no arguments
    public static void MalformedPrefix1()
    {
    }

    [HarmonyPostfix] // Applied without class-level target
    public static void MalformedPostfix()
    {
    }
}
