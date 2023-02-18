// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.Test.Extensions;
using Moq;

#endregion

namespace LobotomyCorporationMods.Test.BadLuckProtectionForGiftsTests
{
    public class BadLuckProtectionForGiftsTests
    {
        protected const string GiftName = "DefaultGiftName";

        protected BadLuckProtectionForGiftsTests()
        {
        }

        [NotNull]
        protected static Mock<IAgentWorkTracker> CreateMockAgentWorkTracker()
        {
            var mockAgentTracker = new Mock<IAgentWorkTracker>();
            Harmony_Patch.Instance.LoadTracker(mockAgentTracker.Object);

            return mockAgentTracker;
        }

        [NotNull]
        protected static CreatureEquipmentMakeInfo GetCreatureEquipmentMakeInfo([NotNull] string giftName)
        {
            var equipTypeInfo = TestExtensions.CreateEquipmentTypeInfo();
            equipTypeInfo.type = EquipmentTypeInfo.EquipmentType.SPECIAL;
            equipTypeInfo.localizeData = new Dictionary<string, string> { { "name", giftName } };

            var creatureEquipmentMakeInfo = TestExtensions.CreateCreatureEquipmentMakeInfo();
            creatureEquipmentMakeInfo.equipTypeInfo = equipTypeInfo;

            LocalizeTextDataModel.instance.Init(new Dictionary<string, string> { { giftName, giftName } });

            return creatureEquipmentMakeInfo;
        }
    }
}
