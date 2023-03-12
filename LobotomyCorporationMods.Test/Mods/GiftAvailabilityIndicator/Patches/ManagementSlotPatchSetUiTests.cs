// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using FluentAssertions;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.GiftAvailabilityIndicator.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
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
            var agent = TestExtensions.GetAgentWithGift(EquipmentId.CrumblingArmorGift1, new List<UnitBuf>());
            var creature = TestExtensions.GetCreatureWithGift();
            var commandWindow = TestExtensions.InitializeCommandWindow(creature);

            var mockImageGameObject = GetMockImageGameObject();
            var mockComponentAdapter = new Mock<IComponentAdapter>();
            mockComponentAdapter.Setup(static adapter => adapter.GameObjectAdapter).Returns(mockImageGameObject.Object);
            mockComponentAdapter.Setup(static adapter => adapter.Name).Returns("Slot");

            var mockFileManager = TestExtensions.GetMockFileManager();
            var mockTextureAdapter = new Mock<ITexture2DAdapter>();
            var mockSpriteAdapter = new Mock<ISpriteAdapter>();
            var mockImageAdapter = new Mock<IImageAdapter>();
            mockImageAdapter.Setup(static adapter => adapter.GameObjectAdapter).Returns(mockImageGameObject.Object);

            // Act
            Action action = () => mockComponentAdapter.Object.PatchAfterSetUi(agent,
                commandWindow,
                mockImageGameObject.Object,
                mockFileManager.Object,
                mockTextureAdapter.Object,
                mockSpriteAdapter.Object,
                mockImageAdapter.Object);

            // Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void Does_not_display_icon_when_observation_level_is_less_than_max()
        {
            var managementSlot = TestUnityExtensions.CreateManagementSlot();
            var agent = TestUnityExtensions.CreateAgentModel();

            Action action = () => ManagementSlotPatchSetUi.Postfix(managementSlot, agent);

            throw new NotImplementedException();
        }
    }
}
