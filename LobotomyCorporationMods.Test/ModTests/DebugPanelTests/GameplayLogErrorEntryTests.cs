// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using DebugPanel.Common.Models.Diagnostics;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class GameplayLogErrorEntryTests
    {
        [Fact]
        public void Constructor_stores_all_properties()
        {
            var entry = new GameplayLogErrorEntry("ModName", "Mod.dll", "Error message", "  at Method()", "Herror - ModName / Mod.dllError message");

            entry.ModName.Should().Be("ModName");
            entry.DllName.Should().Be("Mod.dll");
            entry.ErrorMessage.Should().Be("Error message");
            entry.StackTrace.Should().Be("  at Method()");
            entry.RawLine.Should().Be("Herror - ModName / Mod.dllError message");
        }

        [Fact]
        public void Constructor_throws_when_modName_is_null()
        {
            Action act = () => _ = new GameplayLogErrorEntry(null, "Mod.dll", "Error", "", "raw");

            act.Should().Throw<ArgumentNullException>().WithParameterName("modName");
        }

        [Fact]
        public void Constructor_throws_when_dllName_is_null()
        {
            Action act = () => _ = new GameplayLogErrorEntry("Mod", null, "Error", "", "raw");

            act.Should().Throw<ArgumentNullException>().WithParameterName("dllName");
        }

        [Fact]
        public void Constructor_throws_when_errorMessage_is_null()
        {
            Action act = () => _ = new GameplayLogErrorEntry("Mod", "Mod.dll", null, "", "raw");

            act.Should().Throw<ArgumentNullException>().WithParameterName("errorMessage");
        }

        [Fact]
        public void Constructor_throws_when_stackTrace_is_null()
        {
            Action act = () => _ = new GameplayLogErrorEntry("Mod", "Mod.dll", "Error", null, "raw");

            act.Should().Throw<ArgumentNullException>().WithParameterName("stackTrace");
        }

        [Fact]
        public void Constructor_throws_when_rawLine_is_null()
        {
            Action act = () => _ = new GameplayLogErrorEntry("Mod", "Mod.dll", "Error", "", null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("rawLine");
        }
    }
}
