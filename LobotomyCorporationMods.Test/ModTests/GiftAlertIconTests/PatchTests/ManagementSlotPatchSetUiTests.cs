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

        [Fact]
        public void Only_creates_image_object_once()
        {
            // Arrange
            var sut = UnityTestExtensions.CreateManagementSlot();
            const string ImageName = nameof(Only_creates_image_object_once);
            var creature = UnityTestExtensions.CreateCreatureModel();
            _ = TestExtensions.InitializeCommandWindowWithAbnormality(creature);
            var agent = TestExtensions.GetAgentWithGift(EquipmentIds.CrumblingArmorGift1);
            var fileManager = TestExtensions.GetMockFileManager();
            var testAdapterParameters = SetupTestParameters(ImageName);

            // Act
            // Run twice to see if the image gets created a second time
            sut.PatchAfterSetUi(agent, ImageName, fileManager.Object, testAdapterParameters);
            sut.PatchAfterSetUi(agent, ImageName, fileManager.Object, testAdapterParameters);

            // Assert
            fileManager.Verify(x => x.GetFile(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Errors_when_image_file_does_not_exist()
        {
            // Arrange
            var sut = UnityTestExtensions.CreateManagementSlot();
            const string ImageName = nameof(Errors_when_image_file_does_not_exist);
            var agent = TestExtensions.GetAgentWithGift();
            var tool = UnityTestExtensions.CreateUnitModel();
            _ = TestExtensions.InitializeCommandWindowWithAbnormality(tool);
            var testAdapterParameters = SetupTestParameters(ImageName);

            var mockFileManager = new Mock<IFileManager>();
            mockFileManager.Setup(x => x.GetFile(ImageName)).Returns(string.Empty);

            // Act
            Action action = () => sut.PatchAfterSetUi(agent, ImageName, mockFileManager.Object, testAdapterParameters);

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
            // Act & Assert
            var resultColor = SetupAndReturnImageColor(firstGiftPosition, newGiftPosition, nameof(Shows_as_new_gift_when_gift_is_in_a_new_slot));
            resultColor.Should().Be(_newGiftColor);
        }

        [Theory]
        [InlineData(EGOgiftAttachRegion.EYE, EGOgiftAttachRegion.EYE)]
        [InlineData(EGOgiftAttachRegion.LEFTHAND, EGOgiftAttachRegion.LEFTHAND)]
        public void Shows_as_replacement_gift_when_gift_is_in_an_existing_slot(EGOgiftAttachRegion firstGiftPosition,
            EGOgiftAttachRegion newGiftPosition)
        {
            // Act & Assert
            var resultColor = SetupAndReturnImageColor(firstGiftPosition, newGiftPosition, nameof(Shows_as_replacement_gift_when_gift_is_in_an_existing_slot));
            resultColor.Should().Be(_replacementGiftColor);
        }

        #region Helper Methods

        private static void SetUpSlot(ManagementSlot sut,
            AgentModel agent,
            string imageName,
            [NotNull] Mock<IImageTestAdapter> mockImageTestAdapter)
        {
            var testAdapterParameters = CreateTestAdapterParameters(imageName, mockImageTestAdapter);
            var fileManager = TestExtensions.GetMockFileManager();

            // Act
            Action action = () => sut.PatchAfterSetUi(agent, imageName, fileManager.Object, testAdapterParameters);

            // Assert
            action.Should().NotThrow();
        }

        [NotNull]
        private static OptionalTestAdapterParameters SetupTestParameters(string imageName)
        {
            var mockImageTestAdapter = GetMockImageTestAdapter();
            return CreateTestAdapterParameters(imageName, mockImageTestAdapter);
        }

        [NotNull]
        private static OptionalTestAdapterParameters CreateTestAdapterParameters(string imageName,
            [NotNull] Mock<IImageTestAdapter> mockImageTestAdapter)
        {
            var testAdapterParameters = new OptionalTestAdapterParameters();

            var mockTexture2dTestAdapter = new Mock<ITexture2dTestAdapter>();
            testAdapterParameters.Texture2DTestAdapter = mockTexture2dTestAdapter.Object;

            var mockSpriteTestAdapter = new Mock<ISpriteTestAdapter>();
            testAdapterParameters.SpriteTestAdapter = mockSpriteTestAdapter.Object;

            mockImageTestAdapter.SetupGet(x => x.GameObject).Returns(new Mock<Image>().Object);
            testAdapterParameters.ImageTestAdapter = mockImageTestAdapter.Object;

            var mockGameObjectAdapter = new Mock<IGameObjectTestAdapter>();
            mockGameObjectAdapter.SetupGet(x => x.Transform).Returns(mockImageTestAdapter.Object.Transform);
            mockGameObjectAdapter.Setup(x => x.AddImageComponent()).Returns(mockImageTestAdapter.Object);
            mockGameObjectAdapter.Setup(x => x.ImageComponent).Returns(mockImageTestAdapter.Object);
            testAdapterParameters.GameObjectTestAdapter = mockGameObjectAdapter.Object;

            var mockManagementSlotTestAdapter = new Mock<IManagementSlotTestAdapter>();
            mockManagementSlotTestAdapter.Setup(x => x.Name).Returns(imageName);
            mockManagementSlotTestAdapter.Setup(x => x.Transform.GetChild(It.IsAny<int>())).Returns(mockImageTestAdapter.Object.Transform);
            testAdapterParameters.ManagementSlotTestAdapter = mockManagementSlotTestAdapter.Object;

            return testAdapterParameters;
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

        private static Color SetupAndReturnImageColor(EGOgiftAttachRegion firstGiftPosition,
            EGOgiftAttachRegion newGiftPosition,
            string functionName)
        {
            var sut = UnityTestExtensions.CreateManagementSlot();
            var creature = TestExtensions.GetCreatureWithGift(attachPosition: firstGiftPosition);
            _ = TestExtensions.InitializeCommandWindowWithAbnormality(creature);
            var imageName = functionName + firstGiftPosition + newGiftPosition;
            var agent = TestExtensions.GetAgentWithGift(EquipmentIds.CrumblingArmorGift1, newGiftPosition);
            var mockImageTestAdapter = GetMockImageTestAdapter();
            SetUpSlot(sut, agent, imageName, mockImageTestAdapter);
            return mockImageTestAdapter.Object.Color;
        }

        #endregion
    }
}
