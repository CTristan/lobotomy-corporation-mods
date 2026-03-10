// SPDX-License-Identifier: MIT

using AwesomeAssertions;
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
            _ = result.Should().BeNull();
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
            _ = result.Should().BeNull();
        }

        [Fact]
        public void Abnormality_with_no_gift_returns_default_value_for_attachment_type()
        {
            // Arrange
            var managementSlot = UnityTestExtensions.CreateManagementSlot();
            _ = TestExtensions.InitializeCommandWindowWithAbnormality();

            // Act
            var result = managementSlot.GetAbnormalityGiftAttachmentType();

            // Assert
            _ = result.Should().Be(0);
        }
    }
}
