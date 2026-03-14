// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.DebugPanel.Models;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class MissingPatchInfoTests
    {
        [Fact]
        public void Constructor_stores_all_properties()
        {
            var result = new MissingPatchInfo("TestAssembly", "TestType", "TestMethod", "PatchMethod", PatchType.Postfix);

            result.PatchAssembly.Should().Be("TestAssembly");
            result.TargetType.Should().Be("TestType");
            result.TargetMethod.Should().Be("TestMethod");
            result.PatchMethod.Should().Be("PatchMethod");
            result.PatchType.Should().Be(PatchType.Postfix);
        }

        [Fact]
        public void Constructor_throws_when_patchAssembly_is_null()
        {
            Action act = () => _ = new MissingPatchInfo(null, "TestType", "TestMethod", "PatchMethod", PatchType.Postfix);

            act.Should().Throw<ArgumentNullException>().WithParameterName("patchAssembly");
        }

        [Fact]
        public void Constructor_throws_when_targetType_is_null()
        {
            Action act = () => _ = new MissingPatchInfo("TestAssembly", null, "TestMethod", "PatchMethod", PatchType.Postfix);

            act.Should().Throw<ArgumentNullException>().WithParameterName("targetType");
        }

        [Fact]
        public void Constructor_throws_when_targetMethod_is_null()
        {
            Action act = () => _ = new MissingPatchInfo("TestAssembly", "TestType", null, "PatchMethod", PatchType.Postfix);

            act.Should().Throw<ArgumentNullException>().WithParameterName("targetMethod");
        }

        [Fact]
        public void Constructor_throws_when_patchMethod_is_null()
        {
            Action act = () => _ = new MissingPatchInfo("TestAssembly", "TestType", "TestMethod", null, PatchType.Postfix);

            act.Should().Throw<ArgumentNullException>().WithParameterName("patchMethod");
        }
    }
}
