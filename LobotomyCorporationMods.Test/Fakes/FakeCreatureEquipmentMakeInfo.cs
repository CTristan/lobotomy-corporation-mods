using System.Collections.Generic;
using JetBrains.Annotations;

namespace LobotomyCorporationMods.Test.Fakes
{
    internal sealed class FakeCreatureEquipmentMakeInfo : CreatureEquipmentMakeInfo
    {
        public FakeCreatureEquipmentMakeInfo([NotNull] string giftName)
        {
            equipTypeInfo = new EquipmentTypeInfo
            {
                localizeData = new Dictionary<string, string> { { "name", giftName } },
                type = EquipmentTypeInfo.EquipmentType.SPECIAL
            };
            LocalizeTextDataModel.instance?.Init(new Dictionary<string, string> { { giftName, giftName } });
        }
    }
}
