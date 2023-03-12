// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Test.Extensions;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking;
using Moq;

#endregion

namespace LobotomyCorporationMods.Test.Mods.NotifyWhenAgentReceivesGift
{
    public class NotifyWhenGiftReceivedTests
    {
        protected const string ColorAgentString = "#66bfcd";
        protected const string ColorGiftString = "#84bd36";
        protected const string DefaultAgentName = "DefaultAgentName";
        protected const long DefaultEquipmentId = 1L;
        protected const EGOgiftAttachRegion DefaultGiftAttachRegion = EGOgiftAttachRegion.HEAD;
        protected const int DefaultGiftId = 1;
        protected const string DefaultGiftName = "DefaultGiftName";

        protected NotifyWhenGiftReceivedTests()
        {
            _ = new Harmony_Patch();
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);
        }

        protected Mock<INoticeAdapter> NoticeAdapter { get; } = new();

        /// <summary>
        ///     Returns a gift object that can be used for tests.
        /// </summary>
        protected static EGOgiftModel GetGift(string giftName)
        {
            return GetGift(DefaultEquipmentId, DefaultGiftId, giftName, DefaultGiftAttachRegion);
        }


        protected static EGOgiftModel GetGift(string giftName, EGOgiftAttachRegion attachRegion)
        {
            return GetGift(DefaultEquipmentId, DefaultGiftId, giftName, attachRegion);
        }


        protected static EGOgiftModel GetGift(long equipmentId, int giftId, string giftName, EGOgiftAttachRegion attachRegion)
        {
            var textData = new Dictionary<string, string> { { giftName, giftName } };
            InitializeTextData(textData);

            var equipmentNameDictionary = new Dictionary<string, string> { { "name", giftName } };
            var metaInfo = TestUnityExtensions.CreateEquipmentTypeInfo(localizeData: equipmentNameDictionary);
            metaInfo.id = giftId;
            metaInfo.attachPos = attachRegion.ToString();

            var gift = TestUnityExtensions.CreateEgoGiftModel(metaInfo);
            gift.instanceId = equipmentId;

            return gift;
        }

        /// <summary>
        ///     Adds the gift name to the engine's localized text data and initializes an observer to store any messages sent as a
        ///     notice.
        /// </summary>
        private static void InitializeTextData(Dictionary<string, string> textData)
        {
            TestUnityExtensions.CreateLocalizeTextDataModel(textData);
        }
    }
}
