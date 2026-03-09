// SPDX-License-Identifier: MIT

using AwesomeAssertions;
using CommandWindow;
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
            ManagementSlot managementSlot = UnityTestExtensions.CreateManagementSlot();
            _ = TestExtensions.InitializeCommandWindowWithAbnormality();

            // Act
            int? result = managementSlot.GetAbnormalityGiftId();

            // Assert
            _ = result.Should().BeNull();
        }

        [Fact]
        public void Abnormality_with_no_gift_returns_null_for_position()
        {
            // Arrange
            ManagementSlot managementSlot = UnityTestExtensions.CreateManagementSlot();
            _ = TestExtensions.InitializeCommandWindowWithAbnormality();

            // Act
            string result = managementSlot.GetAbnormalityGiftPosition();

            // Assert
            _ = result.Should().BeNull();
        }

        [Fact]
        public void Abnormality_with_no_gift_returns_default_value_for_attachment_type()
        {
            // Arrange
            ManagementSlot managementSlot = UnityTestExtensions.CreateManagementSlot();
            _ = TestExtensions.InitializeCommandWindowWithAbnormality();

            // Act
            EGOgiftAttachType result = managementSlot.GetAbnormalityGiftAttachmentType();

            // Assert
            _ = result.Should().Be(0);
        }
    }
}
