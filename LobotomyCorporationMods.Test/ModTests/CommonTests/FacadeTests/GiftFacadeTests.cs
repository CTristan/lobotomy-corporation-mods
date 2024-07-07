// SPDX-License-Identifier: MIT

using FluentAssertions;
using LobotomyCorporationMods.Common.Implementations.Facades;
using LobotomyCorporationMods.Test.Extensions;
using Xunit;

namespace LobotomyCorporationMods.Test.ModTests.CommonTests.FacadeTests
{
    public sealed class GiftFacadeTests
    {
        [Fact]
        public void Abnormality_with_no_gift_returns_null_for_id()
        {
            // Arrange
            var managementSlot = UnityTestExtensions.CreateManagementSlot();
            _ = TestExtensions.InitializeCommandWindowWithAbnormality();

            // Act
            var result = managementSlot.GetAbnormalityGiftId();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Abnormality_with_no_gift_returns_null_for_position()
        {
            // Arrange
            var managementSlot = UnityTestExtensions.CreateManagementSlot();
            _ = TestExtensions.InitializeCommandWindowWithAbnormality();

            // Act
            var result = managementSlot.GetAbnormalityGiftPosition();

            // Assert
            result.Should().BeNull();
        }
    }
}
