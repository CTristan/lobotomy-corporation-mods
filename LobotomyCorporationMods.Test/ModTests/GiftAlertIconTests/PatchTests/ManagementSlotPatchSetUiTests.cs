// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.ParameterObjects;
using LobotomyCorporationMods.GiftAlertIcon.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using UnityEngine;
using UnityEngine.UI;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.GiftAlertIconTests.PatchTests
{
    public sealed class ManagementSlotPatchSetUiTests : GiftAlertIconModTests
    {
        private readonly Color _newGiftColor = Color.green;
        private readonly Color _noGiftColor = Color.clear;
        private readonly Color _replacementGiftColor = Color.grey;
        private readonly ManagementSlot _sut = UnityTestExtensions.CreateManagementSlot();

        [Fact]
        public void Only_creates_image_object_once()
        {
            // Arrange
            const string ImageName = nameof(Only_creates_image_object_once);
            CreatureModel creature = UnityTestExtensions.CreateCreatureModel();
            _ = TestExtensions.InitializeCommandWindowWithAbnormality(creature);
            AgentModel agent = TestExtensions.GetAgentWithGift(EquipmentIds.CrumblingArmorGift1);
            Mock<IFileManager> fileManager = TestExtensions.GetMockFileManager();
            OptionalTestAdapterParameters testAdapterParameters = SetupTestParameters(ImageName);

            // Act
            // Run twice to see if the image gets created a second time
            _sut.PatchAfterSetUi(agent, ImageName, fileManager.Object, testAdapterParameters);
            _sut.PatchAfterSetUi(agent, ImageName, fileManager.Object, testAdapterParameters);

            // Assert
            fileManager.Verify(x => x.GetFile(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Errors_when_image_file_does_not_exist()
        {
            // Arrange
            const string ImageName = nameof(Errors_when_image_file_does_not_exist);
            AgentModel agent = TestExtensions.GetAgentWithGift();
            UnitModel tool = UnityTestExtensions.CreateUnitModel();
            _ = TestExtensions.InitializeCommandWindowWithAbnormality(tool);
            OptionalTestAdapterParameters testAdapterParameters = SetupTestParameters(ImageName);

            Mock<IFileManager> mockFileManager = new();
            _ = mockFileManager.Setup(x => x.GetFile(ImageName)).Returns(string.Empty);

            // Act
            Action action = () => _sut.PatchAfterSetUi(agent, ImageName, mockFileManager.Object, testAdapterParameters);

            // Assert
            _ = action.Should().Throw<InvalidOperationException>().WithMessage("No image found with name " + ImageName);
        }

        [Fact]
        public void Hides_image_when_abnormality_does_not_have_a_gift()
        {
            // Arrange
            CreatureModel creature = UnityTestExtensions.CreateCreatureModel();
            _ = TestExtensions.InitializeCommandWindowWithAbnormality(creature);
            AgentModel agent = TestExtensions.GetAgentWithGift(EquipmentIds.CrumblingArmorGift1);
            Mock<IImageTestAdapter> mockImageTestAdapter = GetMockImageTestAdapter();

            SetUpSlot(_sut, agent, nameof(Hides_image_when_abnormality_does_not_have_a_gift), mockImageTestAdapter);

            _ = mockImageTestAdapter.Object.Color.Should().Be(_noGiftColor);
        }

        [Fact]
        public void Hides_image_when_agent_already_has_the_gift()
        {
            // Arrange
            CreatureModel creature = TestExtensions.GetCreatureWithGift(giftId: EquipmentIds.CrumblingArmorGift1);
            _ = TestExtensions.InitializeCommandWindowWithAbnormality(creature);
            AgentModel agent = TestExtensions.GetAgentWithGift(EquipmentIds.CrumblingArmorGift1);
            const string ImageName = nameof(Hides_image_when_abnormality_does_not_have_a_gift);
            Mock<IImageTestAdapter> mockImageTestAdapter = GetMockImageTestAdapter();

            SetUpSlot(_sut, agent, ImageName, mockImageTestAdapter);

            _ = mockImageTestAdapter.Object.Color.Should().Be(_noGiftColor);
        }

        [Fact]
        public void Hides_image_when_abnormality_is_a_tool()
        {
            // Arrange
            UnitModel unitModel = UnityTestExtensions.CreateUnitModel();
            _ = UnityTestExtensions.CreateCommandWindow(unitModel);
            AgentModel agent = TestExtensions.GetAgentWithGift(EquipmentIds.CrumblingArmorGift1);
            Mock<IImageTestAdapter> mockImageTestAdapter = GetMockImageTestAdapter();

            SetUpSlot(_sut, agent, nameof(Hides_image_when_abnormality_is_a_tool), mockImageTestAdapter);

            _ = mockImageTestAdapter.Object.Color.Should().Be(_noGiftColor);
        }

        [Theory]
        [InlineData(EGOgiftAttachRegion.EYE, EGOgiftAttachRegion.MOUTH)]
        [InlineData(EGOgiftAttachRegion.LEFTHAND, EGOgiftAttachRegion.MASK)]
        public void Shows_as_new_gift_when_gift_is_in_a_new_slot_and_same_attachment_type(EGOgiftAttachRegion firstGiftPosition,
            EGOgiftAttachRegion newGiftPosition)
        {
            // Act & Assert
            Color resultColor = SetupAndReturnImageColor(nameof(Shows_as_new_gift_when_gift_is_in_a_new_slot_and_same_attachment_type), firstGiftPosition, newGiftPosition);
            _ = resultColor.Should().Be(_newGiftColor);
        }

        [Theory]
        [InlineData(EGOgiftAttachRegion.EYE, EGOgiftAttachRegion.MOUTH, EGOgiftAttachType.ADD, EGOgiftAttachType.REPLACE)]
        [InlineData(EGOgiftAttachRegion.LEFTHAND, EGOgiftAttachRegion.MASK, EGOgiftAttachType.SPECIAL_ADD, EGOgiftAttachType.REPLACE)]
        public void Shows_as_new_gift_when_gift_is_in_a_new_slot_and_different_attachment_type(EGOgiftAttachRegion firstGiftPosition,
            EGOgiftAttachRegion newGiftPosition,
            EGOgiftAttachType firstGiftAttachmentType,
            EGOgiftAttachType newGiftAttachmentType)
        {
            // Act & Assert
            Color resultColor = SetupAndReturnImageColor(nameof(Shows_as_new_gift_when_gift_is_in_a_new_slot_and_different_attachment_type), firstGiftPosition, newGiftPosition, firstGiftAttachmentType,
                newGiftAttachmentType);

            _ = resultColor.Should().Be(_newGiftColor);
        }

        [Theory]
        [InlineData(EGOgiftAttachRegion.EYE, EGOgiftAttachType.ADD)]
        [InlineData(EGOgiftAttachRegion.LEFTHAND, EGOgiftAttachType.SPECIAL_ADD)]
        public void Shows_as_replacement_gift_when_gift_is_in_an_existing_slot_and_has_same_attachment_type(EGOgiftAttachRegion giftPosition,
            EGOgiftAttachType giftAttachmentType)
        {
            // Act & Assert
            Color resultColor = SetupAndReturnImageColor(nameof(Shows_as_replacement_gift_when_gift_is_in_an_existing_slot_and_has_same_attachment_type), giftPosition, giftPosition, giftAttachmentType,
                giftAttachmentType);

            _ = resultColor.Should().Be(_replacementGiftColor);
        }

        [Theory]
        [InlineData(EGOgiftAttachType.ADD, EGOgiftAttachType.SPECIAL_ADD)]
        [InlineData(EGOgiftAttachType.REPLACE, EGOgiftAttachType.ADD)]
        public void Shows_as_new_gift_when_gift_is_in_existing_slot_but_has_different_attachment_type(EGOgiftAttachType firstGiftAttachmentType,
            EGOgiftAttachType newGiftAttachmentType)
        {
            // Act & Assert
            Color resultColor = SetupAndReturnImageColor(nameof(Shows_as_new_gift_when_gift_is_in_existing_slot_but_has_different_attachment_type), firstGiftAttachmentType: firstGiftAttachmentType,
                newGiftAttachmentType: newGiftAttachmentType);

            _ = resultColor.Should().Be(_newGiftColor);
        }

        #region Helper Methods

        private static void SetUpSlot(ManagementSlot sut,
            AgentModel agent,
            string imageName,
            [NotNull] Mock<IImageTestAdapter> mockImageTestAdapter)
        {
            OptionalTestAdapterParameters testAdapterParameters = CreateTestAdapterParameters(imageName, mockImageTestAdapter);
            Mock<IFileManager> fileManager = TestExtensions.GetMockFileManager();

            // Act
            Action action = () => sut.PatchAfterSetUi(agent, imageName, fileManager.Object, testAdapterParameters);

            // Assert
            _ = action.Should().NotThrow();
        }

        [NotNull]
        private static OptionalTestAdapterParameters SetupTestParameters(string imageName)
        {
            Mock<IImageTestAdapter> mockImageTestAdapter = GetMockImageTestAdapter();

            return CreateTestAdapterParameters(imageName, mockImageTestAdapter);
        }

        [NotNull]
        private static OptionalTestAdapterParameters CreateTestAdapterParameters(string imageName,
            [NotNull] Mock<IImageTestAdapter> mockImageTestAdapter)
        {
            OptionalTestAdapterParameters testAdapterParameters = new();

            Mock<ITexture2dTestAdapter> mockTexture2dTestAdapter = new();
            testAdapterParameters.Texture2DTestAdapter = mockTexture2dTestAdapter.Object;

            Mock<ISpriteTestAdapter> mockSpriteTestAdapter = new();
            testAdapterParameters.SpriteTestAdapter = mockSpriteTestAdapter.Object;

            _ = mockImageTestAdapter.SetupGet(x => x.GameObject).Returns(new Mock<Image>().Object);
            testAdapterParameters.ImageTestAdapter = mockImageTestAdapter.Object;

            Mock<IGameObjectTestAdapter> mockGameObjectAdapter = new();
            _ = mockGameObjectAdapter.SetupGet(x => x.Transform).Returns(mockImageTestAdapter.Object.Transform);
            _ = mockGameObjectAdapter.Setup(x => x.AddImageComponent()).Returns(mockImageTestAdapter.Object);
            _ = mockGameObjectAdapter.Setup(x => x.ImageComponent).Returns(mockImageTestAdapter.Object);
            testAdapterParameters.GameObjectTestAdapter = mockGameObjectAdapter.Object;

            Mock<IManagementSlotTestAdapter> mockManagementSlotTestAdapter = new();
            _ = mockManagementSlotTestAdapter.Setup(x => x.Name).Returns(imageName);
            _ = mockManagementSlotTestAdapter.Setup(x => x.Transform.GetChild(It.IsAny<int>())).Returns(mockImageTestAdapter.Object.Transform);
            testAdapterParameters.ManagementSlotTestAdapter = mockManagementSlotTestAdapter.Object;

            return testAdapterParameters;
        }

        [NotNull]
        private static Mock<IImageTestAdapter> GetMockImageTestAdapter()
        {
            Mock<ITransformTestAdapter> mockTransformTestAdapter = new();
            _ = mockTransformTestAdapter.SetupGet(x => x.Parent).Returns(mockTransformTestAdapter.Object);

            Mock<ITooltipMouseOverTestAdapter> mockTooltipMouseOverTestAdapter = new();
            _ = mockTooltipMouseOverTestAdapter.SetupGet(x => x.Transform).Returns(mockTransformTestAdapter.Object);

            Mock<IImageTestAdapter> mockImageTestAdapter = new();
            _ = mockImageTestAdapter.SetupAllProperties();
            _ = mockImageTestAdapter.Setup(x => x.AddTooltipMouseOverComponent()).Returns(mockTooltipMouseOverTestAdapter.Object);
            _ = mockImageTestAdapter.Setup(x => x.TooltipMouseOverComponent).Returns(mockTooltipMouseOverTestAdapter.Object);
            _ = mockImageTestAdapter.SetupGet(x => x.Transform).Returns(mockTransformTestAdapter.Object);

            return mockImageTestAdapter;
        }

        private Color SetupAndReturnImageColor(string functionName,
            EGOgiftAttachRegion firstGiftPosition = 0,
            EGOgiftAttachRegion newGiftPosition = 0,
            EGOgiftAttachType firstGiftAttachmentType = 0,
            EGOgiftAttachType newGiftAttachmentType = 0)
        {
            CreatureModel creature = TestExtensions.GetCreatureWithGift(attachPosition: firstGiftPosition, giftAttachType: firstGiftAttachmentType);
            _ = TestExtensions.InitializeCommandWindowWithAbnormality(creature);
            string imageName = functionName + firstGiftPosition + newGiftPosition + firstGiftAttachmentType + newGiftAttachmentType;
            AgentModel agent = TestExtensions.GetAgentWithGift(EquipmentIds.CrumblingArmorGift1, newGiftPosition, newGiftAttachmentType);
            Mock<IImageTestAdapter> mockImageTestAdapter = GetMockImageTestAdapter();
            SetUpSlot(_sut, agent, imageName, mockImageTestAdapter);

            return mockImageTestAdapter.Object.Color;
        }

        #endregion
    }
}
