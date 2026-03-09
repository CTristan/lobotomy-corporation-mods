// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using HarmonyDebugPanel.Formatting;
using HarmonyDebugPanel.Models;
using Xunit;

namespace HarmonyDebugPanel.Test
{
    public sealed class DiagnosticLogFormatterTests
    {
        [Fact]
        public void Format_IncludesExpectedSectionsAndWarnings()
        {
            DiagnosticReport report = new()
            {
                CollectedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                RetargetHarmonyStatus = new RetargetHarmonyStatus(true, true, false, "Detected"),
            };

            report.Mods.Add(new ModInfo("Plugin", "1.0.0", ModSource.BepInExPlugin, HarmonyVersion.Harmony2, "PluginAssembly", "plugin.id"));
            report.Mods.Add(new ModInfo("BaseMod", "1.0.0", ModSource.Lmm, HarmonyVersion.Harmony1, "BaseModAssembly", string.Empty));
            report.Patches.Add(new PatchInfo("Type", "Method", PatchType.Postfix, "owner", "patch", "PatchAssembly"));
            report.Warnings.Add("collector warning");

            IList<string> lines = DiagnosticLogFormatter.Format(report);

            _ = lines.Should().Contain(l => l.Contains("Harmony Diagnostic Report", StringComparison.Ordinal));
            _ = lines.Should().Contain(l => l.Contains("BepInEx Plugins (1)", StringComparison.Ordinal));
            _ = lines.Should().Contain(l => l.Contains("LMM/Basemod Mods (1)", StringComparison.Ordinal));
            _ = lines.Should().Contain(l => l.Contains("RetargetHarmony: Detected", StringComparison.Ordinal));
            _ = lines.Should().Contain(l => l.Contains("Active Harmony Patches: 1", StringComparison.Ordinal));
            _ = lines.Should().Contain(l => l.Contains("Warning: collector warning", StringComparison.Ordinal));
        }

        [Fact]
        public void ToHarmonyVersionLabel_ReturnsExpectedLabel()
        {
            _ = DiagnosticLogFormatter.ToHarmonyVersionLabel(HarmonyVersion.Harmony1).Should().Be("Harmony 1");
            _ = DiagnosticLogFormatter.ToHarmonyVersionLabel(HarmonyVersion.Harmony2).Should().Be("Harmony 2");
            _ = DiagnosticLogFormatter.ToHarmonyVersionLabel(HarmonyVersion.Unknown).Should().Be("Unknown");
        }
    }
}
