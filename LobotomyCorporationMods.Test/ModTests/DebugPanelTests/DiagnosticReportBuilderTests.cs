// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyCorporationMods.DebugPanel.Implementations;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.Common.Enums.Diagnostics;
using LobotomyCorporationMods.Common.Models.Diagnostics;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class DiagnosticReportBuilderTests
    {
        private readonly Mock<ICollectorFactory> _mockFactory;
        private readonly Mock<IEnvironmentDetector> _mockDetector;
        private readonly Mock<IActivePatchCollector> _mockPatchCollector;
        private readonly Mock<IInfoCollector<IList<DetectedModInfo>>> _mockBaseModCollector;
        private readonly Mock<IInfoCollector<IList<DetectedModInfo>>> _mockBepInExPluginCollector;
        private readonly Mock<IInfoCollector<IList<AssemblyInfo>>> _mockAssemblyCollector;
        private readonly Mock<IInfoCollector<RetargetHarmonyStatus>> _mockRetargetDetector;
        private readonly Mock<IExpectedPatchSource> _mockExpectedPatchSource;
        private readonly Mock<IInfoCollector<DllIntegrityReport>> _mockDllIntegrityCollector;

        public DiagnosticReportBuilderTests()
        {
            _mockFactory = new Mock<ICollectorFactory>();
            _mockDetector = new Mock<IEnvironmentDetector>();
            _mockPatchCollector = new Mock<IActivePatchCollector>();
            _mockBaseModCollector = new Mock<IInfoCollector<IList<DetectedModInfo>>>();
            _mockBepInExPluginCollector = new Mock<IInfoCollector<IList<DetectedModInfo>>>();
            _mockAssemblyCollector = new Mock<IInfoCollector<IList<AssemblyInfo>>>();
            _mockRetargetDetector = new Mock<IInfoCollector<RetargetHarmonyStatus>>();
            _mockExpectedPatchSource = new Mock<IExpectedPatchSource>();
            _mockDllIntegrityCollector = new Mock<IInfoCollector<DllIntegrityReport>>();

            _mockFactory.Setup(f => f.CreateActivePatchCollector(It.IsAny<IList<string>>())).Returns(_mockPatchCollector.Object);
            _mockFactory.Setup(f => f.CreateBaseModCollector(It.IsAny<IList<string>>())).Returns(_mockBaseModCollector.Object);
            _mockFactory.Setup(f => f.CreateBepInExPluginCollector()).Returns(_mockBepInExPluginCollector.Object);
            _mockFactory.Setup(f => f.CreateAssemblyInfoCollector()).Returns(_mockAssemblyCollector.Object);
            _mockFactory.Setup(f => f.CreateRetargetHarmonyDetector()).Returns(_mockRetargetDetector.Object);
            _mockFactory.Setup(f => f.CreateExpectedPatchSource(It.IsAny<IList<string>>())).Returns(_mockExpectedPatchSource.Object);
            _mockFactory.Setup(f => f.CreateDllIntegrityCollector()).Returns(_mockDllIntegrityCollector.Object);

            _mockPatchCollector.Setup(c => c.Collect()).Returns([]);
            _mockBaseModCollector.Setup(c => c.Collect()).Returns([]);
            _mockBepInExPluginCollector.Setup(c => c.Collect()).Returns([]);
            _mockAssemblyCollector.Setup(c => c.Collect()).Returns([]);
            _mockRetargetDetector.Setup(c => c.Collect()).Returns(new RetargetHarmonyStatus(false, false, false, "Not detected"));
            _mockDllIntegrityCollector.Setup(c => c.Collect()).Returns(new DllIntegrityReport([], false, string.Empty, false, string.Empty, -1, false, 0, [], "No findings"));
            _mockExpectedPatchSource.Setup(s => s.GetExpectedPatches(It.IsAny<IList<string>>())).Returns([]);
            _mockDetector.Setup(d => d.DetectEnvironment()).Returns(new EnvironmentInfo(false, false, false));
        }

        private DiagnosticReportBuilder CreateBuilder()
        {
            return new DiagnosticReportBuilder(_mockFactory.Object, _mockDetector.Object);
        }

        [Fact]
        public void Constructor_throws_when_collectorFactory_is_null()
        {
            Action act = () => _ = new DiagnosticReportBuilder(null, _mockDetector.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("collectorFactory");
        }

        [Fact]
        public void Constructor_throws_when_environmentDetector_is_null()
        {
            Action act = () => _ = new DiagnosticReportBuilder(_mockFactory.Object, null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("environmentDetector");
        }

        [Fact]
        public void BuildReport_returns_report_with_collected_data()
        {
            var mods = new List<DetectedModInfo>
            {
                new("TestMod", "1.0", ModSource.Lmm, HarmonyVersion.Harmony1, "TestMod", "", false, 0, 0),
            };
            var patches = new List<PatchInfo>
            {
                new("TestType", "TestMethod", PatchType.Postfix, "owner", "PatchMethod", "TestMod"),
            };
            var assemblies = new List<AssemblyInfo>
            {
                new("TestAssembly", "1.0.0", "/path/to/test.dll", false),
            };

            _mockBaseModCollector.Setup(c => c.Collect()).Returns(mods);
            _mockPatchCollector.Setup(c => c.Collect()).Returns(patches);
            _mockAssemblyCollector.Setup(c => c.Collect()).Returns(assemblies);

            var builder = CreateBuilder();
            var report = builder.BuildReport();

            report.Mods.Should().HaveCount(1);
            report.Patches.Should().HaveCount(1);
            report.Assemblies.Should().HaveCount(1);
        }

        [Fact]
        public void BuildReport_combines_mods_from_bepinex_and_basemod_collectors()
        {
            var bepInExMods = new List<DetectedModInfo>
            {
                new("BepPlugin", "1.0", ModSource.BepInExPlugin, HarmonyVersion.Harmony2, "BepPlugin", "com.test.plugin", false, 0, 0),
            };
            var baseMods = new List<DetectedModInfo>
            {
                new("LmmMod", "1.0", ModSource.Lmm, HarmonyVersion.Harmony1, "LmmMod", "", false, 0, 0),
            };

            _mockBepInExPluginCollector.Setup(c => c.Collect()).Returns(bepInExMods);
            _mockBaseModCollector.Setup(c => c.Collect()).Returns(baseMods);

            var builder = CreateBuilder();
            var report = builder.BuildReport();

            report.Mods.Should().HaveCount(2);
        }

        [Fact]
        public void BuildReport_captures_collector_failure_as_warning()
        {
            _mockBaseModCollector.Setup(c => c.Collect()).Throws(new InvalidOperationException("test error"));

            var builder = CreateBuilder();
            var report = builder.BuildReport();

            report.Warnings.Should().Contain(w => w.Contains("BaseModCollector failed") && w.Contains("test error"));
            report.Mods.Should().BeEmpty();
        }

        [Fact]
        public void BuildReport_captures_retarget_detector_failure_as_warning()
        {
            _mockRetargetDetector.Setup(c => c.Collect()).Throws(new InvalidOperationException("retarget error"));

            var builder = CreateBuilder();
            var report = builder.BuildReport();

            report.Warnings.Should().Contain(w => w.Contains("RetargetHarmonyDetector failed"));
            report.RetargetHarmonyStatus.IsDetected.Should().BeFalse();
            report.RetargetHarmonyStatus.Message.Should().Contain("Detection failed");
        }

        [Fact]
        public void BuildReport_captures_expected_patch_source_failure_as_warning()
        {
            _mockExpectedPatchSource.Setup(s => s.GetExpectedPatches(It.IsAny<IList<string>>()))
                .Throws(new InvalidOperationException("expected patch error"));

            var builder = CreateBuilder();
            var report = builder.BuildReport();

            report.Warnings.Should().Contain(w => w.Contains("ExpectedPatchSource failed"));
            report.PatchComparison.HasMissingPatches.Should().BeFalse();
        }

        [Fact]
        public void BuildReport_captures_environment_detector_failure_as_warning()
        {
            _mockDetector.Setup(d => d.DetectEnvironment()).Throws(new InvalidOperationException("env error"));

            var builder = CreateBuilder();
            var report = builder.BuildReport();

            report.Warnings.Should().Contain(w => w.Contains("EnvironmentDetector failed"));
            report.EnvironmentInfo.IsHarmony2Available.Should().BeFalse();
        }

        [Fact]
        public void BuildReport_correlates_patches_with_mods()
        {
            var mods = new List<DetectedModInfo>
            {
                new("TestMod", "1.0", ModSource.Lmm, HarmonyVersion.Harmony1, "TestMod", "", false, 0, 0),
            };
            var patches = new List<PatchInfo>
            {
                new("TypeA", "MethodA", PatchType.Postfix, "owner", "PatchA", "TestMod"),
                new("TypeB", "MethodB", PatchType.Prefix, "owner", "PatchB", "TestMod"),
            };

            _mockBaseModCollector.Setup(c => c.Collect()).Returns(mods);
            _mockPatchCollector.Setup(c => c.Collect()).Returns(patches);

            var builder = CreateBuilder();
            var report = builder.BuildReport();

            report.Mods[0].HasActivePatches.Should().BeTrue();
            report.Mods[0].ActivePatchCount.Should().Be(2);
        }

        [Fact]
        public void BuildReport_identifies_missing_patches()
        {
            var mods = new List<DetectedModInfo>
            {
                new("TestMod", "1.0", ModSource.Lmm, HarmonyVersion.Harmony1, "TestMod", "", false, 0, 0),
            };
            var patches = new List<PatchInfo>();
            var expectedPatches = new List<ExpectedPatchInfo>
            {
                new("TestMod", "GameClass", "GameMethod", "Postfix", PatchType.Postfix),
            };

            _mockBaseModCollector.Setup(c => c.Collect()).Returns(mods);
            _mockPatchCollector.Setup(c => c.Collect()).Returns(patches);
            _mockExpectedPatchSource.Setup(s => s.GetExpectedPatches(It.IsAny<IList<string>>())).Returns(expectedPatches);

            var builder = CreateBuilder();
            var report = builder.BuildReport();

            report.PatchComparison.HasMissingPatches.Should().BeTrue();
            report.PatchComparison.MissingPatches.Should().HaveCount(1);
            report.PatchComparison.TotalExpected.Should().Be(1);
            report.PatchComparison.TotalMatched.Should().Be(0);
        }

        [Fact]
        public void BuildReport_matches_patches_when_all_expected_patches_are_active()
        {
            var mods = new List<DetectedModInfo>
            {
                new("TestMod", "1.0", ModSource.Lmm, HarmonyVersion.Harmony1, "TestMod", "", false, 0, 0),
            };
            var patches = new List<PatchInfo>
            {
                new("Namespace.GameClass", "GameMethod", PatchType.Postfix, "owner", "Postfix", "TestMod"),
            };
            var expectedPatches = new List<ExpectedPatchInfo>
            {
                new("TestMod", "Namespace.GameClass", "GameMethod", "Postfix", PatchType.Postfix),
            };

            _mockBaseModCollector.Setup(c => c.Collect()).Returns(mods);
            _mockPatchCollector.Setup(c => c.Collect()).Returns(patches);
            _mockExpectedPatchSource.Setup(s => s.GetExpectedPatches(It.IsAny<IList<string>>())).Returns(expectedPatches);

            var builder = CreateBuilder();
            var report = builder.BuildReport();

            report.PatchComparison.HasMissingPatches.Should().BeFalse();
            report.PatchComparison.TotalExpected.Should().Be(1);
            report.PatchComparison.TotalMatched.Should().Be(1);
        }

        [Fact]
        public void BuildReport_handles_namespace_variance_in_type_matching()
        {
            var mods = new List<DetectedModInfo>
            {
                new("TestMod", "1.0", ModSource.Lmm, HarmonyVersion.Harmony1, "TestMod", "", false, 0, 0),
            };
            var patches = new List<PatchInfo>
            {
                new("Some.Namespace.GameClass", "GameMethod", PatchType.Postfix, "owner", "Postfix", "TestMod"),
            };
            var expectedPatches = new List<ExpectedPatchInfo>
            {
                new("TestMod", "GameClass", "GameMethod", "Postfix", PatchType.Postfix),
            };

            _mockBaseModCollector.Setup(c => c.Collect()).Returns(mods);
            _mockPatchCollector.Setup(c => c.Collect()).Returns(patches);
            _mockExpectedPatchSource.Setup(s => s.GetExpectedPatches(It.IsAny<IList<string>>())).Returns(expectedPatches);

            var builder = CreateBuilder();
            var report = builder.BuildReport();

            report.PatchComparison.HasMissingPatches.Should().BeFalse();
            report.PatchComparison.TotalMatched.Should().Be(1);
        }

        [Fact]
        public void BuildReport_updates_mod_expected_patch_count()
        {
            var mods = new List<DetectedModInfo>
            {
                new("TestMod", "1.0", ModSource.Lmm, HarmonyVersion.Harmony1, "TestMod", "", false, 0, 0),
            };
            var expectedPatches = new List<ExpectedPatchInfo>
            {
                new("TestMod", "GameClass", "Method1", "Postfix", PatchType.Postfix),
                new("TestMod", "GameClass", "Method2", "Prefix", PatchType.Prefix),
            };

            _mockBaseModCollector.Setup(c => c.Collect()).Returns(mods);
            _mockExpectedPatchSource.Setup(s => s.GetExpectedPatches(It.IsAny<IList<string>>())).Returns(expectedPatches);

            var builder = CreateBuilder();
            var report = builder.BuildReport();

            report.Mods[0].ExpectedPatchCount.Should().Be(2);
        }

        [Fact]
        public void BuildReport_includes_environment_info()
        {
            _mockDetector.Setup(d => d.DetectEnvironment()).Returns(new EnvironmentInfo(true, true, false));

            var builder = CreateBuilder();
            var report = builder.BuildReport();

            report.EnvironmentInfo.IsHarmony2Available.Should().BeTrue();
            report.EnvironmentInfo.IsBepInExAvailable.Should().BeTrue();
            report.EnvironmentInfo.IsMonoCecilAvailable.Should().BeFalse();
        }

        [Fact]
        public void BuildReport_includes_dll_integrity_data()
        {
            var findings = new List<DllIntegrityFinding>
            {
                new("/path/mod.dll", "mod.dll", FindingSeverity.Info, [], [], false, "", false, "Not modified"),
            };
            var dllIntegrity = new DllIntegrityReport(findings, true, "/backup", false, "", -1, false, 0, [], "1 DLLs checked, 0 rewritten");
            _mockDllIntegrityCollector.Setup(c => c.Collect()).Returns(dllIntegrity);

            var builder = CreateBuilder();
            var report = builder.BuildReport();

            report.DllIntegrity.Should().BeSameAs(dllIntegrity);
            report.DllIntegrity.Findings.Should().HaveCount(1);
        }

        [Fact]
        public void BuildReport_captures_dll_integrity_collector_failure_as_warning()
        {
            _mockDllIntegrityCollector.Setup(c => c.Collect()).Throws(new InvalidOperationException("integrity error"));

            var builder = CreateBuilder();
            var report = builder.BuildReport();

            report.Warnings.Should().Contain(w => w.Contains("DllIntegrityCollector failed") && w.Contains("integrity error"));
            report.DllIntegrity.Findings.Should().BeEmpty();
            report.DllIntegrity.Summary.Should().Contain("Collection failed");
        }

        [Fact]
        public void BuildReport_synthesizes_mods_from_expected_patches_when_base_mod_collector_returns_empty()
        {
            var expectedPatches = new List<ExpectedPatchInfo>
            {
                new("TestMod", "GameClass", "Method1", "Postfix", PatchType.Postfix),
                new("TestMod", "GameClass", "Method2", "Prefix", PatchType.Prefix),
                new("OtherMod", "OtherClass", "OtherMethod", "Postfix", PatchType.Postfix),
            };
            var assemblies = new List<AssemblyInfo>
            {
                new("TestMod", "1.2.0", "/path/TestMod.dll", false),
                new("OtherMod", "3.0.0", "/path/OtherMod.dll", false),
            };

            _mockExpectedPatchSource.Setup(s => s.GetExpectedPatches(It.IsAny<IList<string>>())).Returns(expectedPatches);
            _mockAssemblyCollector.Setup(c => c.Collect()).Returns(assemblies);

            var builder = CreateBuilder();
            var report = builder.BuildReport();

            report.Mods.Should().HaveCount(2);
            report.Mods.Should().Contain(m => m.Name == "TestMod" && m.Version == "1.2.0" && m.Source == ModSource.Lmm);
            report.Mods.Should().Contain(m => m.Name == "OtherMod" && m.Version == "3.0.0" && m.Source == ModSource.Lmm);
            report.PatchComparison.HasMissingPatches.Should().BeFalse();
        }

        [Fact]
        public void BuildReport_does_not_synthesize_when_base_mod_collector_returns_lmm_mods()
        {
            var baseMods = new List<DetectedModInfo>
            {
                new("TestMod", "1.0", ModSource.Lmm, HarmonyVersion.Harmony1, "TestMod", "", true, 1, 0),
            };
            var expectedPatches = new List<ExpectedPatchInfo>
            {
                new("TestMod", "GameClass", "Method1", "Postfix", PatchType.Postfix),
                new("OtherMod", "OtherClass", "OtherMethod", "Postfix", PatchType.Postfix),
            };

            _mockBaseModCollector.Setup(c => c.Collect()).Returns(baseMods);
            _mockExpectedPatchSource.Setup(s => s.GetExpectedPatches(It.IsAny<IList<string>>())).Returns(expectedPatches);

            var builder = CreateBuilder();
            var report = builder.BuildReport();

            report.Mods.Should().HaveCount(1);
            report.Mods[0].Name.Should().Be("TestMod");
        }

        [Fact]
        public void HasLmmMods_returns_false_for_empty_list()
        {
            DiagnosticReportBuilder.HasLmmMods([]).Should().BeFalse();
        }

        [Fact]
        public void HasLmmMods_returns_false_for_null()
        {
            DiagnosticReportBuilder.HasLmmMods(null).Should().BeFalse();
        }

        [Fact]
        public void HasLmmMods_returns_true_when_lmm_mod_exists()
        {
            var mods = new List<DetectedModInfo>
            {
                new("Mod", "1.0", ModSource.Lmm, HarmonyVersion.Harmony1, "Mod", "", false, 0, 0),
            };

            DiagnosticReportBuilder.HasLmmMods(mods).Should().BeTrue();
        }

        [Fact]
        public void HasLmmMods_returns_false_when_only_bepinex_mods_exist()
        {
            var mods = new List<DetectedModInfo>
            {
                new("Plugin", "1.0", ModSource.BepInExPlugin, HarmonyVersion.Harmony2, "Plugin", "id", false, 0, 0),
            };

            DiagnosticReportBuilder.HasLmmMods(mods).Should().BeFalse();
        }

        [Fact]
        public void SynthesizeModsFromExpectedPatches_throws_when_expectedPatches_is_null()
        {
            Action act = () => DiagnosticReportBuilder.SynthesizeModsFromExpectedPatches(null, []);

            act.Should().Throw<ArgumentNullException>().WithParameterName("expectedPatches");
        }

        [Fact]
        public void SynthesizeModsFromExpectedPatches_groups_patches_by_assembly()
        {
            var expectedPatches = new List<ExpectedPatchInfo>
            {
                new("ModA", "ClassA", "Method1", "Postfix", PatchType.Postfix),
                new("ModA", "ClassB", "Method2", "Prefix", PatchType.Prefix),
                new("ModB", "ClassC", "Method3", "Postfix", PatchType.Postfix),
            };

            var result = DiagnosticReportBuilder.SynthesizeModsFromExpectedPatches(expectedPatches, []);

            result.Should().HaveCount(2);
            result.Should().Contain(m => m.Name == "ModA" && m.ExpectedPatchCount == 2 && m.ActivePatchCount == 2 && m.HasActivePatches);
            result.Should().Contain(m => m.Name == "ModB" && m.ExpectedPatchCount == 1 && m.ActivePatchCount == 1 && m.HasActivePatches);
        }

        [Fact]
        public void SynthesizeModsFromExpectedPatches_uses_version_from_assemblies()
        {
            var expectedPatches = new List<ExpectedPatchInfo>
            {
                new("ModA", "ClassA", "Method1", "Postfix", PatchType.Postfix),
            };
            var assemblies = new List<AssemblyInfo>
            {
                new("ModA", "2.5.0", "/path/ModA.dll", false),
            };

            var result = DiagnosticReportBuilder.SynthesizeModsFromExpectedPatches(expectedPatches, assemblies);

            result.Should().HaveCount(1);
            result[0].Version.Should().Be("2.5.0");
        }

        [Fact]
        public void SynthesizeModsFromExpectedPatches_excludes_framework_assemblies()
        {
            var expectedPatches = new List<ExpectedPatchInfo>
            {
                new("ModA", "ClassA", "Method1", "Postfix", PatchType.Postfix),
                new("BepInEx.Preloader", "Console", "SetOut", "Prefix", PatchType.Prefix),
                new("Mono.Cecil", "SomeType", "SomeMethod", "Postfix", PatchType.Postfix),
            };

            var result = DiagnosticReportBuilder.SynthesizeModsFromExpectedPatches(expectedPatches, []);

            result.Should().HaveCount(1);
            result[0].Name.Should().Be("ModA");
        }

        [Fact]
        public void SynthesizeModsFromExpectedPatches_handles_null_assemblies()
        {
            var expectedPatches = new List<ExpectedPatchInfo>
            {
                new("ModA", "ClassA", "Method1", "Postfix", PatchType.Postfix),
            };

            var result = DiagnosticReportBuilder.SynthesizeModsFromExpectedPatches(expectedPatches, null);

            result.Should().HaveCount(1);
            result[0].Version.Should().BeEmpty();
        }
        [Fact]
        public void SynthesizePatchesFromExpected_throws_when_null()
        {
            Action act = () => DiagnosticReportBuilder.SynthesizePatchesFromExpected(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("expectedPatches");
        }

        [Fact]
        public void SynthesizePatchesFromExpected_converts_expected_patches_to_patch_info()
        {
            var expectedPatches = new List<ExpectedPatchInfo>
            {
                new("ModA", "ClassA", "Method1", "Postfix", PatchType.Postfix),
                new("ModB", "ClassB", "Method2", "Prefix", PatchType.Prefix),
            };

            var result = DiagnosticReportBuilder.SynthesizePatchesFromExpected(expectedPatches);

            result.Should().HaveCount(2);
            result[0].TargetType.Should().Be("ClassA");
            result[0].TargetMethod.Should().Be("Method1");
            result[0].PatchType.Should().Be(PatchType.Postfix);
            result[0].PatchAssemblyName.Should().Be("ModA");
            result[1].TargetType.Should().Be("ClassB");
            result[1].PatchAssemblyName.Should().Be("ModB");
        }

        [Fact]
        public void BuildReport_synthesizes_patches_from_expected_when_active_patches_empty()
        {
            var expectedPatches = new List<ExpectedPatchInfo>
            {
                new("TestMod", "GameClass", "Method1", "Postfix", PatchType.Postfix),
                new("OtherMod", "OtherClass", "OtherMethod", "Postfix", PatchType.Postfix),
            };
            var assemblies = new List<AssemblyInfo>
            {
                new("TestMod", "1.0.0", "/path/TestMod.dll", false),
                new("OtherMod", "2.0.0", "/path/OtherMod.dll", false),
            };

            _mockExpectedPatchSource.Setup(s => s.GetExpectedPatches(It.IsAny<IList<string>>())).Returns(expectedPatches);
            _mockAssemblyCollector.Setup(c => c.Collect()).Returns(assemblies);

            var builder = CreateBuilder();
            var report = builder.BuildReport();

            report.Patches.Should().HaveCount(2);
            report.Patches.Should().Contain(p => p.TargetType == "GameClass" && p.PatchAssemblyName == "TestMod");
            report.Patches.Should().Contain(p => p.TargetType == "OtherClass" && p.PatchAssemblyName == "OtherMod");
        }
        [Fact]
        public void SynthesizeModsFromExpectedPatches_detects_harmony2_from_assemblies()
        {
            var expectedPatches = new List<ExpectedPatchInfo>
            {
                new("ModA", "ClassA", "Method1", "Postfix", PatchType.Postfix),
            };
            var assemblies = new List<AssemblyInfo>
            {
                new("ModA", "1.0.0", "/path/ModA.dll", false),
                new("0Harmony", "2.9.0", "/path/0Harmony.dll", true),
                new("0Harmony109", "1.0.9", "/path/0Harmony109.dll", true),
            };

            var result = DiagnosticReportBuilder.SynthesizeModsFromExpectedPatches(expectedPatches, assemblies);

            result.Should().HaveCount(1);
            result[0].HarmonyVersion.Should().Be(HarmonyVersion.Harmony2);
        }

        [Fact]
        public void SynthesizeModsFromExpectedPatches_detects_harmony1_when_no_harmony2()
        {
            var expectedPatches = new List<ExpectedPatchInfo>
            {
                new("ModA", "ClassA", "Method1", "Postfix", PatchType.Postfix),
            };
            var assemblies = new List<AssemblyInfo>
            {
                new("ModA", "1.0.0", "/path/ModA.dll", false),
                new("0Harmony109", "1.0.9", "/path/0Harmony109.dll", true),
            };

            var result = DiagnosticReportBuilder.SynthesizeModsFromExpectedPatches(expectedPatches, assemblies);

            result.Should().HaveCount(1);
            result[0].HarmonyVersion.Should().Be(HarmonyVersion.Harmony1);
        }
    }
}
