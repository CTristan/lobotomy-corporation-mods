// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Text;
using AwesomeAssertions;
using LobotomyCorporationMods.DebugPanel.Implementations;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.DebugPanel.Models;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class DllIntegrityCollectorTests
    {
        private readonly Mock<IDllFileInspector> _mockInspector;
        private readonly Mock<IShimArtifactSource> _mockShimSource;
        private readonly Mock<ILoadedAssemblyReferenceSource> _mockAssemblySource;

        public DllIntegrityCollectorTests()
        {
            _mockInspector = new Mock<IDllFileInspector>();
            _mockShimSource = new Mock<IShimArtifactSource>();
            _mockAssemblySource = new Mock<ILoadedAssemblyReferenceSource>();

            _mockShimSource.Setup(s => s.BackupDirectoryExists).Returns(false);
            _mockShimSource.Setup(s => s.BackupDirectoryPath).Returns(string.Empty);
            _mockShimSource.Setup(s => s.InteropCacheExists).Returns(false);
            _mockShimSource.Setup(s => s.InteropCachePath).Returns(string.Empty);
            _mockShimSource.Setup(s => s.GetBackupFileNames()).Returns([]);
            _mockAssemblySource.Setup(s => s.GetBaseModAssemblies()).Returns([]);
            _mockInspector.Setup(i => i.IsDeepInspectionAvailable).Returns(false);
        }

        private DllIntegrityCollector CreateCollector()
        {
            return new DllIntegrityCollector(_mockInspector.Object, _mockShimSource.Object, _mockAssemblySource.Object);
        }

        [Fact]
        public void Constructor_throws_when_dllFileInspector_is_null()
        {
            Action act = () => _ = new DllIntegrityCollector(null, _mockShimSource.Object, _mockAssemblySource.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("dllFileInspector");
        }

        [Fact]
        public void Constructor_throws_when_shimArtifactSource_is_null()
        {
            Action act = () => _ = new DllIntegrityCollector(_mockInspector.Object, null, _mockAssemblySource.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("shimArtifactSource");
        }

        [Fact]
        public void Constructor_throws_when_loadedAssemblySource_is_null()
        {
            Action act = () => _ = new DllIntegrityCollector(_mockInspector.Object, _mockShimSource.Object, null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("loadedAssemblySource");
        }

        [Fact]
        public void Collect_returns_empty_report_when_no_baseMod_assemblies_loaded()
        {
            var collector = CreateCollector();

            var report = collector.Collect();

            report.Findings.Should().BeEmpty();
            report.TotalRewrittenCount.Should().Be(0);
            report.Summary.Should().Contain("0 DLLs checked");
        }

        [Fact]
        public void Collect_returns_info_finding_for_unmodified_dll()
        {
            var assemblies = new List<LoadedAssemblyInfo>
            {
                new("TestMod", "/mods/TestMod.dll", ["0Harmony", "mscorlib"]),
            };
            _mockAssemblySource.Setup(s => s.GetBaseModAssemblies()).Returns(assemblies);
            _mockInspector.Setup(i => i.GetAssemblyReferences("/mods/TestMod.dll")).Returns(["0Harmony", "mscorlib"]);

            var collector = CreateCollector();
            var report = collector.Collect();

            report.Findings.Should().HaveCount(1);
            report.Findings[0].Severity.Should().Be(FindingSeverity.Info);
            report.Findings[0].Summary.Should().Be("Not modified");
            report.Findings[0].WasRewritten.Should().BeFalse();
            report.TotalRewrittenCount.Should().Be(0);
        }

        [Fact]
        public void Collect_returns_warning_finding_for_shimmed_dll_with_backup()
        {
            var assemblies = new List<LoadedAssemblyInfo>
            {
                new("TestMod", "/mods/TestMod.dll", ["0Harmony109"]),
            };
            _mockAssemblySource.Setup(s => s.GetBaseModAssemblies()).Returns(assemblies);
            _mockInspector.Setup(i => i.GetAssemblyReferences("/mods/TestMod.dll")).Returns(["0Harmony109"]);

            _mockShimSource.Setup(s => s.BackupDirectoryExists).Returns(true);
            _mockShimSource.Setup(s => s.BackupDirectoryPath).Returns("/backup");
            _mockShimSource.Setup(s => s.GetBackupFileNames()).Returns(["TestMod.dll"]);
            _mockShimSource.Setup(s => s.ReadBackupFileBytes("TestMod.dll")).Returns(Encoding.UTF8.GetBytes("prefix0Harmonysuffix"));

            var collector = CreateCollector();
            var report = collector.Collect();

            report.Findings.Should().HaveCount(1);
            report.Findings[0].Severity.Should().Be(FindingSeverity.Warning);
            report.Findings[0].Summary.Should().Be("Rewritten by BepInEx shim (backup available)");
            report.Findings[0].WasRewritten.Should().BeTrue();
            report.Findings[0].HasBackup.Should().BeTrue();
            report.Findings[0].OriginalHarmonyReferences.Should().Contain("0Harmony");
            report.TotalRewrittenCount.Should().Be(1);
        }

        [Fact]
        public void Collect_returns_error_finding_for_shimmed_dll_without_backup()
        {
            var assemblies = new List<LoadedAssemblyInfo>
            {
                new("TestMod", "/mods/TestMod.dll", ["0Harmony109"]),
            };
            _mockAssemblySource.Setup(s => s.GetBaseModAssemblies()).Returns(assemblies);
            _mockInspector.Setup(i => i.GetAssemblyReferences("/mods/TestMod.dll")).Returns(["0Harmony109"]);

            var collector = CreateCollector();
            var report = collector.Collect();

            report.Findings.Should().HaveCount(1);
            report.Findings[0].Severity.Should().Be(FindingSeverity.Error);
            report.Findings[0].Summary.Should().Be("Rewritten by BepInEx shim (no backup!)");
            report.Findings[0].WasRewritten.Should().BeTrue();
            report.Findings[0].HasBackup.Should().BeFalse();
        }

        [Fact]
        public void Collect_returns_error_finding_for_unexpected_harmony_reference()
        {
            var assemblies = new List<LoadedAssemblyInfo>
            {
                new("TestMod", "/mods/TestMod.dll", ["0Harmony12"]),
            };
            _mockAssemblySource.Setup(s => s.GetBaseModAssemblies()).Returns(assemblies);
            _mockInspector.Setup(i => i.GetAssemblyReferences("/mods/TestMod.dll")).Returns(["0Harmony12"]);

            var collector = CreateCollector();
            var report = collector.Collect();

            report.Findings.Should().HaveCount(1);
            report.Findings[0].Severity.Should().Be(FindingSeverity.Error);
            report.Findings[0].Summary.Should().Contain("Unexpected Harmony reference: 0Harmony12");
            report.Findings[0].WasRewritten.Should().BeFalse();
        }

        [Fact]
        public void Collect_returns_warning_finding_when_dll_is_unreadable()
        {
            var assemblies = new List<LoadedAssemblyInfo>
            {
                new("TestMod", "/mods/TestMod.dll", ["0Harmony"]),
            };
            _mockAssemblySource.Setup(s => s.GetBaseModAssemblies()).Returns(assemblies);
            _mockInspector.Setup(i => i.GetAssemblyReferences("/mods/TestMod.dll")).Throws(new InvalidOperationException("file locked"));

            var collector = CreateCollector();
            var report = collector.Collect();

            report.Findings.Should().HaveCount(1);
            report.Findings[0].Severity.Should().Be(FindingSeverity.Warning);
            report.Findings[0].Summary.Should().Contain("Unable to read DLL");
            report.Findings[0].Summary.Should().Contain("file locked");
            report.Findings[0].WasRewritten.Should().BeFalse();
        }

        [Fact]
        public void Collect_populates_shim_artifact_metadata()
        {
            _mockShimSource.Setup(s => s.BackupDirectoryExists).Returns(true);
            _mockShimSource.Setup(s => s.BackupDirectoryPath).Returns("/game/BepInEx_Shim_Backup");
            _mockShimSource.Setup(s => s.InteropCacheExists).Returns(true);
            _mockShimSource.Setup(s => s.InteropCachePath).Returns("/game/BepInEx/cache/harmony_interop_cache.dat");
            _mockShimSource.Setup(s => s.GetInteropCacheEntryCount()).Returns(3);
            _mockShimSource.Setup(s => s.GetBackupFileNames()).Returns(["mod1.dll", "mod2.dll"]);

            var collector = CreateCollector();
            var report = collector.Collect();

            report.ShimBackupDirectoryExists.Should().BeTrue();
            report.ShimBackupDirectoryPath.Should().Be("/game/BepInEx_Shim_Backup");
            report.InteropCacheExists.Should().BeTrue();
            report.InteropCachePath.Should().Be("/game/BepInEx/cache/harmony_interop_cache.dat");
            report.InteropCacheEntryCount.Should().Be(3);
        }

        [Fact]
        public void Collect_counts_total_rewritten()
        {
            var assemblies = new List<LoadedAssemblyInfo>
            {
                new("Mod1", "/mods/Mod1.dll", ["0Harmony109"]),
                new("Mod2", "/mods/Mod2.dll", ["0Harmony"]),
                new("Mod3", "/mods/Mod3.dll", ["0Harmony109"]),
            };
            _mockAssemblySource.Setup(s => s.GetBaseModAssemblies()).Returns(assemblies);
            _mockInspector.Setup(i => i.GetAssemblyReferences("/mods/Mod1.dll")).Returns(["0Harmony109"]);
            _mockInspector.Setup(i => i.GetAssemblyReferences("/mods/Mod2.dll")).Returns(["0Harmony"]);
            _mockInspector.Setup(i => i.GetAssemblyReferences("/mods/Mod3.dll")).Returns(["0Harmony109"]);

            var collector = CreateCollector();
            var report = collector.Collect();

            report.TotalRewrittenCount.Should().Be(2);
            report.Summary.Should().Contain("3 DLLs checked").And.Contain("2 rewritten");
        }

        [Fact]
        public void Collect_builds_summary_message()
        {
            var assemblies = new List<LoadedAssemblyInfo>
            {
                new("Mod1", "/mods/Mod1.dll", ["0Harmony"]),
            };
            _mockAssemblySource.Setup(s => s.GetBaseModAssemblies()).Returns(assemblies);
            _mockInspector.Setup(i => i.GetAssemblyReferences("/mods/Mod1.dll")).Returns(["0Harmony"]);

            var collector = CreateCollector();
            var report = collector.Collect();

            report.Summary.Should().Be("1 DLLs checked, 0 rewritten");
        }

        [Fact]
        public void Collect_reads_backup_bytes_to_determine_original_references()
        {
            var assemblies = new List<LoadedAssemblyInfo>
            {
                new("TestMod", "/mods/TestMod.dll", ["0Harmony109"]),
            };
            _mockAssemblySource.Setup(s => s.GetBaseModAssemblies()).Returns(assemblies);
            _mockInspector.Setup(i => i.GetAssemblyReferences("/mods/TestMod.dll")).Returns(["0Harmony109"]);

            _mockShimSource.Setup(s => s.BackupDirectoryExists).Returns(true);
            _mockShimSource.Setup(s => s.BackupDirectoryPath).Returns("/backup");
            _mockShimSource.Setup(s => s.GetBackupFileNames()).Returns(["TestMod.dll"]);
            _mockShimSource.Setup(s => s.ReadBackupFileBytes("TestMod.dll")).Returns(Encoding.UTF8.GetBytes("something0Harmonysomething"));

            var collector = CreateCollector();
            var report = collector.Collect();

            _mockShimSource.Verify(s => s.ReadBackupFileBytes("TestMod.dll"), Times.Once);
            report.Findings[0].OriginalHarmonyReferences.Should().Contain("0Harmony");
        }

        [Fact]
        public void Collect_handles_backup_read_failure_gracefully()
        {
            var assemblies = new List<LoadedAssemblyInfo>
            {
                new("TestMod", "/mods/TestMod.dll", ["0Harmony109"]),
            };
            _mockAssemblySource.Setup(s => s.GetBaseModAssemblies()).Returns(assemblies);
            _mockInspector.Setup(i => i.GetAssemblyReferences("/mods/TestMod.dll")).Returns(["0Harmony109"]);

            _mockShimSource.Setup(s => s.BackupDirectoryExists).Returns(true);
            _mockShimSource.Setup(s => s.BackupDirectoryPath).Returns("/backup");
            _mockShimSource.Setup(s => s.GetBackupFileNames()).Returns(["TestMod.dll"]);
            _mockShimSource.Setup(s => s.ReadBackupFileBytes("TestMod.dll")).Throws(new InvalidOperationException("corrupt backup"));

            var collector = CreateCollector();
            var report = collector.Collect();

            report.Findings.Should().HaveCount(1);
            report.Findings[0].Severity.Should().Be(FindingSeverity.Warning);
            report.Findings[0].HasBackup.Should().BeTrue();
            report.Findings[0].OriginalHarmonyReferences.Should().BeEmpty();
            report.Warnings.Should().Contain(w => w.Contains("Unable to read backup") && w.Contains("corrupt backup"));
        }

        [Fact]
        public void Collect_returns_info_for_dll_with_no_harmony_references()
        {
            var assemblies = new List<LoadedAssemblyInfo>
            {
                new("TestMod", "/mods/TestMod.dll", ["mscorlib", "UnityEngine"]),
            };
            _mockAssemblySource.Setup(s => s.GetBaseModAssemblies()).Returns(assemblies);
            _mockInspector.Setup(i => i.GetAssemblyReferences("/mods/TestMod.dll")).Returns(["mscorlib", "UnityEngine"]);

            var collector = CreateCollector();
            var report = collector.Collect();

            report.Findings.Should().HaveCount(1);
            report.Findings[0].Severity.Should().Be(FindingSeverity.Info);
            report.Findings[0].Summary.Should().Be("Not modified");
            report.Findings[0].WasRewritten.Should().BeFalse();
        }

        [Fact]
        public void Collect_skips_null_assembly_entries()
        {
            var assemblies = new List<LoadedAssemblyInfo>
            {
                null!,
                new("TestMod", "/mods/TestMod.dll", ["0Harmony"]),
            };
            _mockAssemblySource.Setup(s => s.GetBaseModAssemblies()).Returns(assemblies);
            _mockInspector.Setup(i => i.GetAssemblyReferences("/mods/TestMod.dll")).Returns(["0Harmony"]);

            var collector = CreateCollector();
            var report = collector.Collect();

            report.Findings.Should().HaveCount(1);
            report.Findings[0].DllName.Should().Be("TestMod.dll");
        }

        [Fact]
        public void Collect_reflects_deep_inspection_availability_in_report()
        {
            _mockInspector.Setup(i => i.IsDeepInspectionAvailable).Returns(true);

            var collector = CreateCollector();
            var report = collector.Collect();

            report.MonoCecilAvailable.Should().BeTrue();
        }

        [Fact]
        public void Collect_shimmed_reference_takes_priority_over_unexpected()
        {
            var assemblies = new List<LoadedAssemblyInfo>
            {
                new("TestMod", "/mods/TestMod.dll", ["0Harmony109", "0Harmony12"]),
            };
            _mockAssemblySource.Setup(s => s.GetBaseModAssemblies()).Returns(assemblies);
            _mockInspector.Setup(i => i.GetAssemblyReferences("/mods/TestMod.dll")).Returns(["0Harmony109", "0Harmony12"]);

            var collector = CreateCollector();
            var report = collector.Collect();

            report.Findings.Should().HaveCount(1);
            report.Findings[0].WasRewritten.Should().BeTrue();
            report.Findings[0].Summary.Should().Contain("Rewritten by BepInEx shim");
        }

        [Fact]
        public void Collect_returns_negative_one_cache_count_when_cache_not_exists()
        {
            _mockShimSource.Setup(s => s.InteropCacheExists).Returns(false);

            var collector = CreateCollector();
            var report = collector.Collect();

            report.InteropCacheEntryCount.Should().Be(-1);
            _mockShimSource.Verify(s => s.GetInteropCacheEntryCount(), Times.Never);
        }

        [Fact]
        public void Collect_handles_mixed_findings_across_multiple_dlls()
        {
            var assemblies = new List<LoadedAssemblyInfo>
            {
                new("CleanMod", "/mods/CleanMod.dll", ["0Harmony"]),
                new("ShimmedMod", "/mods/ShimmedMod.dll", ["0Harmony109"]),
                new("BrokenMod", "/mods/BrokenMod.dll", ["0Harmony"]),
            };
            _mockAssemblySource.Setup(s => s.GetBaseModAssemblies()).Returns(assemblies);
            _mockInspector.Setup(i => i.GetAssemblyReferences("/mods/CleanMod.dll")).Returns(["0Harmony"]);
            _mockInspector.Setup(i => i.GetAssemblyReferences("/mods/ShimmedMod.dll")).Returns(["0Harmony109"]);
            _mockInspector.Setup(i => i.GetAssemblyReferences("/mods/BrokenMod.dll")).Throws(new InvalidOperationException("locked"));

            var collector = CreateCollector();
            var report = collector.Collect();

            report.Findings.Should().HaveCount(3);
            report.Findings[0].Severity.Should().Be(FindingSeverity.Info);
            report.Findings[1].Severity.Should().Be(FindingSeverity.Error);
            report.Findings[2].Severity.Should().Be(FindingSeverity.Warning);
            report.TotalRewrittenCount.Should().Be(1);
            report.Summary.Should().Be("3 DLLs checked, 1 rewritten");
        }
    }
}
