// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.ConfigTests
{
    /// <summary>
    ///     Exercises the live config instance the Harmony_Patch singleton instantiates,
    ///     including the URL-parsing branches that aren't reached by other tests.
    /// </summary>
    public sealed class HarmonyPatchConfigSmokeTests : DontChatMeModTests
    {
        [Fact]
        public void Config_is_constructed_and_exposes_default_values()
        {
            var config = Harmony_Patch.Instance.Config;

            config.Should().NotBeNull();
            config.Enabled.Should().BeTrue();
            config.DangerEffectsEnabled.Should().BeFalse();
            config.GameId.Should().Be(0);
            config.AuthToken.Should().NotBeNull();
        }

        [Fact]
        public void Default_ServerUrl_is_null_because_the_default_string_value_is_empty()
        {
            // Exercises the empty-string branch in DontChatMeConfig.ServerUrl
            Harmony_Patch.Instance.Config.ServerUrl.Should().BeNull();
        }

        [Fact]
        public void Default_amount_values_are_within_plausible_ranges()
        {
            var config = Harmony_Patch.Instance.Config;

            config.EnergyAmount.Should().BeGreaterThan(0f);
            config.MoneyAmount.Should().BeGreaterThan(0);
            config.GlobalCooldownSeconds.Should().BeGreaterThanOrEqualTo(0f);
        }
    }
}
