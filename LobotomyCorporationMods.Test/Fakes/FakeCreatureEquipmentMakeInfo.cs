using System.Collections.Generic;

namespace LobotomyCorporationMods.Test.Fakes
{
    internal sealed class FakeCreatureEquipmentMakeInfo : CreatureEquipmentMakeInfo
    {
        public FakeCreatureEquipmentMakeInfo(string giftName)
        {
            equipTypeInfo = new EquipmentTypeInfo
            {
                localizeData = new Dictionary<string, string> {{"name", giftName}},
                type = EquipmentTypeInfo.EquipmentType.SPECIAL
            };
            LocalizeTextDataModel.instance.Init(new Dictionary<string, string> {{giftName, giftName}});
        }
    }
}
