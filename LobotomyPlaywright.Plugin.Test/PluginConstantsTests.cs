// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Playwright.Test
{
    public sealed class PluginConstantsTests
    {
        [Fact]
        public void PluginGuid_is_expected_value()
        {
            PluginConstants.PluginGuid.Should().Be("com.ctristan.lobotomyplaywright");
        }

        [Fact]
        public void PluginName_is_expected_value()
        {
            PluginConstants.PluginName.Should().Be("LobotomyPlaywright");
        }

        [Fact]
        public void PluginVersion_is_expected_value()
        {
            PluginConstants.PluginVersion.Should().Be("1.0.0");
        }

        [Fact]
        public void DefaultPort_is_expected_value()
        {
            PluginConstants.DefaultPort.Should().Be(8484);
        }
    }
}
