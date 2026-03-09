// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using JetBrains.Annotations;
using LobotomyCorporationMods.Test.Extensions;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.BadLuckProtectionForGiftsTests
{
    internal class BadLuckProtectionForGiftsModTests
    {
        protected const string GiftName = "DefaultGiftName";

        protected BadLuckProtectionForGiftsModTests()
        {
        }

        [NotNull]
        protected static CreatureEquipmentMakeInfo GetCreatureEquipmentMakeInfo([NotNull] string giftName)
        {
            EquipmentTypeInfo equipTypeInfo = UnityTestExtensions.CreateEquipmentTypeInfo();
            equipTypeInfo.type = EquipmentTypeInfo.EquipmentType.SPECIAL;
            equipTypeInfo.localizeData = new Dictionary<string, string>
            {
                {
                    "name", giftName
                },
            };

            CreatureEquipmentMakeInfo creatureEquipmentMakeInfo = UnityTestExtensions.CreateCreatureEquipmentMakeInfo();
            creatureEquipmentMakeInfo.equipTypeInfo = equipTypeInfo;

            LocalizeTextDataModel.instance.Init(new Dictionary<string, string>
            {
                {
                    giftName, giftName
                },
            });

            return creatureEquipmentMakeInfo;
        }
    }
}
