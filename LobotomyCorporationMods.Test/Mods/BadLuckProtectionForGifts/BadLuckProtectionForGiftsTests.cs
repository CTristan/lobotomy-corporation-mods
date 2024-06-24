// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.Test.Extensions;

#endregion

namespace LobotomyCorporationMods.Test.Mods.BadLuckProtectionForGifts
{
    public class BadLuckProtectionForGiftsTests
    {
        protected const string GiftName = "DefaultGiftName";

        protected BadLuckProtectionForGiftsTests()
        {
        }

        protected static CreatureEquipmentMakeInfo GetCreatureEquipmentMakeInfo(string giftName)
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
