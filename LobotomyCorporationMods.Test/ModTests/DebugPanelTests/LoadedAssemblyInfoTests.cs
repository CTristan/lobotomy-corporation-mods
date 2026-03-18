// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using Hemocode.DebugPanel.Interfaces;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class LoadedAssemblyInfoTests
    {
        [Fact]
        public void Constructor_throws_when_name_is_null()
        {
            Action act = () => _ = new LoadedAssemblyInfo(null, "/path/test.dll", []);

            act.Should().Throw<ArgumentNullException>().WithParameterName("name");
        }

        [Fact]
        public void Constructor_throws_when_location_is_null()
        {
            Action act = () => _ = new LoadedAssemblyInfo("TestMod", null, []);

            act.Should().Throw<ArgumentNullException>().WithParameterName("location");
        }

        [Fact]
        public void Constructor_throws_when_referenceNames_is_null()
        {
            Action act = () => _ = new LoadedAssemblyInfo("TestMod", "/path/test.dll", null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("referenceNames");
        }

        [Fact]
        public void Properties_return_constructor_values()
        {
            var references = new List<string> { "0Harmony", "UnityEngine" };

            var info = new LoadedAssemblyInfo("TestMod", "/path/BaseMods/TestMod.dll", references);

            info.Name.Should().Be("TestMod");
            info.Location.Should().Be("/path/BaseMods/TestMod.dll");
            info.ReferenceNames.Should().HaveCount(2);
            info.ReferenceNames.Should().Contain("0Harmony");
            info.ReferenceNames.Should().Contain("UnityEngine");
        }
    }
}
