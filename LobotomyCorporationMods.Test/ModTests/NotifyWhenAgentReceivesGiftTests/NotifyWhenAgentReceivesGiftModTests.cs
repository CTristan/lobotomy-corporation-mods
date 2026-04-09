// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using LobotomyCorporationMods.Test.Extensions;
using LobotomyCorporationMods.Test.Parameters;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking;
using Moq;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.NotifyWhenAgentReceivesGiftTests
{
    public class NotifyWhenAgentReceivesGiftModTests : IDisposable
    {
        protected const string ColorAgentString = "#66bfcd";
        protected const string ColorGiftString = "#84bd36";
        protected const string DefaultAgentName = "DefaultAgentName";
        protected const long DefaultEquipmentId = 1L;
        protected const EGOgiftAttachRegion DefaultGiftAttachRegion = EGOgiftAttachRegion.HEAD;
        protected const int DefaultGiftId = 1;
        protected const string DefaultGiftName = "DefaultGiftName";
        protected const string NotificationLogMessage = "{0} has received the gift {1}.";
        protected const string TestNotificationLogMessage = " has received the gift ";

        protected NotifyWhenAgentReceivesGiftModTests()
        {
            _ = new Harmony_Patch();
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);
        }

        protected Mock<INoticeTestAdapter> NoticeTestAdapter { get; } =
            new Mock<INoticeTestAdapter>();

        [NotNull]
        protected static AgentModel GetAgentWithLockedGift(
            string agentName,
            EGOgiftAttachRegion attachRegion
        )
        {
            GetGift(DefaultGiftName);
            var agentModelCreationParameters = new AgentModelCreationParameters
            {
                Name = agentName,
            };

            var unitModel = UnityTestExtensions.CreateAgentModel(agentModelCreationParameters);
            var oldGift = GetGift(
                DefaultGiftName,
                DefaultEquipmentId + 1,
                DefaultGiftId + 1,
                attachRegion
            );
            unitModel.Equipment.gifts.addedGifts.Add(oldGift);
            var giftLockState = new UnitEGOgiftSpace.GiftLockState
            {
                id = DefaultEquipmentId + 1,
                state = true,
            };

            unitModel.Equipment.gifts.lockState.Add(1, giftLockState);

            return unitModel;
        }

        /// <summary>Returns a gift object that can be used for tests.</summary>
        [NotNull]
        protected static EGOgiftModel GetGift(
            [NotNull] string giftName,
            long equipmentId = DefaultEquipmentId,
            int giftId = DefaultGiftId,
            EGOgiftAttachRegion attachRegion = DefaultGiftAttachRegion
        )
        {
            var textData = new Dictionary<string, string> { { giftName, giftName } };

            InitializeTextData(textData);

            var equipmentNameDictionary = new Dictionary<string, string> { { "name", giftName } };

            var metaInfo = UnityTestExtensions.CreateEquipmentTypeInfo(
                localizeData: equipmentNameDictionary
            );
            metaInfo.id = giftId;
            metaInfo.attachPos = attachRegion.ToString();

            var gift = UnityTestExtensions.CreateEgoGiftModel(metaInfo);
            gift.instanceId = equipmentId;

            return gift;
        }

        /// <summary>Adds the gift name to the engine's localized text data and initializes an observer to store any messages sent as a notice.</summary>
        private static void InitializeTextData([NotNull] Dictionary<string, string> textData)
        {
            // Add the static localized values to the text data
            const string AgentColorCodeId = "NotifyWhenAgentReceivesGift_AgentColorCode";
            const string GiftColorCodeId = "NotifyWhenAgentReceivesGift_GiftColorCode";
            const string LogMessageId = "NotifyWhenAgentReceivesGift_ReceiveGiftMessage";
            textData[AgentColorCodeId] = ColorAgentString;
            textData[GiftColorCodeId] = ColorGiftString;
            textData[LogMessageId] = NotificationLogMessage;

            UnityTestExtensions.CreateLocalizeTextDataModel(textData);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnityTestExtensions.ResetStaticFields();
            }
        }
    }
}
