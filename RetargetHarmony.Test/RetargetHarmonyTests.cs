// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Mono.Cecil;
using Xunit;
using Xunit.Abstractions;
using static RetargetHarmony.RetargetHarmony;

#pragma warning disable CA1515 // Types need to be public for testability
#pragma warning disable xUnit1000 // Test classes must be public - xUnit requirement
#pragma warning disable CA1812 // Internal class apparently never instantiated - xUnit instantiates test classes

namespace RetargetHarmony.Test;

public sealed class RetargetHarmonyTests
{
    private static readonly string RepoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
    private static readonly string ExternalDir = Path.Combine(RepoRoot, "external");
    private static readonly string ManagedDir = Path.Combine(ExternalDir, "LobotomyCorp_Data", "Managed");

    private readonly ITestOutputHelper _output;

    public RetargetHarmonyTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ExternalDirectoryExists()
    {
        _output.WriteLine($"BaseDirectory: {AppContext.BaseDirectory}");
        _output.WriteLine($"RepoRoot: {RepoRoot}");
        _output.WriteLine($"ExternalDir: {ExternalDir}");

        Directory.Exists(ExternalDir).Should().BeTrue("external directory should exist");
        File.Exists(GetManagedAssemblyPath("Assembly-CSharp.dll")).Should().BeTrue("Assembly-CSharp.dll should exist in external directory");
        File.Exists(GetManagedAssemblyPath("LobotomyBaseModLib.dll")).Should().BeTrue("LobotomyBaseModLib.dll should exist in external directory");
    }

    [Fact]
    public void Patch_ThrowsArgumentNullException_WhenAssemblyIsNull()
    {
        Action act = () => Patch(null);

        act.Should().Throw<ArgumentNullException>().WithParameterName("asm");
    }

    [Fact]
    public void Patch_RetargetsHarmonyReference_InAssemblyCSharp()
    {
        using var assemblyDefinition = AssemblyDefinition.ReadAssembly(GetManagedAssemblyPath("Assembly-CSharp.dll"));

        RetargetHarmony.Patch(assemblyDefinition);

        var harmonyRefs = GetHarmonyReferences(assemblyDefinition);

        harmonyRefs.Should().HaveCount(1, "there should be exactly one Harmony reference after patching");
        harmonyRefs[0].Name.Should().Be("0Harmony109", "the reference should be retargeted to 0Harmony109");
    }

    [Fact]
    public void Patch_RetargetsHarmonyReference_InLobotomyBaseModLib()
    {
        using var assemblyDefinition = AssemblyDefinition.ReadAssembly(GetManagedAssemblyPath("LobotomyBaseModLib.dll"));

        RetargetHarmony.Patch(assemblyDefinition);

        var harmonyRefs = GetHarmonyReferences(assemblyDefinition);

        harmonyRefs.Should().HaveCount(1, "there should be exactly one Harmony reference after patching");
        harmonyRefs[0].Name.Should().Be("0Harmony109", "the reference should be retargeted to 0Harmony109");
    }

    [Fact]
    public void Patch_DoesNotModifyOtherReferences_InAssemblyCSharp()
    {
        using var originalAssembly = AssemblyDefinition.ReadAssembly(GetManagedAssemblyPath("Assembly-CSharp.dll"));
        var originalRefs = originalAssembly.MainModule.AssemblyReferences.Select(r => r.Name).ToList();

        using var assemblyDefinition = AssemblyDefinition.ReadAssembly(GetManagedAssemblyPath("Assembly-CSharp.dll"));

        RetargetHarmony.Patch(assemblyDefinition);
        var patchedRefs = assemblyDefinition.MainModule.AssemblyReferences.Select(r => r.Name).ToList();

        var nonHarmonyOriginalRefs = originalRefs.Where(r => r != "0Harmony" && r != "0Harmony109").ToList();
        var nonHarmonyPatchedRefs = patchedRefs.Where(r => r != "0Harmony" && r != "0Harmony109").ToList();

        nonHarmonyPatchedRefs.Should().BeEquivalentTo(nonHarmonyOriginalRefs,
            "non-Harmony references should not be modified");
    }

    [Fact]
    public void Patch_DoesNotModifyOtherReferences_InLobotomyBaseModLib()
    {
        using var originalAssembly = AssemblyDefinition.ReadAssembly(GetManagedAssemblyPath("LobotomyBaseModLib.dll"));
        var originalRefs = originalAssembly.MainModule.AssemblyReferences.Select(r => r.Name).ToList();

        using var assemblyDefinition = AssemblyDefinition.ReadAssembly(GetManagedAssemblyPath("LobotomyBaseModLib.dll"));

        RetargetHarmony.Patch(assemblyDefinition);
        var patchedRefs = assemblyDefinition.MainModule.AssemblyReferences.Select(r => r.Name).ToList();

        var nonHarmonyOriginalRefs = originalRefs.Where(r => r != "0Harmony" && r != "0Harmony109").ToList();
        var nonHarmonyPatchedRefs = patchedRefs.Where(r => r != "0Harmony" && r != "0Harmony109").ToList();

        nonHarmonyPatchedRefs.Should().BeEquivalentTo(nonHarmonyOriginalRefs,
            "non-Harmony references should not be modified");
    }

    [Fact]
    public void Patch_Idempotent_WhenAlreadyRetargetedTo0Harmony109()
    {
        using var assemblyDefinition = CreateSyntheticAssembly("0Harmony109");

        RetargetHarmony.Patch(assemblyDefinition);
        RetargetHarmony.Patch(assemblyDefinition);

        var harmonyRefs = GetHarmonyReferences(assemblyDefinition);

        harmonyRefs.Should().HaveCount(1, "there should still be exactly one Harmony reference");
        harmonyRefs[0].Name.Should().Be("0Harmony109", "the reference should remain 0Harmony109");
    }

    [Fact]
    public void Patch_RemovesDuplicateHarmonyReferences()
    {
        using var assemblyDefinition = CreateSyntheticAssembly("0Harmony", "0Harmony", "0Harmony");

        RetargetHarmony.Patch(assemblyDefinition);

        var harmonyRefs = GetHarmonyReferences(assemblyDefinition);

        harmonyRefs.Should().HaveCount(1, "there should be exactly one Harmony reference after deduplication");
        harmonyRefs[0].Name.Should().Be("0Harmony109", "the remaining reference should be retargeted to 0Harmony109");
    }

    [Fact]
    public void Patch_ConsolidatesMixedHarmonyReferences()
    {
        using var assemblyDefinition = CreateSyntheticAssembly("0Harmony", "0Harmony109", "0Harmony");

        RetargetHarmony.Patch(assemblyDefinition);

        var harmonyRefs = GetHarmonyReferences(assemblyDefinition);

        harmonyRefs.Should().HaveCount(1, "there should be exactly one Harmony reference after consolidation");
        harmonyRefs[0].Name.Should().Be("0Harmony109", "the remaining reference should be 0Harmony109");
    }

    [Fact]
    public void Patch_DoesNothing_WhenNoHarmonyReference()
    {
        using var assemblyDefinition = CreateSyntheticAssembly("mscorlib");

        var exception = Record.Exception(() => RetargetHarmony.Patch(assemblyDefinition));

        exception.Should().BeNull("patching an assembly without Harmony reference should not throw");

        var harmonyRefs = GetHarmonyReferences(assemblyDefinition);
        harmonyRefs.Should().BeEmpty("no Harmony references should be added");
    }

    [Fact]
    public void TargetDLLs_ReturnsExpectedAssemblies()
    {
        var targetDlls = RetargetHarmony.TargetDLLs.ToList();

        targetDlls.Should().HaveCount(2, "there should be 2 target DLLs");
        targetDlls.Should().Contain("Assembly-CSharp.dll");
        targetDlls.Should().Contain("LobotomyBaseModLib.dll");
    }

    private static string GetManagedAssemblyPath(string fileName)
    {
        return Path.Combine(ManagedDir, fileName);
    }

    private static List<AssemblyNameReference> GetHarmonyReferences(AssemblyDefinition assemblyDefinition)
    {
        return assemblyDefinition.MainModule.AssemblyReferences
            .Where(r => r.Name == "0Harmony" || r.Name == "0Harmony109")
            .ToList();
    }

    private static AssemblyDefinition CreateSyntheticAssembly(params string[] referencedAssemblies)
    {
        var assembly = AssemblyDefinition.ReadAssembly(GetManagedAssemblyPath("Assembly-CSharp.dll"));
        assembly.MainModule.AssemblyReferences.Clear();

        foreach (var refName in referencedAssemblies)
        {
            var reference = new AssemblyNameReference(refName, new Version(1, 0, 0, 0));
            assembly.MainModule.AssemblyReferences.Add(reference);
        }

        return assembly;
    }
}
