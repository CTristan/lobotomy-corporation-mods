// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyCorporationMods.Common.Enums.Diagnostics;
using LobotomyCorporationMods.Common.Models.Diagnostics;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class PatchComparisonResultTests
    {
        [Fact]
        public void Constructor_stores_all_properties()
        {
            var missingPatches = new List<MissingPatchInfo>
            {
                new("Asm", "Type", "Method", "Patch", PatchType.Prefix),
            };

            var result = new PatchComparisonResult(missingPatches, 5, 4);

            result.MissingPatches.Should().BeSameAs(missingPatches);
            result.TotalExpected.Should().Be(5);
            result.TotalMatched.Should().Be(4);
        }

        [Fact]
        public void HasMissingPatches_returns_true_when_missing_patches_exist()
        {
            var missingPatches = new List<MissingPatchInfo>
            {
                new("Asm", "Type", "Method", "Patch", PatchType.Prefix),
            };

            var result = new PatchComparisonResult(missingPatches, 1, 0);

            result.HasMissingPatches.Should().BeTrue();
        }

        [Fact]
        public void HasMissingPatches_returns_false_when_no_missing_patches()
        {
            var result = new PatchComparisonResult([], 3, 3);

            result.HasMissingPatches.Should().BeFalse();
        }

        [Fact]
        public void Constructor_throws_when_missingPatches_is_null()
        {
            Action act = () => _ = new PatchComparisonResult(null, 0, 0);

            act.Should().Throw<ArgumentNullException>().WithParameterName("missingPatches");
        }
    }
}
