// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Patches;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.BadLuckProtectionForGiftsTests.PatchTests
{
    public sealed class GiftSlotPatchSetProbTests
    {
        [Fact]
        public void Gift_chance_display_shows_agent_name_when_agent_has_worked_on_abnormality()
        {
            // Arrange
            const string AgentName = "BongBong";
            const float Prob = 0.112f;
            const string GiftTitle = "Gift";

            // Act
            var result = GiftSlotPatchSetProb.FormatGiftChanceText(Prob, AgentName, GiftTitle, 2);

            // Assert
            result.Should().Be("Gift (BongBong Next Chance:11.20%)");
        }

        [Fact]
        public void Gift_chance_display_is_not_modified_when_no_agent_name_is_available()
        {
            // Act
            var result = GiftSlotPatchSetProb.FormatGiftChanceText(0.1f, null, "Gift", 2);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Gift_chance_display_is_not_modified_when_agent_name_is_empty()
        {
            // Act
            var result = GiftSlotPatchSetProb.FormatGiftChanceText(0.1f, string.Empty, "Gift", 2);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData(0f, 2, "Gift (BongBong Next Chance:0.00%)")]
        [InlineData(0.5f, 2, "Gift (BongBong Next Chance:50.00%)")]
        [InlineData(1f, 2, "Gift (BongBong Next Chance:100.00%)")]
        [InlineData(0.112f, 2, "Gift (BongBong Next Chance:11.20%)")]
        [InlineData(0.112f, 0, "Gift (BongBong Next Chance:11%)")]
        [InlineData(0.112f, 1, "Gift (BongBong Next Chance:11.2%)")]
        [InlineData(0.1126f, 3, "Gift (BongBong Next Chance:11.260%)")]
        public void Gift_chance_display_formats_probability_with_configured_decimal_places(
            float prob,
            int decimalPlaces,
            string expected
        )
        {
            // Act
            var result = GiftSlotPatchSetProb.FormatGiftChanceText(
                prob,
                "BongBong",
                "Gift",
                decimalPlaces
            );

            // Assert
            result.Should().Be(expected);
        }
    }
}
