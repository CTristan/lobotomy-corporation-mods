// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.NotifyWhenGiftReceived.Extensions;
using LobotomyCorporationMods.NotifyWhenGiftReceived.Patches;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking;
using Moq;
using Xunit;
using Xunit.Extensions;

namespace LobotomyCorporationMods.Test
{
    public sealed class NotifyWhenGiftReceivedTests
    {
        private const string ColorAgentString = "#66bfcd";
        private const string ColorGiftString = "#84bd36";
        private const string DefaultAgentName = "DefaultAgentName";
        private const int DefaultGiftId = 1;
        private const EGOgiftAttachRegion DefaultGiftAttachRegion = EGOgiftAttachRegion.HEAD;
        private const long DefaultEquipmentId = 1L;
        private const string DefaultGiftName = "DefaultGiftName";
        private readonly List<string> _noticeMessages = new List<string>();

        public NotifyWhenGiftReceivedTests()
        {
            _ = new Harmony_Patch();
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);

            SetLogObserver();
        }

        [Theory]
        [InlineData(DefaultAgentName, DefaultGiftName,
            "<color=" + ColorAgentString + ">" + DefaultAgentName + "</color> has received the gift <color=" + ColorGiftString + ">" + DefaultGiftName + "</color>.")]
        [InlineData("TestAgent", "TestGift", "<color=" + ColorAgentString + ">TestAgent</color> has received the gift <color=" + ColorGiftString + ">TestGift</color>.")]
        public void Receiving_a_gift_causes_a_notification([NotNull] string agentName, [NotNull] string giftName, [NotNull] string expectedMessage)
        {
            // Arrange
            var gift = GetGift(giftName);
            var unitModel = TestExtensions.CreateAgentModel(name: agentName);

            // Act
            UnitModelPatchAttachEgoGift.Prefix(unitModel, gift);

            // Assert
            _noticeMessages.Count.Should().BeGreaterThan(0);
            _noticeMessages[0].Should().BeEquivalentTo(expectedMessage);
        }

        [Theory]
        [InlineData(DefaultAgentName, DefaultGiftName)]
        [InlineData("TestAgent", "TestGift")]
        public void Receiving_a_duplicate_gift_does_not_cause_a_notification([NotNull] string agentName, [NotNull] string giftName)
        {
            // Arrange
            var gift = GetGift(giftName);
            var unitModel = TestExtensions.CreateAgentModel(name: agentName);
            unitModel.Equipment.gifts.addedGifts.Add(gift);

            // Act
            UnitModelPatchAttachEgoGift.Prefix(unitModel, gift);

            // Assert
            _noticeMessages.Count.Should().Be(0);
        }

        [Theory]
        [InlineData(DefaultAgentName, DefaultGiftName, DefaultGiftAttachRegion)]
        [InlineData("Eke", "Our Galaxy", EGOgiftAttachRegion.RIBBORN)]
        public void Attempting_to_replace_a_locked_gift_does_not_cause_a_notification([NotNull] string agentName, [NotNull] string giftName, EGOgiftAttachRegion attachRegion)
        {
            // Arrange
            GetGift(giftName);
            var unitModel = TestExtensions.CreateAgentModel(name: agentName);
            var oldGift = GetGift(DefaultEquipmentId + 1, DefaultGiftId + 1, DefaultGiftName, attachRegion);
            unitModel.Equipment.gifts.addedGifts.Add(oldGift);
            var giftLockState = new UnitEGOgiftSpace.GiftLockState { id = DefaultEquipmentId + 1, state = true };
            unitModel.Equipment.gifts.lockState.Add(1, giftLockState);

            var newGift = GetGift(giftName, attachRegion);

            // Act
            UnitModelPatchAttachEgoGift.Prefix(unitModel, newGift);

            // Assert
            _noticeMessages.Count.Should().Be(0);
        }

        [Theory]
        [InlineData(DefaultAgentName, DefaultGiftName, EGOgiftAttachRegion.EYE)]
        [InlineData("Eke", "Our Galaxy", EGOgiftAttachRegion.RIBBORN)]
        public void Locked_gift_in_another_position_does_not_prevent_notification([NotNull] string agentName, [NotNull] string giftName, EGOgiftAttachRegion attachRegion)
        {
            // Arrange
            GetGift(giftName);
            var unitModel = TestExtensions.CreateAgentModel(name: agentName);
            var otherGift = GetGift(DefaultEquipmentId + 1, DefaultGiftId + 1, DefaultGiftName, EGOgiftAttachRegion.HEADBACK);
            unitModel.Equipment.gifts.addedGifts.Add(otherGift);
            var giftLockState = new UnitEGOgiftSpace.GiftLockState { id = DefaultEquipmentId + 1, state = true };
            unitModel.Equipment.gifts.lockState.Add(1, giftLockState);

            var newGift = GetGift(giftName, attachRegion);

            // Act
            UnitModelPatchAttachEgoGift.Prefix(unitModel, newGift);

            // Assert
            _noticeMessages.Count.Should().BeGreaterThan(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void Checking_for_equipped_gifts_finds_gift_in_added_gifts_list(int giftId)
        {
            var unitModel = TestExtensions.CreateAgentModel();
            var gift = GetGift(DefaultEquipmentId, giftId, DefaultAgentName, DefaultGiftAttachRegion);
            unitModel.Equipment.gifts.addedGifts.Add(gift);

            var result = unitModel.HasGiftEquipped(giftId);

            result.Should().Be(true);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void Checking_for_equipped_gifts_finds_gift_in_replaced_gifts_list(int giftId)
        {
            var unitModel = TestExtensions.CreateAgentModel();
            var gift = GetGift(DefaultEquipmentId, giftId, DefaultAgentName, DefaultGiftAttachRegion);
            unitModel.Equipment.gifts.replacedGifts.Add(gift);

            var result = unitModel.HasGiftEquipped(giftId);

            result.Should().Be(true);
        }

        #region Helper Methods

        /// <summary>
        ///     Adds the gift name to the engine's localized text data and initializes an observer to store any messages sent as a
        ///     notice.
        /// </summary>
        private static void InitializeTextData(Dictionary<string, string> textData)
        {
            TestExtensions.CreateLocalizeTextDataModel(textData);
        }

        /// <summary>
        ///     Returns a gift object that can be used for tests.
        /// </summary>
        [NotNull]
        private static EGOgiftModel GetGift([NotNull] string giftName)
        {
            return GetGift(DefaultEquipmentId, DefaultGiftId, giftName, DefaultGiftAttachRegion);
        }

        [NotNull]
        private static EGOgiftModel GetGift([NotNull] string giftName, EGOgiftAttachRegion attachRegion)
        {
            return GetGift(DefaultEquipmentId, DefaultGiftId, giftName, attachRegion);
        }

        [NotNull]
        private static EGOgiftModel GetGift(long equipmentId, int giftId, [NotNull] string giftName, EGOgiftAttachRegion attachRegion)
        {
            var textData = new Dictionary<string, string> { { giftName, giftName } };
            InitializeTextData(textData);

            var equipmentNameDictionary = new Dictionary<string, string> { { "name", giftName } };
            var metaInfo = TestExtensions.CreateEquipmentTypeInfo(equipmentNameDictionary);
            metaInfo.id = giftId;
            metaInfo.attachPos = attachRegion.ToString();

            var gift = TestExtensions.CreateEgoGiftModel(metaInfo);
            gift.instanceId = equipmentId;

            return gift;
        }

        private void SetLogObserver()
        {
            var observer = new Mock<IObserver>();
            observer.Setup(x => x.OnNotice(It.IsAny<string>(), It.IsAny<object[]>())).Callback((string _, object[] objectArray) => _noticeMessages.Add(objectArray[0].ToString()));

            Notice.instance.Observe(NoticeName.AddSystemLog, observer.Object);
        }

        #endregion


        #region Harmony Tests

        /// <summary>
        ///     Harmony requires the constructor to be public.
        /// </summary>
        [Fact]
        public void Constructor_is_public_and_externally_accessible()
        {
            Action act = () => _ = new NotifyWhenGiftReceived.Harmony_Patch();
            act.ShouldNotThrow();
        }

        [Fact]
        public void Class_UnitModel_Method_AttachEgoGift_is_patched_correctly()
        {
            // ReSharper disable once StringLiteralTypo
            const string MethodName = "AttachEGOgift";
            var patch = typeof(UnitModelPatchAttachEgoGift);
            var originalClass = typeof(UnitModel);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        #endregion
    }
}
