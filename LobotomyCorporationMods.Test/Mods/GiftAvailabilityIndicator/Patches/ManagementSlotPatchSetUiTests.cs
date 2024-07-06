// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using FluentAssertions;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.GiftAvailabilityIndicator.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.GiftAvailabilityIndicator.Patches
{
    public sealed class ManagementSlotPatchSetUiTests : GiftAvailabilityIndicatorTests
    {
        [Fact]
        public void Displays_green_icon_when_gift_is_in_a_new_slot()
        {
            // Arrange
            var sut = UnityTestExtensions.CreateManagementSlot();
            var agent = TestExtensions.GetAgentWithGift(EquipmentIds.CrumblingArmorGift1, new List<UnitBuf>());

            // Act
            Action action = () => sut.PatchAfterSetUi(agent);

            // Assert
            action.Should().NotThrow();
        }

        // [Fact]
        // public void Does_not_display_icon_when_observation_level_is_less_than_max()
        // {
        //     var sut = UnityTestExtensions.CreateManagementSlot();
        //     var agent = UnityTestExtensions.CreateAgentModel();
        //
        //     Action action = () => sut.PatchAfterSetUi(sut, agent);
        //
        //     throw new NotImplementedException();
        // }
    }
}
