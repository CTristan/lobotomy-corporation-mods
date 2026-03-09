// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using LobotomyCorporationMods.Test.Extensions;
using LobotomyCorporationMods.Test.Parameters;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking;
using Moq;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.NotifyWhenAgentReceivesGiftTests
{
    internal class NotifyWhenAgentReceivesGiftModTests
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
            Mock<ILogger> mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);
        }

        protected Mock<INoticeTestAdapter> NoticeTestAdapter { get; } = new Mock<INoticeTestAdapter>();

        [NotNull]
        protected static AgentModel GetAgentWithLockedGift(string agentName,
            EGOgiftAttachRegion attachRegion)
        {
            _ = GetGift(DefaultGiftName);
            AgentModelCreationParameters agentModelCreationParameters = new()
            {
                Name = agentName,
            };

            AgentModel unitModel = UnityTestExtensions.CreateAgentModel(agentModelCreationParameters);
            EGOgiftModel oldGift = GetGift(DefaultGiftName, DefaultEquipmentId + 1, DefaultGiftId + 1, attachRegion);
            unitModel.Equipment.gifts.addedGifts.Add(oldGift);
            UnitEGOgiftSpace.GiftLockState giftLockState = new()
            {
                id = DefaultEquipmentId + 1,
                state = true,
            };

            unitModel.Equipment.gifts.lockState.Add(1, giftLockState);

            return unitModel;
        }

        /// <summary>Returns a gift object that can be used for tests.</summary>
        [NotNull]
        protected static EGOgiftModel GetGift([NotNull] string giftName,
            long equipmentId = DefaultEquipmentId,
            int giftId = DefaultGiftId,
            EGOgiftAttachRegion attachRegion = DefaultGiftAttachRegion)
        {
            Dictionary<string, string> textData = new()
            {
                {
                    giftName, giftName
                },
            };

            InitializeTextData(textData);

            Dictionary<string, string> equipmentNameDictionary = new()
            {
                {
                    "name", giftName
                },
            };

            EquipmentTypeInfo metaInfo = UnityTestExtensions.CreateEquipmentTypeInfo(localizeData: equipmentNameDictionary);
            metaInfo.id = giftId;
            metaInfo.attachPos = attachRegion.ToString();

            EGOgiftModel gift = UnityTestExtensions.CreateEgoGiftModel(metaInfo);
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

            _ = UnityTestExtensions.CreateLocalizeTextDataModel(textData);
        }
    }
}
