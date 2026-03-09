// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CommandWindow;
using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Test.Parameters;
using UnityEngine;
using UnityEngine.UI;
using WorkerSprite;

namespace LobotomyCorporationMods.Test.Extensions
{
    internal static class UnityTestExtensions
    {
        private const BindingFlags BindingFlagsInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static;

        #region Unity Objects

        [NotNull]
        internal static AgentData CreateAgentData(AgentName? agentName = null,
            Appearance? appearance = null,
            StatBonus? statBonus = null)
        {
            agentName = agentName.EnsureNotNullWithMethod(() => CreateAgentName());
            appearance = appearance.EnsureNotNullWithMethod(() => CreateAppearance());
            statBonus = statBonus.EnsureNotNullWithMethod(CreateStatBonus);

            return new AgentData
            {
                agentName = agentName,
                appearance = appearance,
                statBonus = statBonus,
            };
        }

        [NotNull]
        internal static AgentInfoWindow CreateAgentInfoWindow(GameObject? customizingBlock = null,
            CustomizingWindow? customizingWindow = null,
            AgentInfoWindow.UIComponent? uiComponents = null)
        {
            customizingBlock = customizingBlock.EnsureNotNullWithMethod(CreateGameObject);
            customizingWindow = customizingWindow.EnsureNotNullWithMethod(() => CreateCustomizingWindow());
            uiComponents = uiComponents.EnsureNotNullWithMethod(CreateUiComponent);

            CreateUninitializedObject<AgentInfoWindow>(out AgentInfoWindow? agentInfoWindow);

            MemberInfo[] fields = GetUninitializedFieldsIncludingBaseType(agentInfoWindow.GetType());
            Dictionary<string, object> newValues = new()
            {
                {
                    "customizingBlock", customizingBlock
                },
                {
                    "customizingWindow", customizingWindow
                },
                {
                    "UIComponents", uiComponents
                },
            };

            agentInfoWindow = GetPopulatedUninitializedObject(agentInfoWindow, fields, newValues);
            newValues.Add("currentWindow", agentInfoWindow);

            return GetPopulatedUninitializedObject(agentInfoWindow, fields, newValues);
        }

        [NotNull]
        internal static AgentModel CreateAgentModel([CanBeNull] AgentModelCreationParameters? parameters = null)
        {
            parameters = parameters.EnsureNotNullWithMethod(() => new AgentModelCreationParameters());
            parameters.AgentName = parameters.AgentName.EnsureNotNullWithMethod(() => CreateAgentName());
            parameters.BufList = parameters.BufList.EnsureNotNullWithMethod(() => []);
            parameters.Equipment = parameters.Equipment.EnsureNotNullWithMethod(CreateUnitEquipSpace);
            parameters.PrimaryStat = parameters.PrimaryStat.EnsureNotNullWithMethod(CreateWorkerPrimaryStat);
            parameters.SpriteData = parameters.SpriteData.EnsureNotNullWithMethod(CreateWorkerSprite);
            parameters.StatBufList = parameters.StatBufList.EnsureNotNullWithMethod(() => []);

            CreateUninitializedObject<AgentModel>(out AgentModel? agentModel);
            MemberInfo[] fields = GetUninitializedFieldsIncludingBaseType(agentModel.GetType());
            Dictionary<string, object> newValues = new()
            {
                {
                    "_agentName", parameters.AgentName
                },
                {
                    "_bufList", parameters.BufList
                },
                {
                    "_equipment", parameters.Equipment
                },
                {
                    "instanceId", parameters.InstanceId
                },
                {
                    "name", parameters.Name
                },
                {
                    "primaryStat", parameters.PrimaryStat
                },
                {
                    "spriteData", parameters.SpriteData
                },
                {
                    "_statBufList", parameters.StatBufList
                },
            };

            return GetPopulatedUninitializedObject(agentModel, fields, newValues);
        }

        [NotNull]
        internal static AgentName CreateAgentName(AgentNameTypeInfo? metaInfo = null,
            Dictionary<string, string>? nameDic = null)
        {
            metaInfo = metaInfo.EnsureNotNullWithMethod(() => CreateAgentNameTypeInfo());
            nameDic = nameDic.EnsureNotNullWithMethod(() => []);

            CreateUninitializedObject<AgentName>(out AgentName? agentName);

            MemberInfo[] fields = GetUninitializedFieldsIncludingBaseType(agentName.GetType());
            Dictionary<string, object> newValues = new()
            {
                {
                    "metaInfo", metaInfo
                },
                {
                    "nameDic", nameDic
                },
            };

            return GetPopulatedUninitializedObject(agentName, fields, newValues);
        }

        [NotNull]
        private static AgentNameTypeInfo CreateAgentNameTypeInfo(Dictionary<string, string>? nameDic = null)
        {
            nameDic = nameDic.EnsureNotNullWithMethod(() => []);

            return new AgentNameTypeInfo
            {
                nameDic = nameDic,
            };
        }

        [NotNull]
        internal static AgentSlot CreateAgentSlot(List<MaskableGraphic>? coloredTargets = null,
            AgentModel? currentAgent = null,
            Image? workFilterFill = null,
            Text? workFilterText = null)
        {
            coloredTargets = coloredTargets.EnsureNotNullWithMethod(() => []);
            currentAgent = currentAgent.EnsureNotNullWithMethod(() => CreateAgentModel());
            workFilterFill = workFilterFill.EnsureNotNullWithMethod(CreateImage);
            workFilterText = workFilterText.EnsureNotNullWithMethod(CreateText);

            CreateUninitializedObject<AgentSlot>(out AgentSlot? agentSlot);

            MemberInfo[] fields = GetUninitializedFieldsIncludingBaseType(agentSlot.GetType());
            Dictionary<string, object> newValues = new()
            {
                {
                    "coloredTargets", coloredTargets
                },
                {
                    "_currentAgent", currentAgent
                },
                {
                    "WorkFilterFill", workFilterFill
                },
                {
                    "WorkFilterText", workFilterText
                },
            };

            return GetPopulatedUninitializedObject(agentSlot, fields, newValues);
        }

        [NotNull]
        private static Appearance CreateAppearance(WorkerSprite.WorkerSprite? spriteSet = null)
        {
            spriteSet = spriteSet.EnsureNotNullWithMethod(CreateWorkerSprite);

            return new Appearance
            {
                spriteSet = spriteSet,
            };
        }

        internal static AppearanceUI CreateAppearanceUi()
        {
            CreateUninitializedObject<AppearanceUI>(out AppearanceUI? appearanceUi);

            return appearanceUi;
        }

        [NotNull]
        internal static CommandWindow.CommandWindow CreateCommandWindow(UnitModel? currentTarget = null,
            CommandType currentWindowType = (CommandType)1,
            long selectedWork = 0L)
        {
            currentTarget = currentTarget.EnsureNotNullWithMethod(() => CreateUnitModel());

            CreateUninitializedObject<CommandWindow.CommandWindow>(out CommandWindow.CommandWindow? commandWindow);

            MemberInfo[] fields = GetUninitializedFieldsIncludingBaseType(commandWindow.GetType());
            Dictionary<string, object> newValues = new()
            {
                {
                    "_currentTarget", currentTarget
                },
                {
                    "_currentWindowType", currentWindowType
                },
                {
                    "_selectedWork", selectedWork
                },
            };

            commandWindow = GetPopulatedUninitializedObject(commandWindow, fields, newValues);
            newValues.Add("_currentWindow", commandWindow);

            return GetPopulatedUninitializedObject(commandWindow, fields, newValues);
        }

        [NotNull]
        internal static CreatureEquipmentMakeInfo CreateCreatureEquipmentMakeInfo(EquipmentTypeInfo? equipTypeInfo = null)
        {
            equipTypeInfo = equipTypeInfo.EnsureNotNullWithMethod(() => CreateEquipmentTypeInfo());

            return new CreatureEquipmentMakeInfo
            {
                equipTypeInfo = equipTypeInfo,
            };
        }

        [NotNull]
        internal static CreatureLayer CreateCreatureLayer(Dictionary<long, CreatureUnit>? creatureDic = null)
        {
            creatureDic = creatureDic.EnsureNotNullWithMethod(() => []);

            CreateUninitializedObject<CreatureLayer>(out CreatureLayer? creatureLayer);

            MemberInfo[] fields = GetUninitializedFieldsIncludingBaseType(creatureLayer.GetType());
            Dictionary<string, object> newValues = new()
            {
                {
                    "creatureDic", creatureDic
                },
            };

            creatureLayer = GetPopulatedUninitializedObject(creatureLayer, fields, newValues);
            newValues.Add("<currentLayer>k__BackingField", creatureLayer);

            creatureLayer = GetPopulatedUninitializedObject(creatureLayer, fields, newValues);

            return creatureLayer;
        }

        [NotNull]
        internal static CreatureModel CreateCreatureModel(AgentModel? agent = null,
            CreatureTypeInfo? metaInfo = null,
            CreatureObserveInfoModel? observeInfo = null,
            int qliphothCounter = 1,
            SkillTypeInfo? skillTypeInfo = null)
        {
            agent = agent.EnsureNotNullWithMethod(() => CreateAgentModel());
            metaInfo = metaInfo.EnsureNotNullWithMethod(() => CreateCreatureTypeInfo());
            observeInfo = observeInfo.EnsureNotNullWithMethod(() => CreateCreatureObserveInfoModel());
            skillTypeInfo = skillTypeInfo.EnsureNotNullWithMethod(CreateSkillTypeInfo);

            CreateUninitializedObject<CreatureModel>(out CreatureModel? creatureModel);

            MemberInfo[] fields = GetUninitializedFieldsIncludingBaseType(creatureModel.GetType());
            Dictionary<string, object> newValues = new()
            {
                {
                    "metaInfo", metaInfo
                },
                {
                    "observeInfo", observeInfo
                },
                {
                    "_qliphothCounter", qliphothCounter
                },
            };

            CreatureModel newCreatureModel = GetPopulatedUninitializedObject(creatureModel, fields, newValues);

            // Needed to avoid a circular reference from currentSkill
            UseSkill currentSkill = CreateUseSkill(agent, skillTypeInfo, newCreatureModel);
            newValues.Add("_currentSkill", currentSkill);

            return GetPopulatedUninitializedObject(newCreatureModel, fields, newValues);
        }

        [NotNull]
        private static CreatureObserveInfoModel CreateCreatureObserveInfoModel(CreatureTypeInfo? metaInfo = null,
            Dictionary<string, ObserveRegion>? observeRegions = null)
        {
            metaInfo = metaInfo.EnsureNotNullWithMethod(() => CreateCreatureTypeInfo());
            observeRegions = observeRegions.EnsureNotNullWithMethod(() => []);

            CreateUninitializedObject<CreatureObserveInfoModel>(out CreatureObserveInfoModel? creatureObserveInfoModel);

            MemberInfo[] fields = GetUninitializedFieldsIncludingBaseType(creatureObserveInfoModel.GetType());
            Dictionary<string, object> newValues = new()
            {
                {
                    "_metaInfo", metaInfo
                },
                {
                    "observeRegions", observeRegions
                },
            };

            return GetPopulatedUninitializedObject(creatureObserveInfoModel, fields, newValues);
        }

        [NotNull]
        internal static CreatureTypeInfo CreateCreatureTypeInfo(List<CreatureEquipmentMakeInfo>? equipMakeInfos = null)
        {
            equipMakeInfos = equipMakeInfos.EnsureNotNullWithMethod(() => []);

            return new CreatureTypeInfo
            {
                equipMakeInfos = equipMakeInfos,
            };
        }

        internal static CreatureUnit CreateCreatureUnit()
        {
            CreateUninitializedObject<CreatureUnit>(out CreatureUnit? creatureUnit);

            return creatureUnit;
        }

        [NotNull]
        internal static CustomizingWindow CreateCustomizingWindow(GameObject? appearanceBlock = null,
            AppearanceUI? appearanceUi = null,
            AgentModel? currentAgent = null,
            AgentData? currentData = null,
            CustomizingType currentWindowType = (CustomizingType)1)
        {
            appearanceBlock = appearanceBlock.EnsureNotNullWithMethod(CreateGameObject);
            appearanceUi = appearanceUi.EnsureNotNullWithMethod(CreateAppearanceUi);
            currentAgent = currentAgent.EnsureNotNullWithMethod(() => CreateAgentModel());
            currentData = currentData.EnsureNotNullWithMethod(() => CreateAgentData());

            CreateUninitializedObject<CustomizingWindow>(out CustomizingWindow? customizingWindow);

            MemberInfo[] fields = GetUninitializedFieldsIncludingBaseType(customizingWindow.GetType());
            Dictionary<string, object> newValues = new()
            {
                {
                    "appearanceBlock", appearanceBlock
                },
                {
                    "appearanceUI", appearanceUi
                },
                {
                    "_currentAgent", currentAgent
                },
                {
                    "CurrentData", currentData
                },
                {
                    "_currentWindowType", currentWindowType
                },
            };

            customizingWindow = GetPopulatedUninitializedObject(customizingWindow, fields, newValues);
            newValues.Add("_currentWindow", customizingWindow);

            return GetPopulatedUninitializedObject(customizingWindow, fields, newValues);
        }

        [NotNull]
        private static EGObonusInfo CreateEgoBonusInfo()
        {
            CreateUninitializedObject<EGObonusInfo>(out EGObonusInfo? egoBonusInfo);

            MemberInfo[] fields = GetUninitializedFieldsIncludingBaseType(egoBonusInfo.GetType());
            Dictionary<string, object> newValues = [];

            return GetPopulatedUninitializedObject(egoBonusInfo, fields, newValues);
        }

        [NotNull]
        internal static EGOgiftModel CreateEgoGiftModel(EquipmentTypeInfo? metaInfo = null,
            EquipmentScriptBase? script = null)
        {
            metaInfo = metaInfo.EnsureNotNullWithMethod(() => CreateEquipmentTypeInfo());
            script = script.EnsureNotNullWithMethod(() => CreateEquipmentScriptBase());

            return new EGOgiftModel
            {
                metaInfo = metaInfo,
                script = script,
            };
        }

        [NotNull]
        private static EquipmentModel CreateEquipmentModel(EquipmentTypeInfo? metaInfo = null)
        {
            metaInfo = metaInfo.EnsureNotNullWithMethod(() => CreateEquipmentTypeInfo());

            return new EquipmentModel
            {
                metaInfo = metaInfo,
            };
        }

        [NotNull]
        private static EquipmentScriptBase CreateEquipmentScriptBase(EquipmentModel? model = null)
        {
            model = model.EnsureNotNullWithMethod(() => CreateEquipmentModel());

            CreateUninitializedObject<EquipmentScriptBase>(out EquipmentScriptBase? equipmentScriptBase);

            MemberInfo[] fields = GetUninitializedFieldsIncludingBaseType(equipmentScriptBase.GetType());
            Dictionary<string, object> newValues = new()
            {
                {
                    "_model", model
                },
            };

            return GetPopulatedUninitializedObject(equipmentScriptBase, fields, newValues);
        }

        [NotNull]
        internal static EquipmentTypeInfo CreateEquipmentTypeInfo(EGObonusInfo? bonus = null,
            Dictionary<string, string>? localizeData = null)
        {
            bonus = bonus.EnsureNotNullWithMethod(CreateEgoBonusInfo);
            localizeData = localizeData.EnsureNotNullWithMethod(() => []);

            CreateUninitializedObject<EquipmentTypeInfo>(out EquipmentTypeInfo? equipmentTypeInfo);

            MemberInfo[] fields = GetUninitializedFieldsIncludingBaseType(equipmentTypeInfo.GetType());
            Dictionary<string, object> newValues = new()
            {
                {
                    "bonus", bonus
                },
                {
                    "localizeData", localizeData
                },
            };

            return GetPopulatedUninitializedObject(equipmentTypeInfo, fields, newValues);
        }

        internal static FairyBuf CreateFairyBuf()
        {
            CreateUninitializedObject<FairyBuf>(out FairyBuf? fairyBuf);

            return fairyBuf;
        }

        [NotNull]
        internal static GameManager CreateGameManager()
        {
            CreateUninitializedObject<GameManager>(out GameManager? gameManager);

            MemberInfo[] fields = GetUninitializedFieldsIncludingBaseType(gameManager.GetType());
            Dictionary<string, object> newValues = [];
            gameManager = GetPopulatedUninitializedObject(gameManager, fields, newValues);
            newValues.Add("_currentGameManager", gameManager);

            return GetPopulatedUninitializedObject(gameManager, fields, newValues);
        }


        private static GameObject CreateGameObject()
        {
            CreateUninitializedObject<GameObject>(out GameObject? gameObject);

            return gameObject;
        }

        private static Image CreateImage()
        {
            CreateUninitializedObject<Image>(out Image? image);

            return image;
        }

        internal static LittleWitchBuf CreateLittleWitchBuf()
        {
            CreateUninitializedObject<LittleWitchBuf>(out LittleWitchBuf? littleWitchBuf);

            return littleWitchBuf;
        }

        [NotNull]
        internal static LocalizeTextDataModel CreateLocalizeTextDataModel(Dictionary<string, string>? list = null)
        {
            list = list.EnsureNotNullWithMethod(() => []);

            CreateUninitializedObject<LocalizeTextDataModel>(out LocalizeTextDataModel? localizeTextDataModel);

            MemberInfo[] fields = GetUninitializedFieldsIncludingBaseType(localizeTextDataModel.GetType());
            Dictionary<string, object> newValues = new()
            {
                {
                    "_list", list
                },
            };

            localizeTextDataModel = GetPopulatedUninitializedObject(localizeTextDataModel, fields, newValues);
            newValues.Add("_instance", localizeTextDataModel);

            return GetPopulatedUninitializedObject(localizeTextDataModel, fields, newValues);
        }

        [NotNull]
        internal static ManagementSlot CreateManagementSlot()
        {
            CreateUninitializedObject<ManagementSlot>(out ManagementSlot? managementSlot);

            MemberInfo[] fields = GetUninitializedFieldsIncludingBaseType(managementSlot.GetType());
            Dictionary<string, object> newValues = [];

            return GetPopulatedUninitializedObject(managementSlot, fields, newValues);
        }

        [NotNull]
        private static SkillTypeInfo CreateSkillTypeInfo()
        {
            return new SkillTypeInfo();
        }

        [NotNull]
        internal static SkillTypeList CreateSkillTypeList(SkillTypeInfo[]? skillTypeInfoList = null)
        {
            skillTypeInfoList = skillTypeInfoList.EnsureNotNullWithMethod(Array.Empty<SkillTypeInfo>);

            SkillTypeList skillType = SkillTypeList.instance;
            skillType.Init(skillTypeInfoList);

            return skillType;
        }

        [NotNull]
        internal static Sprite CreateSprite(string name = "")
        {
            CreateUninitializedObject<Sprite>(out Sprite? sprite);

            MemberInfo[] fields = GetUninitializedFieldsIncludingBaseType(sprite.GetType());
            Dictionary<string, object> newValues = new()
            {
                {
                    "name", name
                },
            };

            return GetPopulatedUninitializedObject(sprite, fields, newValues);
        }

        [NotNull]
        private static StatBonus CreateStatBonus()
        {
            return new StatBonus();
        }

        private static Text CreateText()
        {
            CreateUninitializedObject<Text>(out Text? text);

            return text;
        }

        [NotNull]
        private static AgentInfoWindow.UIComponent CreateUiComponent()
        {
            return new AgentInfoWindow.UIComponent();
        }

        [NotNull]
        internal static UnitEquipSpace CreateUnitEquipSpace()
        {
            return new UnitEquipSpace();
        }

        [NotNull]
        internal static UnitModel CreateUnitModel(List<UnitBuf>? bufList = null,
            List<UnitStatBuf>? statBufList = null)
        {
            bufList = bufList.EnsureNotNullWithMethod(() => []);
            statBufList = statBufList.EnsureNotNullWithMethod(() => []);

            CreateUninitializedObject<UnitModel>(out UnitModel? unitModel);

            MemberInfo[] fields = GetUninitializedFieldsIncludingBaseType(unitModel.GetType());
            Dictionary<string, object> newValues = new()
            {
                {
                    "_bufList", bufList
                },
                {
                    "_statBufList", statBufList
                },
            };

            return GetPopulatedUninitializedObject(unitModel, fields, newValues);
        }

        [NotNull]
        internal static UnitStatBuf CreateUnitStatBuf(WorkerPrimaryStatBonus? primaryStat = null)
        {
            primaryStat = primaryStat.EnsureNotNullWithMethod(CreateWorkerPrimaryStatBonus);

            CreateUninitializedObject<UnitStatBuf>(out UnitStatBuf? unitStatBuf);

            MemberInfo[] fields = GetUninitializedFieldsIncludingBaseType(unitStatBuf.GetType());
            Dictionary<string, object> newValues = new()
            {
                {
                    "primaryStat", primaryStat
                },
            };

            return GetPopulatedUninitializedObject(unitStatBuf, fields, newValues);
        }

        [NotNull]
        internal static UseSkill CreateUseSkill(AgentModel? agent = null,
            SkillTypeInfo? skillTypeInfo = null,
            CreatureModel? targetCreature = null)
        {
            agent = agent.EnsureNotNullWithMethod(() => CreateAgentModel());
            skillTypeInfo = skillTypeInfo.EnsureNotNullWithMethod(CreateSkillTypeInfo);
            targetCreature = targetCreature.EnsureNotNullWithMethod(() => CreateCreatureModel());

            // Needed to avoid circular reference
            if (targetCreature.currentSkill.IsNotNull())
            {
                return targetCreature.currentSkill;
            }

            UseSkill useSkill = new()
            {
                agent = agent,
                skillTypeInfo = skillTypeInfo,
                targetCreature = targetCreature,
            };

            targetCreature.currentSkill = useSkill;

            return useSkill;
        }

        [NotNull]
        private static WorkerBasicSpriteController CreateWorkerBasicSpriteController()
        {
            return new WorkerBasicSpriteController();
        }

        [NotNull]
        internal static WorkerPrimaryStat CreateWorkerPrimaryStat()
        {
            return new WorkerPrimaryStat();
        }

        [NotNull]
        internal static WorkerPrimaryStatBonus CreateWorkerPrimaryStatBonus()
        {
            return new WorkerPrimaryStatBonus();
        }

        [NotNull]
        internal static WorkerSprite.WorkerSprite CreateWorkerSprite()
        {
            return new WorkerSprite.WorkerSprite();
        }

        [NotNull]
        internal static WorkerSpriteManager CreateWorkerSpriteManager(WorkerBasicSpriteController? basicData = null)
        {
            basicData = basicData.EnsureNotNullWithMethod(CreateWorkerBasicSpriteController);

            CreateUninitializedObject<WorkerSpriteManager>(out WorkerSpriteManager? workerSpriteManager);

            MemberInfo[] fields = GetUninitializedFieldsIncludingBaseType(workerSpriteManager.GetType());
            Dictionary<string, object> newValues = new()
            {
                {
                    "basicData", basicData
                },
            };

            workerSpriteManager = GetPopulatedUninitializedObject(workerSpriteManager, fields, newValues);
            newValues.Add("_instance", workerSpriteManager);

            return GetPopulatedUninitializedObject(workerSpriteManager, fields, newValues);
        }

        internal static YggdrasilBlessBuf CreateYggdrasilBlessBuf()
        {
            CreateUninitializedObject<YggdrasilBlessBuf>(out YggdrasilBlessBuf? yggdrasilBlessBuf);

            return yggdrasilBlessBuf;
        }

        #endregion

        #region Uninitialized Object Functions

        /// <summary>
        ///     Create an uninitialized object without calling a constructor. Needed because some of the classes we need to mock either don't have a public constructor or cause a Unity
        ///     exception.
        /// </summary>
        private static void CreateUninitializedObject<TObject>(out TObject obj)
        {
            obj = (TObject)RuntimeHelpers.GetUninitializedObject(typeof(TObject));
        }

        /// <summary>Gets the fields of the specified type, including those of its base types, for uninitialized object.</summary>
        [NotNull]
        private static MemberInfo[] GetUninitializedFieldsIncludingBaseType(Type type)
        {
            List<MemberInfo> fields = [];

            while (type.IsNotNull() && type != typeof(object))
            {
                FieldInfo[] typeFields = type.GetFields(BindingFlagsInstance);
                fields.AddRange(typeFields.GetValidFields());

                Type? baseType = type.BaseType;
                if (baseType.IsNotNull())
                {
                    type = baseType;
                }
            }

            return [.. fields];
        }

        /// <summary>Returns the fields which are valid and can be used to populate an object.</summary>
        [NotNull]
        private static List<MemberInfo> GetValidFields([NotNull] this IEnumerable<FieldInfo> typeFields)
        {
            return [.. typeFields.Where(FieldIsValidAndCanPopulateObject).Cast<MemberInfo>()];
        }

        private static bool FieldIsValidAndCanPopulateObject(FieldInfo typeField)
        {
            try
            {
                return FieldHasValidHandleAndIsNotInitOnly(typeField);
            }
            catch (NotSupportedException)
            {
                // Some fields will give an exception when checked, so we cannot consider those fields as valid.
                return false;
            }
        }

        private static bool FieldHasValidHandleAndIsNotInitOnly([NotNull] FieldInfo typeField)
        {
            bool hasValidHandle = typeField.FieldHandle.Value != IntPtr.Zero;
            bool isNotInitOnly = !typeField.IsInitOnly;

            return hasValidHandle && isNotInitOnly;
        }

        /// <summary>Populate the fields of an uninitialized object with a provided list of objects.</summary>
        [NotNull]
        private static TObject GetPopulatedUninitializedObject<TObject>([NotNull] TObject obj,
            [NotNull] MemberInfo[] fields,
            Dictionary<string, object> newValues) where TObject : class
        {
            CreateUninitializedObject(out TObject? newObj);
            object?[] values = [.. fields.Select(m => ((FieldInfo)m).GetValue(obj))];

            for (int i = 0; i < fields.Length; i++)
            {
                if (newValues.TryGetValue(fields[i].Name, out object? value))
                {
                    values[i] = value;
                }
            }

            for (int i = 0; i < fields.Length; i++)
            {
                ((FieldInfo)fields[i]).SetValue(newObj, values[i]);
            }

            return newObj;
        }

        #endregion
    }
}
