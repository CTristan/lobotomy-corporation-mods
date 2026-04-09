// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using LobotomyCorporationMods.BadLuckProtectionForGifts;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.BadLuckProtectionForGiftsTests
{
    public sealed class DefaultBadLuckProtectionConfigTests
    {
        [Fact]
        public void Gift_chance_decimal_places_defaults_to_two()
        {
            var config = new DefaultBadLuckProtectionConfig();

            config.GiftChanceDecimalPlaces.Should().Be(2);
        }

        [Fact]
        public void Bonus_calculation_mode_defaults_to_normalized()
        {
            var config = new DefaultBadLuckProtectionConfig();

            config.BonusCalculationMode.Should().Be(BonusCalculationMode.Normalized);
        }
    }
}
