// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommandWindow;
using Customizing;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using WorkerSprite;

namespace LobotomyCorporationMods.Test
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal static class TestData
    {
        internal const long DefaultAgentId = 1L;
        internal const string DefaultAgentNameString = "DefaultAgentName";
        internal const AgentState DefaultAgentState = (AgentState)1;
        internal const CommandType DefaultCommandType = (CommandType)1;
        internal const CustomizingType DefaultCustomizingType = (CustomizingType)1;
        internal const string DefaultGiftName = "DefaultGiftName";
        internal const int DefaultQliphothCounter = 0;
        internal const string DefaultSpriteName = "DefaultSpriteName";
        internal const int None = 0;

        [NotNull] internal static readonly List<UnitBuf> DefaultBuffList = new List<UnitBuf>();
        [NotNull] internal static readonly Dictionary<long, CreatureUnit> DefaultCreatureDictionary = new Dictionary<long, CreatureUnit>();
        [NotNull] internal static readonly Dictionary<string, ObserveRegion> DefaultObserveRegions = new Dictionary<string, ObserveRegion>();
        [NotNull] internal static readonly SkillTypeInfo[] DefaultSkillTypeInfoArray = { new SkillTypeInfo() };
        [NotNull] internal static readonly List<UnitStatBuf> DefaultStatBuffList = new List<UnitStatBuf>();

        [NotNull]
        internal static AgentData DefaultAgentData => TestExtensions.CreateAgentData(DefaultAgentName, DefaultAppearance);

        [NotNull]
        internal static AgentModel DefaultAgentModel => TestExtensions.CreateAgentModel(DefaultAgentName, DefaultBuffList, DefaultUnitEquipSpace, DefaultAgentId, DefaultAgentNameString,
            DefaultWorkerPrimaryStat, DefaultWorkerSprite, DefaultStatBuffList);

        [NotNull]
        internal static AgentName DefaultAgentName => TestExtensions.CreateAgentName(DefaultGlobalGameManager, DefaultAgentNameTypeInfo, new Dictionary<string, string>());

        [NotNull]
        internal static AgentNameTypeInfo DefaultAgentNameTypeInfo => TestExtensions.CreateAgentNameTypeInfo(new Dictionary<string, string>());

        [NotNull]
        internal static AgentSlot DefaultAgentSlot => TestExtensions.CreateAgentSlot(DefaultAgentModel, DefaultImage, DefaultText);

        [NotNull]
        internal static Appearance DefaultAppearance => TestExtensions.CreateAppearance(DefaultWorkerSpriteManager, DefaultWorkerSprite);

        [NotNull]
        internal static AppearanceUI DefaultAppearanceUI => TestExtensions.CreateAppearanceUI();

        [NotNull]
        internal static CommandWindow.CommandWindow DefaultCommandWindow => TestExtensions.CreateCommandWindow(DefaultUnitModel, DefaultCommandType, 1L, DefaultSkillTypeList);

        [NotNull]
        internal static CreatureEquipmentMakeInfo DefaultCreatureEquipmentMakeInfo => TestExtensions.CreateCreatureEquipmentMakeInfo(DefaultEquipmentTypeInfo);

        [NotNull]
        internal static CreatureLayer DefaultCreatureLayer => TestExtensions.CreateCreatureLayer(DefaultCreatureDictionary);

        [NotNull]
        internal static CreatureModel DefaultCreatureModel => TestExtensions.CreateCreatureModel(DefaultAgentModel, DefaultCreatureLayer, DefaultCreatureTypeInfo, DefaultCreatureObserveInfoModel,
            DefaultQliphothCounter, DefaultSkillTypeInfo);

        [NotNull]
        internal static CreatureObserveInfoModel DefaultCreatureObserveInfoModel => TestExtensions.CreateCreatureObserveInfoModel(DefaultCreatureTypeInfo, DefaultObserveRegions);

        [NotNull]
        internal static CreatureTypeInfo DefaultCreatureTypeInfo => TestExtensions.CreateCreatureTypeInfo(new List<CreatureEquipmentMakeInfo>());

        [NotNull]
        internal static CustomizingWindow DefaultCustomizingWindow => TestExtensions.CreateCustomizingWindow(DefaultAppearanceUI, DefaultAgentModel, DefaultAgentData, DefaultCustomizingType);

        [NotNull]
        internal static EGOgiftModel DefaultEgoGiftModel => TestExtensions.CreateEgoGiftModel(DefaultEquipmentTypeInfo, DefaultEquipmentScriptBase);

        [NotNull]
        internal static EquipmentModel DefaultEquipmentModel => TestExtensions.CreateEquipmentModel(DefaultEquipmentTypeInfo);

        [NotNull]
        internal static EquipmentScriptBase DefaultEquipmentScriptBase => TestExtensions.CreateEquipmentScriptBase(DefaultEquipmentModel);

        [NotNull]
        internal static EquipmentTypeInfo DefaultEquipmentTypeInfo => TestExtensions.CreateEquipmentTypeInfo();

        [NotNull]
        internal static GlobalGameManager DefaultGlobalGameManager => TestExtensions.CreateGlobalGameManager();

        [NotNull]
        internal static Image DefaultImage => TestExtensions.CreateImage();

        [NotNull]
        internal static SkillTypeInfo DefaultSkillTypeInfo => TestExtensions.CreateSkillTypeInfo();

        [NotNull]
        internal static SkillTypeList DefaultSkillTypeList => TestExtensions.CreateSkillTypeList(DefaultSkillTypeInfoArray);

        [NotNull]
        internal static Sprite DefaultSprite => TestExtensions.CreateSprite(DefaultSpriteName);

        [NotNull]
        internal static Text DefaultText => TestExtensions.CreateText();

        [NotNull]
        internal static UnitEquipSpace DefaultUnitEquipSpace => TestExtensions.CreateUnitEquipSpace();

        [NotNull]
        internal static UnitModel DefaultUnitModel => TestExtensions.CreateUnitModel(DefaultBuffList, DefaultStatBuffList);

        [NotNull]
        internal static UseSkill DefaultUseSkill => TestExtensions.CreateUseSkill(DefaultAgentModel, DefaultSkillTypeInfo, DefaultCreatureModel);

        [NotNull]
        internal static WorkerBasicSpriteController DefaultWorkerBasicSpriteController => TestExtensions.CreateWorkerBasicSpriteController();

        [NotNull]
        internal static WorkerPrimaryStat DefaultWorkerPrimaryStat => TestExtensions.CreateWorkerPrimaryStat();

        [NotNull]
        internal static WorkerSprite.WorkerSprite DefaultWorkerSprite => TestExtensions.CreateWorkerSprite();

        [NotNull]
        internal static WorkerSpriteManager DefaultWorkerSpriteManager => TestExtensions.CreateWorkerSpriteManager(DefaultWorkerBasicSpriteController);
    }
}
