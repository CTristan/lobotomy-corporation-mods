// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.Common.Models.Diagnostics;
using Xunit;

// ReSharper disable ObjectCreationAsStatement

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class ExternalLogDataTests
    {
        [Fact]
        public void Constructor_throws_when_retargetHarmonyLog_is_null()
        {
            Action act = () => _ = new ExternalLogData(null!, string.Empty, string.Empty);

            act.Should().Throw<ArgumentNullException>().WithParameterName("retargetHarmonyLog");
        }

        [Fact]
        public void Constructor_throws_when_bepInExLog_is_null()
        {
            Action act = () => _ = new ExternalLogData(string.Empty, null!, string.Empty);

            act.Should().Throw<ArgumentNullException>().WithParameterName("bepInExLog");
        }

        [Fact]
        public void Constructor_throws_when_unityLog_is_null()
        {
            Action act = () => _ = new ExternalLogData(string.Empty, string.Empty, null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("unityLog");
        }

        [Fact]
        public void Properties_return_constructor_values()
        {
            var data = new ExternalLogData("retarget", "bepinex", "unity");

            data.RetargetHarmonyLog.Should().Be("retarget");
            data.BepInExLog.Should().Be("bepinex");
            data.UnityLog.Should().Be("unity");
        }
    }
}
