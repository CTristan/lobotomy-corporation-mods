// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LobotomyCorporationMods.Test.Extensions;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.BadLuckProtectionForGiftsTests
{
    public class BadLuckProtectionForGiftsModTests : IDisposable
    {
        protected const string GiftName = "DefaultGiftName";

        protected BadLuckProtectionForGiftsModTests() { }

        [NotNull]
        protected static CreatureEquipmentMakeInfo GetCreatureEquipmentMakeInfo(
            [NotNull] string giftName
        )
        {
            var equipTypeInfo = UnityTestExtensions.CreateEquipmentTypeInfo();
            equipTypeInfo.type = EquipmentTypeInfo.EquipmentType.SPECIAL;
            equipTypeInfo.localizeData = new Dictionary<string, string> { { "name", giftName } };

            var creatureEquipmentMakeInfo = UnityTestExtensions.CreateCreatureEquipmentMakeInfo();
            creatureEquipmentMakeInfo.equipTypeInfo = equipTypeInfo;

            LocalizeTextDataModel.instance.Init(
                new Dictionary<string, string> { { giftName, giftName } }
            );

            return creatureEquipmentMakeInfo;
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
