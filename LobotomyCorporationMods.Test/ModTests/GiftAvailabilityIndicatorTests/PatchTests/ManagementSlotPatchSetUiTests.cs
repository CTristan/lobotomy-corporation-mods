// SPDX-License-Identifier: MIT

#region

using System;
using CommandWindow;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using LobotomyCorporationMods.GiftAvailabilityIndicator.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using UnityEngine;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.GiftAvailabilityIndicatorTests.PatchTests
{
    public sealed class ManagementSlotPatchSetUiTests : GiftAvailabilityIndicatorModTests
    {
        private readonly Color _newGiftColor = Color.green;
        private readonly Color _noGiftColor = Color.clear;
        private readonly Color _replacementGiftColor = Color.grey;

        [Fact]
        public void Errors_when_image_file_does_not_exist()
        {
            // Arrange
            var sut = UnityTestExtensions.CreateManagementSlot();
            const string ImageName = nameof(Errors_when_image_file_does_not_exist);
            var agent = TestExtensions.GetAgentWithGift();
            var tool = UnityTestExtensions.CreateUnitModel();
            _ = TestExtensions.InitializeCommandWindowWithAbnormality(tool);
            var mockTexture2dTestAdapter = new Mock<ITexture2dTestAdapter>();

            var mockManagementSlotTestAdapter = new Mock<IManagementSlotTestAdapter>();
            mockManagementSlotTestAdapter.SetupGet(x => x.Name).Returns(ImageName);

            var mockFileManager = new Mock<IFileManager>();
            mockFileManager.Setup(x => x.GetOrCreateFile(ImageName, false)).Returns(string.Empty);

            // Act
            Action action = () => sut.PatchAfterSetUi(agent, ImageName, mockManagementSlotTestAdapter.Object, mockFileManager.Object, texture2dTestAdapter: mockTexture2dTestAdapter.Object);

            // Assert
            action.Should().Throw<InvalidOperationException>().WithMessage("No image found with name " + ImageName);
        }

        [Fact]
        public void Hides_image_when_abnormality_does_not_have_a_gift()
        {
            // Arrange
            var sut = UnityTestExtensions.CreateManagementSlot();
            var creature = UnityTestExtensions.CreateCreatureModel();
            _ = TestExtensions.InitializeCommandWindowWithAbnormality(creature);
            var agent = TestExtensions.GetAgentWithGift(EquipmentIds.CrumblingArmorGift1);
            var mockImageTestAdapter = GetMockImageTestAdapter();

            SetUpSlot(sut, agent, nameof(Hides_image_when_abnormality_does_not_have_a_gift), mockImageTestAdapter);

            mockImageTestAdapter.Object.Color.Should().Be(_noGiftColor);
        }

        [Fact]
        public void Hides_image_when_agent_already_has_the_gift()
        {
            // Arrange
            var sut = UnityTestExtensions.CreateManagementSlot();
            var creature = TestExtensions.GetCreatureWithGift(giftId: EquipmentIds.CrumblingArmorGift1);
            _ = TestExtensions.InitializeCommandWindowWithAbnormality(creature);
            var agent = TestExtensions.GetAgentWithGift(EquipmentIds.CrumblingArmorGift1);
            const string ImageName = nameof(Hides_image_when_abnormality_does_not_have_a_gift);
            var mockImageTestAdapter = GetMockImageTestAdapter();

            SetUpSlot(sut, agent, ImageName, mockImageTestAdapter);

            mockImageTestAdapter.Object.Color.Should().Be(_noGiftColor);
        }

        [Fact]
        public void Hides_image_when_abnormality_is_a_tool()
        {
            // Arrange
            var sut = UnityTestExtensions.CreateManagementSlot();
            var unitModel = UnityTestExtensions.CreateUnitModel();
            _ = UnityTestExtensions.CreateCommandWindow(unitModel);
            var agent = TestExtensions.GetAgentWithGift(EquipmentIds.CrumblingArmorGift1);
            var mockImageTestAdapter = GetMockImageTestAdapter();

            SetUpSlot(sut, agent, nameof(Hides_image_when_abnormality_is_a_tool), mockImageTestAdapter);

            mockImageTestAdapter.Object.Color.Should().Be(_noGiftColor);
        }

        [Theory]
        [InlineData(EGOgiftAttachRegion.EYE, EGOgiftAttachRegion.MOUTH)]
        [InlineData(EGOgiftAttachRegion.LEFTHAND, EGOgiftAttachRegion.MASK)]
        public void Shows_as_new_gift_when_gift_is_in_a_new_slot(EGOgiftAttachRegion firstGiftPosition,
            EGOgiftAttachRegion newGiftPosition)
        {
            // Arrange
            var sut = UnityTestExtensions.CreateManagementSlot();
            var creature = TestExtensions.GetCreatureWithGift(attachPosition: firstGiftPosition);
            _ = TestExtensions.InitializeCommandWindowWithAbnormality(creature);
            var imageName = nameof(Shows_as_new_gift_when_gift_is_in_a_new_slot) + firstGiftPosition + newGiftPosition;
            var agent = TestExtensions.GetAgentWithGift(EquipmentIds.CrumblingArmorGift1, newGiftPosition);
            var mockImageTestAdapter = GetMockImageTestAdapter();

            SetUpSlot(sut, agent, imageName, mockImageTestAdapter);

            mockImageTestAdapter.Object.Color.Should().Be(_newGiftColor);
        }

        [Theory]
        [InlineData(EGOgiftAttachRegion.EYE, EGOgiftAttachRegion.EYE)]
        [InlineData(EGOgiftAttachRegion.LEFTHAND, EGOgiftAttachRegion.LEFTHAND)]
        public void Shows_as_replacement_gift_when_gift_is_in_an_existing_slot(EGOgiftAttachRegion firstGiftPosition,
            EGOgiftAttachRegion newGiftPosition)
        {
            // Arrange
            var sut = UnityTestExtensions.CreateManagementSlot();
            var creature = TestExtensions.GetCreatureWithGift(attachPosition: firstGiftPosition);
            _ = TestExtensions.InitializeCommandWindowWithAbnormality(creature);
            var imageName = nameof(Shows_as_replacement_gift_when_gift_is_in_an_existing_slot) + firstGiftPosition + newGiftPosition;
            var agent = TestExtensions.GetAgentWithGift(EquipmentIds.CrumblingArmorGift1, newGiftPosition);
            var mockImageTestAdapter = GetMockImageTestAdapter();

            SetUpSlot(sut, agent, imageName, mockImageTestAdapter);

            mockImageTestAdapter.Object.Color.Should().Be(_replacementGiftColor);
        }

        #region Helper Methods

        private static void SetUpSlot(ManagementSlot sut,
            AgentModel agent,
            string imageName,
            [NotNull] Mock<IImageTestAdapter> mockImageTestAdapter)
        {
            var fileManager = TestExtensions.GetMockFileManager();
            var mockTexture2dTestAdapter = new Mock<ITexture2dTestAdapter>();
            var mockSpriteTestAdapter = new Mock<ISpriteTestAdapter>();

            var mockTransformTestAdapter = new Mock<ITransformTestAdapter>();
            mockTransformTestAdapter.SetupGet(x => x.Parent).Returns(mockTransformTestAdapter.Object);

            var mockTooltipMouseOverTestAdapter = new Mock<ITooltipMouseOverTestAdapter>();
            mockTooltipMouseOverTestAdapter.SetupGet(x => x.Transform).Returns(mockTransformTestAdapter.Object);

            var mockGameObjectAdapter = new Mock<IGameObjectTestAdapter>();
            mockGameObjectAdapter.SetupGet(x => x.Transform).Returns(mockImageTestAdapter.Object.Transform);
            mockGameObjectAdapter.Setup(x => x.AddImageComponent()).Returns(mockImageTestAdapter.Object);
            mockGameObjectAdapter.Setup(x => x.ImageComponent).Returns(mockImageTestAdapter.Object);

            var mockManagementSlotTestAdapter = new Mock<IManagementSlotTestAdapter>();
            mockManagementSlotTestAdapter.Setup(x => x.Name).Returns(imageName);
            mockManagementSlotTestAdapter.Setup(x => x.Transform.GetChild(It.IsAny<int>())).Returns(mockTransformTestAdapter.Object);

            // Act
            Action action = () => sut.PatchAfterSetUi(agent, imageName, mockManagementSlotTestAdapter.Object, fileManager.Object, mockGameObjectAdapter.Object, mockTexture2dTestAdapter.Object,
                mockSpriteTestAdapter.Object);

            // Assert
            action.Should().NotThrow();
        }

        [NotNull]
        private static Mock<IImageTestAdapter> GetMockImageTestAdapter()
        {
            var mockTransformTestAdapter = new Mock<ITransformTestAdapter>();
            mockTransformTestAdapter.SetupGet(x => x.Parent).Returns(mockTransformTestAdapter.Object);

            var mockTooltipMouseOverTestAdapter = new Mock<ITooltipMouseOverTestAdapter>();
            mockTooltipMouseOverTestAdapter.SetupGet(x => x.Transform).Returns(mockTransformTestAdapter.Object);

            var mockImageTestAdapter = new Mock<IImageTestAdapter>();
            mockImageTestAdapter.SetupAllProperties();
            mockImageTestAdapter.Setup(x => x.AddTooltipMouseOverComponent()).Returns(mockTooltipMouseOverTestAdapter.Object);
            mockImageTestAdapter.Setup(x => x.TooltipMouseOverComponent).Returns(mockTooltipMouseOverTestAdapter.Object);
            mockImageTestAdapter.SetupGet(x => x.Transform).Returns(mockTransformTestAdapter.Object);

            return mockImageTestAdapter;
        }

        #endregion
    }
}
