// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyCorporationMods.DebugPanel.Implementations;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.DebugPanel.Models;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class FallbackPatchInspectionSourceTests
    {
        private readonly Mock<IPatchInspectionSource> _mockPrimary;
        private readonly Mock<IPatchInspectionSource> _mockFallback;
        private readonly List<string> _diagnosticLog;

        public FallbackPatchInspectionSourceTests()
        {
            _mockPrimary = new Mock<IPatchInspectionSource>();
            _mockFallback = new Mock<IPatchInspectionSource>();
            _diagnosticLog = [];

            _mockPrimary.Setup(s => s.GetPatches()).Returns([]);
            _mockFallback.Setup(s => s.GetPatches()).Returns([]);
        }

        private FallbackPatchInspectionSource CreateSource()
        {
            return new FallbackPatchInspectionSource(
                _mockPrimary.Object,
                _mockFallback.Object,
                _diagnosticLog,
                "Primary",
                "Fallback");
        }

        [Fact]
        public void Constructor_throws_when_primarySource_is_null()
        {
            Action act = () => _ = new FallbackPatchInspectionSource(null, _mockFallback.Object, _diagnosticLog, "Primary", "Fallback");

            act.Should().Throw<ArgumentNullException>().WithParameterName("primarySource");
        }

        [Fact]
        public void Constructor_throws_when_fallbackSource_is_null()
        {
            Action act = () => _ = new FallbackPatchInspectionSource(_mockPrimary.Object, null, _diagnosticLog, "Primary", "Fallback");

            act.Should().Throw<ArgumentNullException>().WithParameterName("fallbackSource");
        }

        [Fact]
        public void Constructor_throws_when_diagnosticLog_is_null()
        {
            Action act = () => _ = new FallbackPatchInspectionSource(_mockPrimary.Object, _mockFallback.Object, null, "Primary", "Fallback");

            act.Should().Throw<ArgumentNullException>().WithParameterName("diagnosticLog");
        }

        [Fact]
        public void Constructor_throws_when_primaryLabel_is_null()
        {
            Action act = () => _ = new FallbackPatchInspectionSource(_mockPrimary.Object, _mockFallback.Object, _diagnosticLog, null, "Fallback");

            act.Should().Throw<ArgumentNullException>().WithParameterName("primaryLabel");
        }

        [Fact]
        public void Constructor_throws_when_fallbackLabel_is_null()
        {
            Action act = () => _ = new FallbackPatchInspectionSource(_mockPrimary.Object, _mockFallback.Object, _diagnosticLog, "Primary", null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("fallbackLabel");
        }

        [Fact]
        public void GetPatches_returns_primary_results_when_primary_has_patches()
        {
            var patch = new PatchInspectionInfo("GameClass", "Method", PatchType.Postfix, "owner", "PatchMethod", "MyMod", "1.0", []);
            _mockPrimary.Setup(s => s.GetPatches()).Returns([patch]);

            var source = CreateSource();
            var results = new List<PatchInspectionInfo>(source.GetPatches());

            results.Should().HaveCount(1);
            results[0].TargetType.Should().Be("GameClass");
        }

        [Fact]
        public void GetPatches_does_not_invoke_fallback_when_primary_has_patches()
        {
            var patch = new PatchInspectionInfo("GameClass", "Method", PatchType.Postfix, "owner", "PatchMethod", "MyMod", "1.0", []);
            _mockPrimary.Setup(s => s.GetPatches()).Returns([patch]);

            var source = CreateSource();
            _ = new List<PatchInspectionInfo>(source.GetPatches());

            _mockFallback.Verify(s => s.GetPatches(), Times.Never);
        }

        [Fact]
        public void GetPatches_uses_fallback_when_primary_returns_zero_patches()
        {
            var patch = new PatchInspectionInfo("FallbackClass", "Method", PatchType.Prefix, "owner", "PatchMethod", "FallbackMod", "2.0", []);
            _mockFallback.Setup(s => s.GetPatches()).Returns([patch]);

            var source = CreateSource();
            var results = new List<PatchInspectionInfo>(source.GetPatches());

            results.Should().HaveCount(1);
            results[0].TargetType.Should().Be("FallbackClass");
        }

        [Fact]
        public void GetPatches_uses_fallback_when_primary_throws_exception()
        {
            _mockPrimary.Setup(s => s.GetPatches()).Throws(new InvalidOperationException("Reflection failed"));
            var patch = new PatchInspectionInfo("FallbackClass", "Method", PatchType.Prefix, "owner", "PatchMethod", "FallbackMod", "2.0", []);
            _mockFallback.Setup(s => s.GetPatches()).Returns([patch]);

            var source = CreateSource();
            var results = new List<PatchInspectionInfo>(source.GetPatches());

            results.Should().HaveCount(1);
            results[0].TargetType.Should().Be("FallbackClass");
        }

        [Fact]
        public void GetPatches_logs_primary_exception_to_diagnostic_log()
        {
            _mockPrimary.Setup(s => s.GetPatches()).Throws(new InvalidOperationException("Reflection failed"));

            var source = CreateSource();
            _ = new List<PatchInspectionInfo>(source.GetPatches());

            _diagnosticLog.Should().Contain(s => s.Contains("Primary") && s.Contains("Reflection failed"));
        }

        [Fact]
        public void GetPatches_returns_empty_list_when_both_sources_return_zero()
        {
            var source = CreateSource();
            var results = new List<PatchInspectionInfo>(source.GetPatches());

            results.Should().BeEmpty();
        }

        [Fact]
        public void GetPatches_returns_empty_list_when_both_sources_throw()
        {
            _mockPrimary.Setup(s => s.GetPatches()).Throws(new InvalidOperationException("Primary failed"));
            _mockFallback.Setup(s => s.GetPatches()).Throws(new InvalidOperationException("Fallback failed"));

            var source = CreateSource();
            var results = new List<PatchInspectionInfo>(source.GetPatches());

            results.Should().BeEmpty();
        }

        [Fact]
        public void GetPatches_logs_both_exceptions_when_both_sources_throw()
        {
            _mockPrimary.Setup(s => s.GetPatches()).Throws(new InvalidOperationException("Primary failed"));
            _mockFallback.Setup(s => s.GetPatches()).Throws(new InvalidOperationException("Fallback failed"));

            var source = CreateSource();
            _ = new List<PatchInspectionInfo>(source.GetPatches());

            _diagnosticLog.Should().Contain(s => s.Contains("Primary") && s.Contains("Primary failed"));
            _diagnosticLog.Should().Contain(s => s.Contains("Fallback") && s.Contains("Fallback failed"));
        }

        [Fact]
        public void GetPatches_logs_patch_counts_for_primary()
        {
            var patch = new PatchInspectionInfo("GameClass", "Method", PatchType.Postfix, "owner", "PatchMethod", "MyMod", "1.0", []);
            _mockPrimary.Setup(s => s.GetPatches()).Returns([patch]);

            var source = CreateSource();
            _ = new List<PatchInspectionInfo>(source.GetPatches());

            _diagnosticLog.Should().Contain(s => s.Contains("Primary") && s.Contains("1 patches"));
        }

        [Fact]
        public void GetPatches_logs_fallback_message_when_primary_returns_zero()
        {
            var source = CreateSource();
            _ = new List<PatchInspectionInfo>(source.GetPatches());

            _diagnosticLog.Should().Contain(s => s.Contains("falling back to Fallback"));
        }

        [Fact]
        public void GetPatches_skips_null_patches_from_primary()
        {
            var patch = new PatchInspectionInfo("GameClass", "Method", PatchType.Postfix, "owner", "PatchMethod", "MyMod", "1.0", []);
            _mockPrimary.Setup(s => s.GetPatches()).Returns(new PatchInspectionInfo[] { null!, patch });

            var source = CreateSource();
            var results = new List<PatchInspectionInfo>(source.GetPatches());

            results.Should().HaveCount(1);
        }
    }
}
