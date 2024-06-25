// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using JetBrains.Annotations;
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

        [NotNull]
        protected static CreatureEquipmentMakeInfo GetCreatureEquipmentMakeInfo(string giftName)
        {
            var equipTypeInfo = UnityTestExtensions.CreateEquipmentTypeInfo();
            equipTypeInfo.type = EquipmentTypeInfo.EquipmentType.SPECIAL;
            equipTypeInfo.localizeData = new Dictionary<string, string> { { "name", giftName } };

            var creatureEquipmentMakeInfo = UnityTestExtensions.CreateCreatureEquipmentMakeInfo();
            creatureEquipmentMakeInfo.equipTypeInfo = equipTypeInfo;

            LocalizeTextDataModel.instance.Init(new Dictionary<string, string> { { giftName, giftName } });

            return creatureEquipmentMakeInfo;
        }
    }
}
