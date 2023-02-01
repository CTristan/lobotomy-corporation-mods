// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using JetBrains.Annotations;
using UnityEngine;
using WorkerSprite;

namespace LobotomyCorporationMods.Test
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal static class TestData
    {
        internal const long DefaultAgentId = 1L;
        internal const string DefaultAgentNameString = "DefaultAgentName";
        internal const CustomizingType DefaultCustomizingType = CustomizingType.GENERATE;
        internal const string DefaultGiftName = "DefaultGiftName";
        internal const string DefaultSpriteName = "DefaultSpriteName";
        internal const int None = 0;

        [NotNull]
        internal static AgentData DefaultAgentData => TestExtensions.CreateAgentData(DefaultAgentName, DefaultAppearance);

        [NotNull]
        internal static AgentModel DefaultAgentModel => TestExtensions.CreateAgentModel(DefaultAgentName, DefaultUnitEquipSpace, DefaultAgentId, DefaultAgentNameString, DefaultWorkerSprite);

        [NotNull]
        internal static AgentName DefaultAgentName => TestExtensions.CreateAgentName(DefaultGlobalGameManager, DefaultAgentNameTypeInfo, new Dictionary<string, string>());

        [NotNull]
        internal static AgentNameTypeInfo DefaultAgentNameTypeInfo => TestExtensions.CreateAgentNameTypeInfo(new Dictionary<string, string>());

        [NotNull]
        internal static Appearance DefaultAppearance => TestExtensions.CreateAppearance(DefaultWorkerSpriteManager, DefaultWorkerSprite);

        [NotNull]
        internal static AppearanceUI DefaultAppearanceUI => TestExtensions.CreateAppearanceUI();

        [NotNull]
        internal static CreatureEquipmentMakeInfo DefaultCreatureEquipmentMakeInfo => TestExtensions.CreateCreatureEquipmentMakeInfo(DefaultEquipmentTypeInfo);

        [NotNull]
        internal static CreatureModel DefaultCreatureModel => TestExtensions.CreateCreatureModel(DefaultAgentModel, DefaultSkillTypeInfo, DefaultCreatureTypeInfo);

        [NotNull]
        internal static CreatureTypeInfo DefaultCreatureTypeInfo => TestExtensions.CreateCreatureTypeInfo(new List<CreatureEquipmentMakeInfo>());

        [NotNull]
        internal static CustomizingWindow DefaultCustomizingWindow => TestExtensions.CreateCustomizingWindow(DefaultAppearanceUI, DefaultAgentModel, DefaultAgentData, DefaultCustomizingType);

        [NotNull]
        internal static EGOgiftModel DefaultEgoGiftModel => TestExtensions.CreateEgoGiftModel(DefaultEquipmentTypeInfo);

        [NotNull]
        internal static EquipmentTypeInfo DefaultEquipmentTypeInfo => TestExtensions.CreateEquipmentTypeInfo();

        [NotNull]
        internal static GlobalGameManager DefaultGlobalGameManager => TestExtensions.CreateGlobalGameManager();

        [NotNull]
        internal static SkillTypeInfo DefaultSkillTypeInfo => TestExtensions.CreateSkillTypeInfo();

        [NotNull]
        internal static Sprite DefaultSprite => TestExtensions.CreateSprite(DefaultSpriteName);

        [NotNull]
        internal static UnitEquipSpace DefaultUnitEquipSpace => TestExtensions.CreateUnitEquipSpace();

        [NotNull]
        internal static UseSkill DefaultUseSkill => TestExtensions.CreateUseSkill(DefaultAgentModel, DefaultSkillTypeInfo, DefaultCreatureModel);

        [NotNull]
        internal static WorkerBasicSpriteController DefaultWorkerBasicSpriteController => TestExtensions.CreateWorkerBasicSpriteController();

        [NotNull]
        internal static WorkerSprite.WorkerSprite DefaultWorkerSprite => TestExtensions.CreateWorkerSprite();

        [NotNull]
        internal static WorkerSpriteManager DefaultWorkerSpriteManager => TestExtensions.CreateWorkerSpriteManager(DefaultWorkerBasicSpriteController);
    }
}
