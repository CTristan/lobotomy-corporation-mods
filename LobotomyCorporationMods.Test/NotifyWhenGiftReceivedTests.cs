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
        private const int DefaultEquipmentId = 1;
        private const long DefaultGiftId = 1L;
        private readonly List<string> _noticeMessages = new List<string>();

        public NotifyWhenGiftReceivedTests()
        {
            _ = new Harmony_Patch();
            var fileManager = TestExtensions.CreateFileManager();
            Harmony_Patch.Instance.LoadData(fileManager);
        }

        [Theory]
        [InlineData("TestAgent", "TestGift", "<color=" + ColorAgentString + ">TestAgent</color> has received the gift <color=" + ColorGiftString + ">TestGift</color>.")]
        [InlineData("Eke", "Our Galaxy", "<color=" + ColorAgentString + ">Eke</color> has received the gift <color=" + ColorGiftString + ">Our Galaxy</color>.")]
        public void Receiving_a_gift_causes_a_notification([NotNull] string agentName, [NotNull] string giftName, [NotNull] string expectedMessage)
        {
            // Arrange
            var textData = new Dictionary<string, string> { { giftName, giftName } };
            TestExtensions.InitializeTextData(textData);
            InitializeNotice();

            var unitModel = TestExtensions.CreateAgentModel(DefaultAgentId, agentName);
            var gift = TestExtensions.CreateEgoGiftModel(DefaultGiftId, DefaultEquipmentId, giftName);

            // Act
            UnitModelPatchAttachEgoGift.Prefix(unitModel, gift);

            // Assert
            _noticeMessages.Count.Should().BeGreaterThan(0);
            _noticeMessages[0].Should().BeEquivalentTo(expectedMessage);
        }

        #region Helper Methods

        /// <summary>
        ///     Initializes an observer to store any messages sent as a notice.
        /// </summary>
        private void InitializeNotice()
        {
            const string NoticeString = "AddSystemLog";

            var observer = Substitute.For<IObserver>();
            observer.When(x => x.OnNotice(Arg.Any<string>(), Arg.Any<object[]>())).Do(x => _noticeMessages.Add(x.ArgAt<object[]>(1)[0].ToString()));

            Notice.instance.Observe(NoticeString, observer);
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
