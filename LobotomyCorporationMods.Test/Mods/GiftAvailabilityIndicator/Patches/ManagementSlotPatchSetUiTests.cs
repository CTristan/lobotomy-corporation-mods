// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using FluentAssertions;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using LobotomyCorporationMods.GiftAvailabilityIndicator.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.GiftAvailabilityIndicator.Patches
{
    public sealed class ManagementSlotPatchSetUiTests : GiftAvailabilityIndicatorTests
    {
        private const string ImageName = "Assets/gift.png";

        [Fact]
        public void Displays_green_icon_when_gift_is_in_a_new_slot()
        {
            // Arrange
            var sut = UnityTestExtensions.CreateManagementSlot();
            var creature = TestExtensions.GetCreatureWithGift();
            _ = TestExtensions.InitializeCommandWindow(creature);
            var agent = TestExtensions.GetAgentWithGift(EquipmentIds.CrumblingArmorGift1, unitBuffs: new List<UnitBuf>());
            var fileManager = TestExtensions.GetMockFileManager();
            var mockTexture2dTestAdapter = new Mock<ITexture2dTestAdapter>();
            var mockSpriteTestAdapter = new Mock<ISpriteTestAdapter>();

            var mockTransformTestAdapter = new Mock<ITransformTestAdapter>();
            mockTransformTestAdapter.SetupGet(x => x.Parent).Returns(mockTransformTestAdapter.Object);

            var mockTooltipMouseOverTestAdapter = new Mock<ITooltipMouseOverTestAdapter>();
            mockTooltipMouseOverTestAdapter.SetupGet(x => x.Transform).Returns(mockTransformTestAdapter.Object);

            var mockImageTestAdapter = new Mock<IImageTestAdapter>();
            mockImageTestAdapter.Setup(x => x.AddTooltipMouseOverComponent()).Returns(mockTooltipMouseOverTestAdapter.Object);
            mockImageTestAdapter.Setup(x => x.TooltipMouseOverComponent).Returns(mockTooltipMouseOverTestAdapter.Object);

            var mockGameObjectAdapter = new Mock<IGameObjectTestAdapter>();
            mockGameObjectAdapter.SetupGet(x => x.Transform).Returns(mockTransformTestAdapter.Object);
            mockGameObjectAdapter.Setup(x => x.AddImageComponent()).Returns(mockImageTestAdapter.Object);
            mockGameObjectAdapter.Setup(x => x.ImageComponent).Returns(mockImageTestAdapter.Object);

            var mockManagementSlotTestAdapter = new Mock<IManagementSlotTestAdapter>();
            mockManagementSlotTestAdapter.Setup(x => x.Name).Returns(ImageName);
            mockManagementSlotTestAdapter.Setup(x => x.Transform.GetChild(It.IsAny<int>())).Returns(mockTransformTestAdapter.Object);

            // Act
            Action action = () => sut.PatchAfterSetUi(agent, mockManagementSlotTestAdapter.Object, fileManager.Object, mockGameObjectAdapter.Object, mockTexture2dTestAdapter.Object,
                mockSpriteTestAdapter.Object);

            // Assert
            action.Should().NotThrow();
        }
    }
}
