// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Abstractions;
using LobotomyCorporation.Mods.Common;
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
            var creature = UnityTestExtensions.CreateCreatureModel();
            _ = TestExtensions.InitializeCommandWindowWithAbnormality(creature);
            var agent = TestExtensions.GetAgentWithGift(EquipmentIds.CrumblingArmorGift1);
            var fileManager = TestExtensions.GetMockFileManager();
            var mockImageInternals = GetMockImageInternals();
            var optionalOverrides = CreateInternalsParameters(ImageName, mockImageInternals);

            // Set up FindChild to return null on first call (create path),
            // then return an existing image on subsequent calls (cached fast path).
            var mockGameObjectInternals = new Mock<IGameObjectInternals>();
            mockGameObjectInternals.Setup(x => x.ImageComponent).Returns(mockImageInternals.Object);
            var parentTransform = optionalOverrides.ManagementSlotInternals.Transform.GetChild(0);
            Mock.Get(parentTransform)
                .SetupSequence(x => x.FindChild(It.IsAny<string>()))
                .Returns((IGameObjectInternals)null)
                .Returns((IGameObjectInternals)null)
                .Returns(mockGameObjectInternals.Object);

            // Act
            // Run twice to see if the image gets created a second time
            _sut.PatchAfterSetUi(agent, ImageName, fileManager.Object, optionalOverrides);
            _sut.PatchAfterSetUi(agent, ImageName, fileManager.Object, optionalOverrides);

            // Assert — GetFile is only called on the first invocation (creation path)
            fileManager.Verify(x => x.GetFile(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Errors_when_image_file_does_not_exist()
        {
            // Arrange
            const string ImageName = nameof(Errors_when_image_file_does_not_exist);
            var agent = TestExtensions.GetAgentWithGift();
            var tool = UnityTestExtensions.CreateUnitModel();
            _ = TestExtensions.InitializeCommandWindowWithAbnormality(tool);
            var optionalOverrides = SetupTestParameters(ImageName);

            var mockFile = new Mock<IFile>();
            mockFile.Setup(f => f.Exists(It.IsAny<string>())).Returns(false);
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.SetupGet(fs => fs.File).Returns(mockFile.Object);
            var mockFileManager = new Mock<IFileManager>();
            mockFileManager.SetupGet(fm => fm.FileSystem).Returns(mockFileSystem.Object);
            mockFileManager.Setup(x => x.GetFile(ImageName)).Returns(string.Empty);

            // Act
            Action action = () =>
                _sut.PatchAfterSetUi(agent, ImageName, mockFileManager.Object, optionalOverrides);

            // Assert
            action
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("No image found with name " + ImageName);
        }

        [Fact]
        public void Hides_image_when_abnormality_does_not_have_a_gift()
        {
            // Arrange
            var creature = UnityTestExtensions.CreateCreatureModel();
            _ = TestExtensions.InitializeCommandWindowWithAbnormality(creature);
            var agent = TestExtensions.GetAgentWithGift(EquipmentIds.CrumblingArmorGift1);
            var mockImageInternals = GetMockImageInternals();

            SetUpSlot(
                _sut,
                agent,
                nameof(Hides_image_when_abnormality_does_not_have_a_gift),
                mockImageInternals
            );

            mockImageInternals.Object.Color.Should().Be(_noGiftColor);
        }

        [Fact]
        public void Hides_image_when_agent_already_has_the_gift()
        {
            // Arrange
            var creature = TestExtensions.GetCreatureWithGift(
                giftId: EquipmentIds.CrumblingArmorGift1
            );
            _ = TestExtensions.InitializeCommandWindowWithAbnormality(creature);
            var agent = TestExtensions.GetAgentWithGift(EquipmentIds.CrumblingArmorGift1);
            const string ImageName = nameof(Hides_image_when_abnormality_does_not_have_a_gift);
            var mockImageInternals = GetMockImageInternals();

            SetUpSlot(_sut, agent, ImageName, mockImageInternals);

            mockImageInternals.Object.Color.Should().Be(_noGiftColor);
        }

        [Fact]
        public void Hides_image_when_abnormality_is_a_tool()
        {
            // Arrange
            var unitModel = UnityTestExtensions.CreateUnitModel();
            _ = UnityTestExtensions.CreateCommandWindow(unitModel);
            var agent = TestExtensions.GetAgentWithGift(EquipmentIds.CrumblingArmorGift1);
            var mockImageInternals = GetMockImageInternals();

            SetUpSlot(
                _sut,
                agent,
                nameof(Hides_image_when_abnormality_is_a_tool),
                mockImageInternals
            );

            mockImageInternals.Object.Color.Should().Be(_noGiftColor);
        }

        [Theory]
        [InlineData(EGOgiftAttachRegion.EYE, EGOgiftAttachRegion.MOUTH)]
        [InlineData(EGOgiftAttachRegion.LEFTHAND, EGOgiftAttachRegion.MASK)]
        public void Shows_as_new_gift_when_gift_is_in_a_new_slot_and_same_attachment_type(
            EGOgiftAttachRegion firstGiftPosition,
            EGOgiftAttachRegion newGiftPosition
        )
        {
            // Act & Assert
            var resultColor = SetupAndReturnImageColor(
                nameof(Shows_as_new_gift_when_gift_is_in_a_new_slot_and_same_attachment_type),
                firstGiftPosition,
                newGiftPosition
            );
            resultColor.Should().Be(_newGiftColor);
        }

        [Theory]
        [InlineData(
            EGOgiftAttachRegion.EYE,
            EGOgiftAttachRegion.MOUTH,
            EGOgiftAttachType.ADD,
            EGOgiftAttachType.REPLACE
        )]
        [InlineData(
            EGOgiftAttachRegion.LEFTHAND,
            EGOgiftAttachRegion.MASK,
            EGOgiftAttachType.SPECIAL_ADD,
            EGOgiftAttachType.REPLACE
        )]
        public void Shows_as_new_gift_when_gift_is_in_a_new_slot_and_different_attachment_type(
            EGOgiftAttachRegion firstGiftPosition,
            EGOgiftAttachRegion newGiftPosition,
            EGOgiftAttachType firstGiftAttachmentType,
            EGOgiftAttachType newGiftAttachmentType
        )
        {
            // Act & Assert
            var resultColor = SetupAndReturnImageColor(
                nameof(Shows_as_new_gift_when_gift_is_in_a_new_slot_and_different_attachment_type),
                firstGiftPosition,
                newGiftPosition,
                firstGiftAttachmentType,
                newGiftAttachmentType
            );

            resultColor.Should().Be(_newGiftColor);
        }

        [Theory]
        [InlineData(EGOgiftAttachRegion.EYE, EGOgiftAttachType.ADD)]
        [InlineData(EGOgiftAttachRegion.LEFTHAND, EGOgiftAttachType.SPECIAL_ADD)]
        public void Shows_as_replacement_gift_when_gift_is_in_an_existing_slot_and_has_same_attachment_type(
            EGOgiftAttachRegion giftPosition,
            EGOgiftAttachType giftAttachmentType
        )
        {
            // Act & Assert
            var resultColor = SetupAndReturnImageColor(
                nameof(
                    Shows_as_replacement_gift_when_gift_is_in_an_existing_slot_and_has_same_attachment_type
                ),
                giftPosition,
                giftPosition,
                giftAttachmentType,
                giftAttachmentType
            );

            resultColor.Should().Be(_replacementGiftColor);
        }

        [Theory]
        [InlineData(EGOgiftAttachType.ADD, EGOgiftAttachType.SPECIAL_ADD)]
        [InlineData(EGOgiftAttachType.REPLACE, EGOgiftAttachType.ADD)]
        public void Shows_as_new_gift_when_gift_is_in_existing_slot_but_has_different_attachment_type(
            EGOgiftAttachType firstGiftAttachmentType,
            EGOgiftAttachType newGiftAttachmentType
        )
        {
            // Act & Assert
            var resultColor = SetupAndReturnImageColor(
                nameof(
                    Shows_as_new_gift_when_gift_is_in_existing_slot_but_has_different_attachment_type
                ),
                firstGiftAttachmentType: firstGiftAttachmentType,
                newGiftAttachmentType: newGiftAttachmentType
            );

            resultColor.Should().Be(_newGiftColor);
        }

        #region Helper Methods

        private static void SetUpSlot(
            ManagementSlot sut,
            AgentModel agent,
            string imageName,
            [NotNull] Mock<IImageInternals> mockImageInternals
        )
        {
            var optionalOverrides = CreateInternalsParameters(imageName, mockImageInternals);
            var fileManager = TestExtensions.GetMockFileManager();

            // Act
            Action action = () =>
                sut.PatchAfterSetUi(agent, imageName, fileManager.Object, optionalOverrides);

            // Assert
            action.Should().NotThrow();
        }

        [NotNull]
        private static OptionalOverrides SetupTestParameters(string imageName)
        {
            var mockImageInternals = GetMockImageInternals();

            return CreateInternalsParameters(imageName, mockImageInternals);
        }

        [NotNull]
        private static OptionalOverrides CreateInternalsParameters(
            string imageName,
            [NotNull] Mock<IImageInternals> mockImageInternals
        )
        {
            var optionalOverrides = new OptionalOverrides();

            var mockTexture2dInternals = new Mock<ITexture2dInternals>();
            optionalOverrides.Texture2DInternals = mockTexture2dInternals.Object;

            var mockSpriteInternals = new Mock<ISpriteInternals>();
            optionalOverrides.SpriteInternals = mockSpriteInternals.Object;

            mockImageInternals.SetupGet(x => x.GameObject).Returns(new Mock<Image>().Object);
            optionalOverrides.ImageInternals = mockImageInternals.Object;

            var mockGameObjectAdapter = new Mock<IGameObjectInternals>();
            mockGameObjectAdapter
                .SetupGet(x => x.Transform)
                .Returns(mockImageInternals.Object.Transform);
            mockGameObjectAdapter
                .Setup(x => x.AddImageComponent())
                .Returns(mockImageInternals.Object);
            mockGameObjectAdapter.Setup(x => x.ImageComponent).Returns(mockImageInternals.Object);
            optionalOverrides.GameObjectInternals = mockGameObjectAdapter.Object;

            var mockManagementSlotInternals = new Mock<IManagementSlotInternals>();
            mockManagementSlotInternals.Setup(x => x.Name).Returns(imageName);
            mockManagementSlotInternals
                .Setup(x => x.Transform.GetChild(It.IsAny<int>()))
                .Returns(mockImageInternals.Object.Transform);
            optionalOverrides.ManagementSlotInternals = mockManagementSlotInternals.Object;

            return optionalOverrides;
        }

        [NotNull]
        private static Mock<IImageInternals> GetMockImageInternals()
        {
            var mockTransformInternals = new Mock<ITransformInternals>();
            mockTransformInternals.SetupGet(x => x.Parent).Returns(mockTransformInternals.Object);

            var mockTooltipMouseOverInternals = new Mock<ITooltipMouseOverInternals>();
            mockTooltipMouseOverInternals
                .SetupGet(x => x.Transform)
                .Returns(mockTransformInternals.Object);

            var mockImageInternals = new Mock<IImageInternals>();
            mockImageInternals.SetupAllProperties();
            mockImageInternals
                .Setup(x => x.AddTooltipMouseOverComponent())
                .Returns(mockTooltipMouseOverInternals.Object);
            mockImageInternals
                .Setup(x => x.TooltipMouseOverComponent)
                .Returns(mockTooltipMouseOverInternals.Object);
            mockImageInternals.SetupGet(x => x.Transform).Returns(mockTransformInternals.Object);

            return mockImageInternals;
        }

        private Color SetupAndReturnImageColor(
            string functionName,
            EGOgiftAttachRegion firstGiftPosition = 0,
            EGOgiftAttachRegion newGiftPosition = 0,
            EGOgiftAttachType firstGiftAttachmentType = 0,
            EGOgiftAttachType newGiftAttachmentType = 0
        )
        {
            var creature = TestExtensions.GetCreatureWithGift(
                attachPosition: firstGiftPosition,
                giftAttachType: firstGiftAttachmentType
            );
            _ = TestExtensions.InitializeCommandWindowWithAbnormality(creature);
            var imageName =
                functionName
                + firstGiftPosition
                + newGiftPosition
                + firstGiftAttachmentType
                + newGiftAttachmentType;
            var agent = TestExtensions.GetAgentWithGift(
                EquipmentIds.CrumblingArmorGift1,
                newGiftPosition,
                newGiftAttachmentType
            );
            var mockImageInternals = GetMockImageInternals();
            SetUpSlot(_sut, agent, imageName, mockImageInternals);

            return mockImageInternals.Object.Color;
        }

        #endregion
    }
}
