// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.NotifyWhenGiftReceived;
using LobotomyCorporationMods.NotifyWhenGiftReceived.Patches;
using NSubstitute;
using Xunit;
using Xunit.Extensions;

namespace LobotomyCorporationMods.Test
{
    public sealed class NotifyWhenGiftReceivedTests
    {
        private const string ColorAgentString = "#66bfcd";
        private const string ColorGiftString = "#84bd36";
        private const long DefaultAgentId = 1L;
        private const string DefaultAgentName = "DefaultAgentName";
        private const int DefaultEquipmentId = 1;
        private const EGOgiftAttachRegion DefaultGiftAttachRegion = EGOgiftAttachRegion.HEAD;
        private const long DefaultGiftId = 1L;
        private const string DefaultGiftName = "DefaultGiftName";
        private readonly List<string> _noticeMessages = new List<string>();

        public NotifyWhenGiftReceivedTests()
        {
            _ = new Harmony_Patch();
            var fileManager = TestExtensions.CreateFileManager();
            Harmony_Patch.Instance.LoadData(fileManager);
        }

        [Theory]
        [InlineData(DefaultAgentName, DefaultGiftName,
            "<color=" + ColorAgentString + ">" + DefaultAgentName + "</color> has received the gift <color=" + ColorGiftString + ">" + DefaultGiftName + "</color>.")]
        [InlineData("TestAgent", "TestGift", "<color=" + ColorAgentString + ">TestAgent</color> has received the gift <color=" + ColorGiftString + ">TestGift</color>.")]
        public void Receiving_a_gift_causes_a_notification([NotNull] string agentName, [NotNull] string giftName, [NotNull] string expectedMessage)
        {
            // Arrange
            AddGiftName(giftName);
            var unitModel = CreateAgentModel(agentName);
            var gift = CreateEgoGiftModel(giftName);

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
            AddGiftName(giftName);
            var unitModel = CreateAgentModel(agentName);
            var gift = CreateEgoGiftModel(giftName);
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
            AddGiftName(giftName);
            var unitModel = CreateAgentModel(agentName);
            var oldGift = CreateEgoGiftModel(DefaultGiftId + 1, DefaultEquipmentId + 1, DefaultGiftName, attachRegion);
            unitModel.Equipment.gifts.addedGifts.Add(oldGift);
            var giftLockState = new UnitEGOgiftSpace.GiftLockState { id = DefaultGiftId + 1, state = true };
            unitModel.Equipment.gifts.lockState.Add(1, giftLockState);

            var newGift = CreateEgoGiftModel(giftName, attachRegion);

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
            AddGiftName(giftName);
            var unitModel = CreateAgentModel(agentName);
            var otherGift = CreateEgoGiftModel(DefaultGiftId + 1, DefaultEquipmentId + 1, DefaultGiftName, EGOgiftAttachRegion.HEADBACK);
            unitModel.Equipment.gifts.addedGifts.Add(otherGift);
            var giftLockState = new UnitEGOgiftSpace.GiftLockState { id = DefaultGiftId + 1, state = true };
            unitModel.Equipment.gifts.lockState.Add(1, giftLockState);

            var newGift = CreateEgoGiftModel(giftName, attachRegion);

            // Act
            UnitModelPatchAttachEgoGift.Prefix(unitModel, newGift);

            // Assert
            _noticeMessages.Count.Should().BeGreaterThan(0);
        }

        #region Helper Methods

        /// <summary>
        ///     Adds the gift name to the engine's localized text data and initializes an observer to store any messages sent as a
        ///     notice.
        /// </summary>
        private void AddGiftName([NotNull] string giftName)
        {
            var textData = new Dictionary<string, string> { { giftName, giftName } };
            TestExtensions.InitializeTextData(textData);

            var observer = Substitute.For<IObserver>();
            observer.When(x => x.OnNotice(Arg.Any<string>(), Arg.Any<object[]>())).Do(x => _noticeMessages.Add(x.ArgAt<object[]>(1)[0].ToString()));

            const string NoticeString = "AddSystemLog";
            Notice.instance.Observe(NoticeString, observer);
        }

        [NotNull]
        private static AgentModel CreateAgentModel(string agentName)
        {
            return TestExtensions.CreateAgentModel(DefaultAgentId, agentName);
        }

        [NotNull]
        private static EGOgiftModel CreateEgoGiftModel(string giftName)
        {
            return TestExtensions.CreateEgoGiftModel(DefaultGiftId, DefaultEquipmentId, giftName, DefaultGiftAttachRegion.ToString());
        }

        [NotNull]
        private static EGOgiftModel CreateEgoGiftModel(string giftName, EGOgiftAttachRegion giftAttachRegion)
        {
            return TestExtensions.CreateEgoGiftModel(DefaultGiftId, DefaultEquipmentId, giftName, giftAttachRegion.ToString());
        }

        [NotNull]
        private static EGOgiftModel CreateEgoGiftModel(long giftId, int equipmentId, string giftName, EGOgiftAttachRegion giftAttachRegion)
        {
            return TestExtensions.CreateEgoGiftModel(giftId, equipmentId, giftName, giftAttachRegion.ToString());
        }

        #endregion

        #region Harmony Tests

        /// <summary>
        ///     Harmony requires the constructor to be public.
        /// </summary>
        [Fact]
        public void Constructor_is_public_and_externally_accessible()
        {
            Action act = () => _ = new Harmony_Patch();
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
