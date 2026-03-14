// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using LobotomyCorporationMods.DebugPanel.Implementations;
using LobotomyCorporationMods.DebugPanel.Models;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class ReportFormatterTests
    {
        private static DiagnosticReport CreateReport(
            IList<DetectedModInfo>? mods = null,
            IList<PatchInfo>? patches = null,
            IList<AssemblyInfo>? assemblies = null,
            PatchComparisonResult? patchComparison = null,
            RetargetHarmonyStatus? retargetHarmonyStatus = null,
            EnvironmentInfo? environmentInfo = null,
            IList<string>? warnings = null,
            IList<string>? debugInfo = null,
            DateTime? collectedAt = null)
        {
            return new DiagnosticReport(
                mods ?? [],
                patches ?? [],
                assemblies ?? [],
                patchComparison ?? new PatchComparisonResult([], 0, 0),
                retargetHarmonyStatus ?? new RetargetHarmonyStatus(false, false, false, "Not detected"),
                environmentInfo ?? new EnvironmentInfo(false, false, false),
                warnings ?? [],
                debugInfo ?? [],
                collectedAt ?? DateTime.UtcNow);
        }

        [Fact]
        public void FormatForOverlay_throws_when_report_is_null()
        {
            var formatter = new ReportFormatter();

            Action act = () => formatter.FormatForOverlay(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("report");
        }

        [Fact]
        public void FormatForLogFile_throws_when_report_is_null()
        {
            var formatter = new ReportFormatter();

            Action act = () => formatter.FormatForLogFile(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("report");
        }

        [Fact]
        public void FormatForOverlay_includes_collected_timestamp()
        {
            var report = CreateReport(collectedAt: new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc));

            var formatter = new ReportFormatter();
            var lines = formatter.FormatForOverlay(report);

            lines.Should().Contain(l => l.StartsWith("Collected: ", StringComparison.Ordinal));
        }

        [Fact]
        public void FormatForOverlay_includes_standalone_mode_label()
        {
            var report = CreateReport(environmentInfo: new EnvironmentInfo(false, false, false));

            var formatter = new ReportFormatter();
            var lines = formatter.FormatForOverlay(report);

            lines.Should().Contain("Mode: Standalone (Harmony 1.09)");
        }

        [Fact]
        public void FormatForOverlay_includes_enhanced_mode_label_when_bepinex_and_harmony2()
        {
            var report = CreateReport(environmentInfo: new EnvironmentInfo(true, true, false));

            var formatter = new ReportFormatter();
            var lines = formatter.FormatForOverlay(report);

            lines.Should().Contain("Mode: Enhanced (BepInEx + Harmony 2)");
        }

        [Fact]
        public void FormatForOverlay_includes_harmony2_only_mode_label()
        {
            var report = CreateReport(environmentInfo: new EnvironmentInfo(true, false, false));

            var formatter = new ReportFormatter();
            var lines = formatter.FormatForOverlay(report);

            lines.Should().Contain("Mode: Enhanced (Harmony 2)");
        }

        [Fact]
        public void FormatForOverlay_includes_mod_sections()
        {
            var mods = new List<DetectedModInfo>
            {
                new("TestMod", "1.0", ModSource.Lmm, HarmonyVersion.Harmony1, "TestMod", "", true, 2, 2),
            };
            var report = CreateReport(mods: mods);

            var formatter = new ReportFormatter();
            var lines = formatter.FormatForOverlay(report);

            lines.Should().Contain(l => l.Contains("LMM/Basemod Mods (1):"));
            lines.Should().Contain(l => l.Contains("TestMod") && l.Contains("Harmony 1"));
        }

        [Fact]
        public void FormatForOverlay_shows_none_when_no_bepinex_plugins()
        {
            var report = CreateReport();

            var formatter = new ReportFormatter();
            var lines = formatter.FormatForOverlay(report);

            var bepInExIndex = lines.ToList().FindIndex(l => l.Contains("BepInEx Plugins", StringComparison.Ordinal));
            lines[bepInExIndex + 1].Should().Be("  - None");
        }

        [Fact]
        public void FormatForOverlay_includes_missing_patches_section()
        {
            var missingPatches = new List<MissingPatchInfo>
            {
                new("TestMod", "GameClass", "Method", "Postfix", PatchType.Postfix),
            };
            var patchComparison = new PatchComparisonResult(missingPatches, 1, 0);
            var report = CreateReport(patchComparison: patchComparison);

            var formatter = new ReportFormatter();
            var lines = formatter.FormatForOverlay(report);

            lines.Should().Contain(l => l.Contains("Missing Harmony Patches (1):"));
            lines.Should().Contain(l => l.Contains("Postfix for Method in GameClass"));
        }

        [Fact]
        public void FormatForOverlay_includes_warnings()
        {
            var warnings = new List<string> { "Something went wrong" };
            var report = CreateReport(warnings: warnings);

            var formatter = new ReportFormatter();
            var lines = formatter.FormatForOverlay(report);

            lines.Should().Contain("Warning: Something went wrong");
        }

        [Fact]
        public void FormatForOverlay_shows_patch_status_with_expected_counts()
        {
            var mods = new List<DetectedModInfo>
            {
                new("TestMod", "1.0", ModSource.Lmm, HarmonyVersion.Harmony1, "TestMod", "", true, 3, 5),
            };
            var report = CreateReport(mods: mods);

            var formatter = new ReportFormatter();
            var lines = formatter.FormatForOverlay(report);

            lines.Should().Contain(l => l.Contains("3 loaded/5 expected, 2 failed"));
        }

        [Fact]
        public void FormatForOverlay_shows_identifier_for_bepinex_plugins()
        {
            var mods = new List<DetectedModInfo>
            {
                new("TestPlugin", "1.0", ModSource.BepInExPlugin, HarmonyVersion.Harmony2, "TestPlugin", "com.test.plugin", false, 0, 0),
            };
            var report = CreateReport(mods: mods);

            var formatter = new ReportFormatter();
            var lines = formatter.FormatForOverlay(report);

            lines.Should().Contain(l => l.Contains("[com.test.plugin]"));
        }

        [Fact]
        public void FormatForLogFile_includes_header_banner()
        {
            var report = CreateReport();

            var formatter = new ReportFormatter();
            var lines = formatter.FormatForLogFile(report);

            lines[0].Should().Contain("###");
            lines.Should().Contain(l => l.Contains("Harmony Diagnostic Report"));
        }

        [Fact]
        public void FormatForLogFile_includes_environment_section()
        {
            var report = CreateReport(environmentInfo: new EnvironmentInfo(true, false, true));

            var formatter = new ReportFormatter();
            var lines = formatter.FormatForLogFile(report);

            lines.Should().Contain(l => l.Contains("ENVIRONMENT"));
            lines.Should().Contain("  Harmony 2: Available");
            lines.Should().Contain("  BepInEx: Not detected");
            lines.Should().Contain("  Mono.Cecil: Available");
        }

        [Fact]
        public void FormatForLogFile_includes_assemblies_section()
        {
            var assemblies = new List<AssemblyInfo>
            {
                new("TestDll", "1.0.0", "/path/test.dll", false),
                new("0Harmony", "2.0.0", "/path/harmony.dll", true),
            };
            var report = CreateReport(assemblies: assemblies);

            var formatter = new ReportFormatter();
            var lines = formatter.FormatForLogFile(report);

            lines.Should().Contain(l => l.Contains("LOADED ASSEMBLIES (2)"));
            lines.Should().Contain(l => l.Contains("TestDll") && l.Contains("1.0.0"));
            lines.Should().Contain(l => l.Contains("0Harmony") && l.Contains("[Harmony]"));
        }

        [Fact]
        public void FormatForLogFile_includes_debug_info_section()
        {
            var debugInfo = new List<string> { "Phase 1: scanned 5 assemblies", "Phase 2: no source files" };
            var report = CreateReport(debugInfo: debugInfo);

            var formatter = new ReportFormatter();
            var lines = formatter.FormatForLogFile(report);

            lines.Should().Contain(l => l.Contains("DEBUG INFO (2)"));
            lines.Should().Contain("  - Phase 1: scanned 5 assemblies");
            lines.Should().Contain("  - Phase 2: no source files");
        }

        [Fact]
        public void FormatForLogFile_excludes_debug_info_section_when_empty()
        {
            var report = CreateReport();

            var formatter = new ReportFormatter();
            var lines = formatter.FormatForLogFile(report);

            lines.Should().NotContain(l => l.Contains("DEBUG INFO"));
        }

        [Fact]
        public void FormatForLogFile_ends_with_separator()
        {
            var report = CreateReport();

            var formatter = new ReportFormatter();
            var lines = formatter.FormatForLogFile(report);

            lines[^1].Should().Be("===============================");
        }

        [Fact]
        public void ToHarmonyVersionLabel_returns_correct_labels()
        {
            ReportFormatter.ToHarmonyVersionLabel(HarmonyVersion.Harmony1).Should().Be("Harmony 1");
            ReportFormatter.ToHarmonyVersionLabel(HarmonyVersion.Harmony2).Should().Be("Harmony 2");
            ReportFormatter.ToHarmonyVersionLabel(HarmonyVersion.Unknown).Should().Be("Unknown");
        }

        [Fact]
        public void FormatForOverlay_shows_missing_patch_types_correctly()
        {
            var missingPatches = new List<MissingPatchInfo>
            {
                new("Mod", "Type", "Method1", "Prefix", PatchType.Prefix),
                new("Mod", "Type", "Method2", "Transpiler", PatchType.Transpiler),
                new("Mod", "Type", "Method3", "Finalizer", PatchType.Finalizer),
            };
            var patchComparison = new PatchComparisonResult(missingPatches, 3, 0);
            var report = CreateReport(patchComparison: patchComparison);

            var formatter = new ReportFormatter();
            var lines = formatter.FormatForOverlay(report);

            lines.Should().Contain(l => l.Contains("Prefix for Method1"));
            lines.Should().Contain(l => l.Contains("Transpiler for Method2"));
            lines.Should().Contain(l => l.Contains("Finalizer for Method3"));
        }

        [Fact]
        public void FormatForLogFile_shows_none_for_empty_assemblies()
        {
            var report = CreateReport();

            var formatter = new ReportFormatter();
            var lines = formatter.FormatForLogFile(report);

            var asmIndex = lines.ToList().FindIndex(l => l.Contains("LOADED ASSEMBLIES", StringComparison.Ordinal));
            lines[asmIndex + 1].Should().Be("  - None");
        }

        [Fact]
        public void FormatForOverlay_shows_loaded_count_when_no_expected_patches()
        {
            var mods = new List<DetectedModInfo>
            {
                new("TestMod", "1.0", ModSource.Lmm, HarmonyVersion.Harmony1, "TestMod", "", true, 3, -1),
            };
            var report = CreateReport(mods: mods);

            var formatter = new ReportFormatter();
            var lines = formatter.FormatForOverlay(report);

            lines.Should().Contain(l => l.Contains("[3 loaded]"));
        }

        [Fact]
        public void FormatForOverlay_shows_zero_loaded_when_no_active_patches()
        {
            var mods = new List<DetectedModInfo>
            {
                new("TestMod", "1.0", ModSource.Lmm, HarmonyVersion.Harmony1, "TestMod", "", false, 0, -1),
            };
            var report = CreateReport(mods: mods);

            var formatter = new ReportFormatter();
            var lines = formatter.FormatForOverlay(report);

            lines.Should().Contain(l => l.Contains("[0 loaded]"));
        }
    }
}
