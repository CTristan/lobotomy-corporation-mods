// SPDX-License-Identifier: MIT

#region

using AutoFixture;
using LobotomyCorporationMods.Test.Extensions;

#endregion

namespace LobotomyCorporationMods.Test.Customizations
{
    public sealed class UnityCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() => UnityTestExtensions.CreateAgentData());
            fixture.Register(() => UnityTestExtensions.CreateAgentInfoWindow());
            fixture.Register(() => UnityTestExtensions.CreateAgentModel());
            fixture.Register(() => UnityTestExtensions.CreateAgentName());
            fixture.Register(() => UnityTestExtensions.CreateAgentSlot());
            fixture.Register(UnityTestExtensions.CreateAppearanceUi);
            fixture.Register(() => UnityTestExtensions.CreateCommandWindow());
            fixture.Register(() => UnityTestExtensions.CreateCreatureEquipmentMakeInfo());
            fixture.Register(() => UnityTestExtensions.CreateCreatureLayer());
            fixture.Register(() => UnityTestExtensions.CreateCreatureModel());
            fixture.Register(() => UnityTestExtensions.CreateCreatureTypeInfo());
            fixture.Register(UnityTestExtensions.CreateCreatureUnit);
            fixture.Register(() => UnityTestExtensions.CreateCustomizingWindow());
            fixture.Register(() => UnityTestExtensions.CreateEgoGiftModel());
            fixture.Register(() => UnityTestExtensions.CreateEquipmentTypeInfo());
            fixture.Register(UnityTestExtensions.CreateFairyBuf);
            fixture.Register(UnityTestExtensions.CreateGameManager);
            fixture.Register(UnityTestExtensions.CreateLittleWitchBuf);
            fixture.Register(() => UnityTestExtensions.CreateLocalizeTextDataModel());
            fixture.Register(UnityTestExtensions.CreateManagementSlot);
            fixture.Register(() => UnityTestExtensions.CreateSkillTypeList());
            fixture.Register(() => UnityTestExtensions.CreateSprite());
            fixture.Register(UnityTestExtensions.CreateUnitEquipSpace);
            fixture.Register(() => UnityTestExtensions.CreateUnitModel());
            fixture.Register(() => UnityTestExtensions.CreateUnitStatBuf());
            fixture.Register(() => UnityTestExtensions.CreateUseSkill());
            fixture.Register(UnityTestExtensions.CreateWorkerPrimaryStat);
            fixture.Register(UnityTestExtensions.CreateWorkerPrimaryStatBonus);
            fixture.Register(UnityTestExtensions.CreateWorkerSprite);
            fixture.Register(() => UnityTestExtensions.CreateWorkerSpriteManager());
            fixture.Register(UnityTestExtensions.CreateYggdrasilBlessBuf);
        }
    }
}
