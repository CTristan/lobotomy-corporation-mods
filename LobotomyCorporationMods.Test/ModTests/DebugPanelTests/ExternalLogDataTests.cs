// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using DebugPanel.Common.Models.Diagnostics;
using Xunit;

// ReSharper disable ObjectCreationAsStatement

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class ExternalLogDataTests
    {
        private static ExternalLogData CreateData(
            string retargetHarmonyLog = "",
            string bepInExLog = "",
            string unityLog = "",
            string gameplayLog = "",
            string saveFolderLog = "",
            string lmmDirectoryLog = "",
            string lmmSystemLog = "",
            string baseModsLog = "")
        {
            return new ExternalLogData(retargetHarmonyLog, bepInExLog, unityLog, gameplayLog, saveFolderLog, lmmDirectoryLog, lmmSystemLog, baseModsLog);
        }

        [Fact]
        public void Constructor_throws_when_retargetHarmonyLog_is_null()
        {
            Action act = () => CreateData(retargetHarmonyLog: null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("retargetHarmonyLog");
        }

        [Fact]
        public void Constructor_throws_when_bepInExLog_is_null()
        {
            Action act = () => CreateData(bepInExLog: null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("bepInExLog");
        }

        [Fact]
        public void Constructor_throws_when_unityLog_is_null()
        {
            Action act = () => CreateData(unityLog: null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("unityLog");
        }

        [Fact]
        public void Constructor_throws_when_gameplayLog_is_null()
        {
            Action act = () => CreateData(gameplayLog: null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("gameplayLog");
        }

        [Fact]
        public void Constructor_throws_when_saveFolderLog_is_null()
        {
            Action act = () => CreateData(saveFolderLog: null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("saveFolderLog");
        }

        [Fact]
        public void Constructor_throws_when_lmmDirectoryLog_is_null()
        {
            Action act = () => CreateData(lmmDirectoryLog: null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("lmmDirectoryLog");
        }

        [Fact]
        public void Constructor_throws_when_lmmSystemLog_is_null()
        {
            Action act = () => CreateData(lmmSystemLog: null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("lmmSystemLog");
        }

        [Fact]
        public void Constructor_throws_when_baseModsLog_is_null()
        {
            Action act = () => CreateData(baseModsLog: null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("baseModsLog");
        }

        [Fact]
        public void Properties_return_constructor_values()
        {
            var data = new ExternalLogData("retarget", "bepinex", "unity", "gameplay", "savefolder", "lmmdir", "lmmsys", "basemods");

            data.RetargetHarmonyLog.Should().Be("retarget");
            data.BepInExLog.Should().Be("bepinex");
            data.UnityLog.Should().Be("unity");
            data.GameplayLog.Should().Be("gameplay");
            data.SaveFolderLog.Should().Be("savefolder");
            data.LmmDirectoryLog.Should().Be("lmmdir");
            data.LmmSystemLog.Should().Be("lmmsys");
            data.BaseModsLog.Should().Be("basemods");
        }

        [Fact]
        public void BepInExLogTimestamp_is_null_when_log_has_no_timestamp()
        {
            var data = CreateData(bepInExLog: "some log without timestamp");

            data.BepInExLogTimestamp.Should().BeNull();
        }

        [Fact]
        public void BepInExLogTimestamp_is_parsed_when_log_has_timestamp()
        {
            var data = CreateData(bepInExLog: "BepInEx 5.4.23.2 - LobotomyCorp (3/15/2026 2:30:00 PM)\n[Message] stuff");

            data.BepInExLogTimestamp.Should().NotBeNull();
            data.BepInExLogTimestamp!.Value.Year.Should().Be(2026);
            data.BepInExLogTimestamp!.Value.Month.Should().Be(3);
            data.BepInExLogTimestamp!.Value.Day.Should().Be(15);
        }
    }
}
