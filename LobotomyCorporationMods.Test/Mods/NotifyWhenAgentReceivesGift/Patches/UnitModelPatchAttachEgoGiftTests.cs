// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.NotifyWhenAgentReceivesGift.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.NotifyWhenAgentReceivesGift.Patches
{
    public sealed class UnitModelPatchAttachEgoGiftTests : NotifyWhenGiftReceivedTests
    {
        [Theory]
        [InlineData(DefaultAgentName, DefaultGiftName, DefaultGiftAttachRegion)]
        [InlineData("Eke", "Our Galaxy", EGOgiftAttachRegion.RIBBORN)]
        public void Attempting_to_replace_a_locked_gift_does_not_cause_a_notification(string agentName, [NotNull] string giftName, EGOgiftAttachRegion attachRegion)
        {
            // Arrange
            GetGift(giftName);
            var unitModel = UnityTestExtensions.CreateAgentModel(name: agentName);
            var oldGift = GetGift(DefaultGiftName, DefaultEquipmentId + 1, DefaultGiftId + 1, attachRegion);
            unitModel.Equipment.gifts.addedGifts.Add(oldGift);
            var giftLockState = new UnitEGOgiftSpace.GiftLockState { id = DefaultEquipmentId + 1, state = true };
            unitModel.Equipment.gifts.lockState.Add(1, giftLockState);

            var newGift = GetGift(giftName, attachRegion: attachRegion);

            // Act
            UnitModelPatchAttachEgoGift.PatchBeforeAttachEgoGift(unitModel, newGift, NoticeAdapter.Object);

            // Assert
            NoticeAdapter.Verify(adapter => adapter.Send(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void Checking_for_equipped_gifts_finds_gift_in_added_gifts_list(int giftId)
        {
            var unitModel = UnityTestExtensions.CreateAgentModel();
            var gift = GetGift(DefaultAgentName, DefaultEquipmentId, giftId);
            unitModel.Equipment.gifts.addedGifts.Add(gift);

            UnitModelPatchAttachEgoGift.PatchBeforeAttachEgoGift(unitModel, gift, NoticeAdapter.Object);

            // Assert
            NoticeAdapter.Verify(adapter => adapter.Send(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void Checking_for_equipped_gifts_finds_gift_in_replaced_gifts_list(int giftId)
        {
            var unitModel = UnityTestExtensions.CreateAgentModel();
            var gift = GetGift(DefaultAgentName, DefaultEquipmentId, giftId);
            unitModel.Equipment.gifts.replacedGifts.Add(gift);

            UnitModelPatchAttachEgoGift.PatchBeforeAttachEgoGift(unitModel, gift, NoticeAdapter.Object);

            // Assert
            NoticeAdapter.Verify(adapter => adapter.Send(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Theory]
        [InlineData(DefaultAgentName, DefaultGiftName, EGOgiftAttachRegion.EYE)]
        [InlineData("Eke", "Our Galaxy", EGOgiftAttachRegion.RIBBORN)]
        public void Locked_gift_in_another_position_does_not_prevent_notification(string agentName, [NotNull] string giftName, EGOgiftAttachRegion attachRegion)
        {
            // Arrange
            GetGift(giftName);
            var unitModel = UnityTestExtensions.CreateAgentModel(name: agentName);
            var otherGift = GetGift(DefaultGiftName, DefaultEquipmentId + 1, DefaultGiftId + 1, EGOgiftAttachRegion.HEADBACK);
            unitModel.Equipment.gifts.addedGifts.Add(otherGift);
            var giftLockState = new UnitEGOgiftSpace.GiftLockState { id = DefaultEquipmentId + 1, state = true };
            unitModel.Equipment.gifts.lockState.Add(1, giftLockState);

            var newGift = GetGift(giftName, attachRegion: attachRegion);

            // Act
            UnitModelPatchAttachEgoGift.PatchBeforeAttachEgoGift(unitModel, newGift, NoticeAdapter.Object);

            // Assert
            NoticeAdapter.Verify(adapter => adapter.Send(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Theory]
        [InlineData(DefaultAgentName, DefaultGiftName)]
        [InlineData("TestAgent", "TestGift")]
        public void Receiving_a_duplicate_gift_does_not_cause_a_notification(string agentName, [NotNull] string giftName)
        {
            // Arrange
            var gift = GetGift(giftName);
            var unitModel = UnityTestExtensions.CreateAgentModel(name: agentName);
            unitModel.Equipment.gifts.addedGifts.Add(gift);

            // Act
            UnitModelPatchAttachEgoGift.PatchBeforeAttachEgoGift(unitModel, gift, NoticeAdapter.Object);

            // Assert
            NoticeAdapter.Verify(adapter => adapter.Send(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Theory]
        [InlineData(DefaultAgentName,
            DefaultGiftName,
            "<color=" + ColorAgentString + ">" + DefaultAgentName + "</color> has received the gift <color=" + ColorGiftString + ">" + DefaultGiftName + "</color>.")]
        [InlineData("TestAgent", "TestGift", "<color=" + ColorAgentString + ">TestAgent</color> has received the gift <color=" + ColorGiftString + ">TestGift</color>.")]
        public void Receiving_a_gift_causes_a_notification(string agentName, [NotNull] string giftName, string expectedMessage)
        {
            // Arrange
            var gift = GetGift(giftName);
            var unitModel = UnityTestExtensions.CreateAgentModel(name: agentName);
            var noticeMessages = new List<string>();
            NoticeAdapter.Setup(adapter => adapter.Send(It.IsAny<string>(), It.IsAny<object[]>())).Callback((string _, object[] objectArray) => noticeMessages.Add(objectArray[0].ToString()));

            // Act
            UnitModelPatchAttachEgoGift.PatchBeforeAttachEgoGift(unitModel, gift, NoticeAdapter.Object);

            // Assert
            NoticeAdapter.Verify(adapter => adapter.Send(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            noticeMessages.First().Should().Be(expectedMessage);
        }
    }
}
