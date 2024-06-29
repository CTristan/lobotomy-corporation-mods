﻿// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using CommandWindow;
using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
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
        internal static AgentData CreateAgentData(AgentName agentName = null,
            Appearance appearance = null,
            StatBonus statBonus = null)
        {
            agentName = agentName ?? CreateAgentName();
            appearance = appearance ?? CreateAppearance();
            statBonus = statBonus ?? CreateStatBonus();

            return new AgentData
            {
                agentName = agentName,
                appearance = appearance,
                statBonus = statBonus,
            };
        }

        [NotNull]
        internal static AgentInfoWindow CreateAgentInfoWindow(GameObject customizingBlock = null,
            CustomizingWindow customizingWindow = null,
            AgentInfoWindow.UIComponent uiComponents = null)
        {
            customizingBlock = EnsureNotNullWithDefault(customizingBlock, CreateGameObject);
            customizingWindow = EnsureNotNullWithDefault(customizingWindow, () => CreateCustomizingWindow());
            uiComponents = EnsureNotNullWithDefault(uiComponents, CreateUIComponent);

            CreateUninitializedObject<AgentInfoWindow>(out var agentInfoWindow);

            var fields = GetUninitializedFieldsIncludingBaseType(agentInfoWindow.GetType());
            var newValues = new Dictionary<string, object>
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
        internal static AgentModel CreateAgentModel(AgentName agentName = null,
            List<UnitBuf> bufList = null,
            UnitEquipSpace equipment = null,
            long instanceId = 1L,
            string name = "",
            WorkerPrimaryStat primaryStat = null,
            WorkerSprite.WorkerSprite spriteData = null,
            List<UnitStatBuf> statBufList = null)
        {
            agentName = EnsureNotNullWithDefault(agentName, () => CreateAgentName());
            bufList = EnsureNotNullWithDefault(bufList, () => new List<UnitBuf>());
            equipment = EnsureNotNullWithDefault(equipment, CreateUnitEquipSpace);
            primaryStat = EnsureNotNullWithDefault(primaryStat, CreateWorkerPrimaryStat);
            spriteData = EnsureNotNullWithDefault(spriteData, CreateWorkerSprite);
            statBufList = EnsureNotNullWithDefault(statBufList, () => new List<UnitStatBuf>());

            CreateUninitializedObject<AgentModel>(out var agentModel);

            var fields = GetUninitializedFieldsIncludingBaseType(agentModel.GetType());
            var newValues = new Dictionary<string, object>
            {
                {
                    "_agentName", agentName
                },
                {
                    "_bufList", bufList
                },
                {
                    "_equipment", equipment
                },
                {
                    "instanceId", instanceId
                },
                {
                    "name", name
                },
                {
                    "primaryStat", primaryStat
                },
                {
                    "spriteData", spriteData
                },
                {
                    "_statBufList", statBufList
                },
            };

            return GetPopulatedUninitializedObject(agentModel, fields, newValues);
        }

        /// <summary>Ensure that a value is not null, and if it.IsNull(), return the result of the specified default method.</summary>
        /// <remarks>
        ///     This method is used to ensure that a value is not null. If the value.IsNull(), the specified default method will be called to provide a default value. The default method
        ///     should return a value of the same type as the value being checked.
        /// </remarks>
        /// <typeparam name="T">The type of the value being checked.</typeparam>
        /// <param name="value">The value to check for null.</param>
        /// <param name="defaultMethod">The method to call to provide a default value if the value.IsNull().</param>
        /// <returns>The original value if it is not null, or the result of the default method if the value.IsNull().</returns>
        private static T EnsureNotNullWithDefault<T>([CanBeNull] T value,
            Func<T> defaultMethod) where T : class
        {
            return value ?? defaultMethod();
        }

        [NotNull]
        internal static AgentName CreateAgentName(AgentNameTypeInfo metaInfo = null,
            Dictionary<string, string> nameDic = null)
        {
            metaInfo = metaInfo ?? CreateAgentNameTypeInfo();
            nameDic = nameDic ?? new Dictionary<string, string>();

            CreateUninitializedObject<AgentName>(out var agentName);

            var fields = GetUninitializedFieldsIncludingBaseType(agentName.GetType());
            var newValues = new Dictionary<string, object>
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
        private static AgentNameTypeInfo CreateAgentNameTypeInfo(Dictionary<string, string> nameDic = null)
        {
            nameDic = nameDic ?? new Dictionary<string, string>();

            return new AgentNameTypeInfo
            {
                nameDic = nameDic,
            };
        }

        [NotNull]
        internal static AgentSlot CreateAgentSlot(List<MaskableGraphic> coloredTargets = null,
            AgentModel currentAgent = null,
            Image workFilterFill = null,
            Text workFilterText = null)
        {
            coloredTargets = EnsureNotNullWithDefault(coloredTargets, () => new List<MaskableGraphic>());
            currentAgent = EnsureNotNullWithDefault(currentAgent, () => CreateAgentModel());
            workFilterFill = EnsureNotNullWithDefault(workFilterFill, CreateImage);
            workFilterText = EnsureNotNullWithDefault(workFilterText, CreateText);

            CreateUninitializedObject<AgentSlot>(out var agentSlot);

            var fields = GetUninitializedFieldsIncludingBaseType(agentSlot.GetType());
            var newValues = new Dictionary<string, object>
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
        private static Appearance CreateAppearance(WorkerSprite.WorkerSprite spriteSet = null)
        {
            spriteSet = spriteSet ?? CreateWorkerSprite();

            return new Appearance
            {
                spriteSet = spriteSet,
            };
        }

        internal static AppearanceUI CreateAppearanceUI()
        {
            CreateUninitializedObject<AppearanceUI>(out var appearanceUi);

            return appearanceUi;
        }

        [NotNull]
        internal static CommandWindow.CommandWindow CreateCommandWindow(UnitModel currentTarget = null,
            CommandType currentWindowType = (CommandType)1,
            long selectedWork = 0L)
        {
            currentTarget = currentTarget ?? CreateUnitModel();

            CreateUninitializedObject<CommandWindow.CommandWindow>(out var commandWindow);

            var fields = GetUninitializedFieldsIncludingBaseType(commandWindow.GetType());
            var newValues = new Dictionary<string, object>
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
        internal static CreatureEquipmentMakeInfo CreateCreatureEquipmentMakeInfo(EquipmentTypeInfo equipTypeInfo = null)
        {
            equipTypeInfo = equipTypeInfo ?? CreateEquipmentTypeInfo();

            return new CreatureEquipmentMakeInfo
            {
                equipTypeInfo = equipTypeInfo,
            };
        }

        [NotNull]
        internal static CreatureLayer CreateCreatureLayer(Dictionary<long, CreatureUnit> creatureDic = null)
        {
            creatureDic = creatureDic ?? new Dictionary<long, CreatureUnit>();

            CreateUninitializedObject<CreatureLayer>(out var creatureLayer);

            var fields = GetUninitializedFieldsIncludingBaseType(creatureLayer.GetType());
            var newValues = new Dictionary<string, object>
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
        internal static CreatureModel CreateCreatureModel(AgentModel agent = null,
            CreatureTypeInfo metaInfo = null,
            CreatureObserveInfoModel observeInfo = null,
            int qliphothCounter = 1,
            SkillTypeInfo skillTypeInfo = null)
        {
            agent = agent ?? CreateAgentModel();
            metaInfo = metaInfo ?? CreateCreatureTypeInfo();
            observeInfo = observeInfo ?? CreateCreatureObserveInfoModel();
            skillTypeInfo = skillTypeInfo ?? CreateSkillTypeInfo();

            CreateUninitializedObject<CreatureModel>(out var creatureModel);

            var fields = GetUninitializedFieldsIncludingBaseType(creatureModel.GetType());
            var newValues = new Dictionary<string, object>
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
            var newCreatureModel = GetPopulatedUninitializedObject(creatureModel, fields, newValues);

            // Needed to avoid a circular reference from currentSkill
            var currentSkill = CreateUseSkill(agent, skillTypeInfo, newCreatureModel);
            newValues.Add("_currentSkill", currentSkill);

            return GetPopulatedUninitializedObject(newCreatureModel, fields, newValues);
        }

        [NotNull]
        private static CreatureObserveInfoModel CreateCreatureObserveInfoModel(CreatureTypeInfo metaInfo = null,
            Dictionary<string, ObserveRegion> observeRegions = null)
        {
            metaInfo = metaInfo ?? CreateCreatureTypeInfo();
            observeRegions = observeRegions ?? new Dictionary<string, ObserveRegion>();

            CreateUninitializedObject<CreatureObserveInfoModel>(out var creatureObserveInfoModel);

            var fields = GetUninitializedFieldsIncludingBaseType(creatureObserveInfoModel.GetType());
            var newValues = new Dictionary<string, object>
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
        internal static CreatureTypeInfo CreateCreatureTypeInfo(List<CreatureEquipmentMakeInfo> equipMakeInfos = null)
        {
            equipMakeInfos = equipMakeInfos ?? new List<CreatureEquipmentMakeInfo>();

            return new CreatureTypeInfo
            {
                equipMakeInfos = equipMakeInfos,
            };
        }

        internal static CreatureUnit CreateCreatureUnit()
        {
            CreateUninitializedObject<CreatureUnit>(out var creatureUnit);

            return creatureUnit;
        }

        [NotNull]
        internal static CustomizingWindow CreateCustomizingWindow(GameObject appearanceBlock = null,
            AppearanceUI appearanceUI = null,
            AgentModel currentAgent = null,
            AgentData currentData = null,
            CustomizingType currentWindowType = (CustomizingType)1)
        {
            appearanceBlock = EnsureNotNullWithDefault(appearanceBlock, CreateGameObject);
            appearanceUI = EnsureNotNullWithDefault(appearanceUI, CreateAppearanceUI);
            currentAgent = EnsureNotNullWithDefault(currentAgent, () => CreateAgentModel());
            currentData = EnsureNotNullWithDefault(currentData, () => CreateAgentData());

            CreateUninitializedObject<CustomizingWindow>(out var customizingWindow);

            var fields = GetUninitializedFieldsIncludingBaseType(customizingWindow.GetType());
            var newValues = new Dictionary<string, object>
            {
                {
                    "appearanceBlock", appearanceBlock
                },
                {
                    "appearanceUI", appearanceUI
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
            CreateUninitializedObject<EGObonusInfo>(out var egoBonusInfo);

            var fields = GetUninitializedFieldsIncludingBaseType(egoBonusInfo.GetType());
            var newValues = new Dictionary<string, object>();

            return GetPopulatedUninitializedObject(egoBonusInfo, fields, newValues);
        }

        [NotNull]
        internal static EGOgiftModel CreateEgoGiftModel(EquipmentTypeInfo metaInfo = null,
            EquipmentScriptBase script = null)
        {
            metaInfo = metaInfo ?? CreateEquipmentTypeInfo();
            script = script ?? CreateEquipmentScriptBase();

            return new EGOgiftModel
            {
                metaInfo = metaInfo,
                script = script,
            };
        }

        [NotNull]
        private static EquipmentModel CreateEquipmentModel(EquipmentTypeInfo metaInfo = null)
        {
            metaInfo = metaInfo ?? CreateEquipmentTypeInfo();

            return new EquipmentModel
            {
                metaInfo = metaInfo,
            };
        }

        [NotNull]
        private static EquipmentScriptBase CreateEquipmentScriptBase(EquipmentModel model = null)
        {
            model = model ?? CreateEquipmentModel();

            CreateUninitializedObject<EquipmentScriptBase>(out var equipmentScriptBase);

            var fields = GetUninitializedFieldsIncludingBaseType(equipmentScriptBase.GetType());
            var newValues = new Dictionary<string, object>
            {
                {
                    "_model", model
                },
            };

            return GetPopulatedUninitializedObject(equipmentScriptBase, fields, newValues);
        }

        [NotNull]
        internal static EquipmentTypeInfo CreateEquipmentTypeInfo(EGObonusInfo bonus = null,
            Dictionary<string, string> localizeData = null)
        {
            bonus = bonus ?? CreateEgoBonusInfo();
            localizeData = localizeData ?? new Dictionary<string, string>();

            CreateUninitializedObject<EquipmentTypeInfo>(out var equipmentTypeInfo);

            var fields = GetUninitializedFieldsIncludingBaseType(equipmentTypeInfo.GetType());
            var newValues = new Dictionary<string, object>
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
            CreateUninitializedObject<FairyBuf>(out var fairyBuf);

            return fairyBuf;
        }

        [NotNull]
        internal static GameManager CreateGameManager()
        {
            CreateUninitializedObject<GameManager>(out var gameManager);

            var fields = GetUninitializedFieldsIncludingBaseType(gameManager.GetType());
            var newValues = new Dictionary<string, object>();
            gameManager = GetPopulatedUninitializedObject(gameManager, fields, newValues);
            newValues.Add("_currentGameManager", gameManager);

            return GetPopulatedUninitializedObject(gameManager, fields, newValues);
        }


        private static GameObject CreateGameObject()
        {
            CreateUninitializedObject<GameObject>(out var gameObject);

            return gameObject;
        }

        private static Image CreateImage()
        {
            CreateUninitializedObject<Image>(out var image);

            return image;
        }

        internal static LittleWitchBuf CreateLittleWitchBuf()
        {
            CreateUninitializedObject<LittleWitchBuf>(out var littleWitchBuf);

            return littleWitchBuf;
        }

        [NotNull]
        internal static LocalizeTextDataModel CreateLocalizeTextDataModel(Dictionary<string, string> list = null)
        {
            list = list ?? new Dictionary<string, string>();

            CreateUninitializedObject<LocalizeTextDataModel>(out var localizeTextDataModel);

            var fields = GetUninitializedFieldsIncludingBaseType(localizeTextDataModel.GetType());
            var newValues = new Dictionary<string, object>
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
        private static SkillTypeInfo CreateSkillTypeInfo()
        {
            return new SkillTypeInfo();
        }

        [NotNull]
        internal static SkillTypeList CreateSkillTypeList(SkillTypeInfo[] list = null)
        {
            list = list ?? Array.Empty<SkillTypeInfo>();

            var skillType = SkillTypeList.instance;
            skillType.Init(list);

            return skillType;
        }

        [NotNull]
        internal static Sprite CreateSprite(string name = "")
        {
            CreateUninitializedObject<Sprite>(out var sprite);

            var fields = GetUninitializedFieldsIncludingBaseType(sprite.GetType());
            var newValues = new Dictionary<string, object>
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
            CreateUninitializedObject<Text>(out var text);

            return text;
        }

        [NotNull]
        private static AgentInfoWindow.UIComponent CreateUIComponent()
        {
            return new AgentInfoWindow.UIComponent();
        }

        [NotNull]
        internal static UnitEquipSpace CreateUnitEquipSpace()
        {
            return new UnitEquipSpace();
        }

        [NotNull]
        internal static UnitModel CreateUnitModel(List<UnitBuf> bufList = null,
            List<UnitStatBuf> statBufList = null)
        {
            bufList = bufList ?? new List<UnitBuf>();
            statBufList = statBufList ?? new List<UnitStatBuf>();

            CreateUninitializedObject<UnitModel>(out var unitModel);

            var fields = GetUninitializedFieldsIncludingBaseType(unitModel.GetType());
            var newValues = new Dictionary<string, object>
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
        internal static UnitStatBuf CreateUnitStatBuf(WorkerPrimaryStatBonus primaryStat = null)
        {
            primaryStat = primaryStat ?? CreateWorkerPrimaryStatBonus();

            CreateUninitializedObject<UnitStatBuf>(out var unitStatBuf);

            var fields = GetUninitializedFieldsIncludingBaseType(unitStatBuf.GetType());
            var newValues = new Dictionary<string, object>
            {
                {
                    "primaryStat", primaryStat
                },
            };

            return GetPopulatedUninitializedObject(unitStatBuf, fields, newValues);
        }

        [NotNull]
        internal static UseSkill CreateUseSkill(AgentModel agent = null,
            SkillTypeInfo skillTypeInfo = null,
            CreatureModel targetCreature = null)
        {
            agent = agent ?? CreateAgentModel();
            skillTypeInfo = skillTypeInfo ?? CreateSkillTypeInfo();
            targetCreature = targetCreature ?? CreateCreatureModel();

            // Needed to avoid circular reference
            if (targetCreature.currentSkill.IsNotNull())
            {
                return targetCreature.currentSkill;
            }

            var useSkill = new UseSkill
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
        internal static WorkerSpriteManager CreateWorkerSpriteManager(WorkerBasicSpriteController basicData = null)
        {
            basicData = basicData ?? CreateWorkerBasicSpriteController();

            CreateUninitializedObject<WorkerSpriteManager>(out var workerSpriteManager);

            var fields = GetUninitializedFieldsIncludingBaseType(workerSpriteManager.GetType());
            var newValues = new Dictionary<string, object>
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
            CreateUninitializedObject<YggdrasilBlessBuf>(out var yggdrasilBlessBuf);

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
            obj = (TObject)FormatterServices.GetUninitializedObject(typeof(TObject));
        }

        /// <summary>Gets the fields of the specified type, including those of its base types, for uninitialized object.</summary>
        [NotNull]
        private static MemberInfo[] GetUninitializedFieldsIncludingBaseType(Type type)
        {
            var fields = new List<MemberInfo>();

            while (type.IsNotNull() && type != typeof(object))
            {
                var typeFields = type.GetFields(BindingFlagsInstance);
                fields.AddRange(typeFields.GetValidFields());

                var baseType = type.BaseType;
                if (baseType.IsNotNull())
                {
                    type = baseType;
                }
            }

            return fields.ToArray();
        }

        /// <summary>Returns the fields which are valid and can be used to populate an object.</summary>
        [NotNull]
        private static List<MemberInfo> GetValidFields([NotNull] this IEnumerable<FieldInfo> typeFields)
        {
            return typeFields.Where(FieldIsValidAndCanPopulateObject).Cast<MemberInfo>().ToList();
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
            var hasValidHandle = typeField.FieldHandle.Value != IntPtr.Zero;
            var isNotInitOnly = !typeField.IsInitOnly;
            return hasValidHandle && isNotInitOnly;
        }

        /// <summary>Populate the fields of an uninitialized object with a provided list of objects.</summary>
        [NotNull]
        private static TObject GetPopulatedUninitializedObject<TObject>([NotNull] TObject obj,
            [NotNull] IReadOnlyList<MemberInfo> fields,
            Dictionary<string, object> newValues) where TObject : class
        {
            CreateUninitializedObject<TObject>(out var newObj);
            var values = FormatterServices.GetObjectData(obj, fields.ToArray());

            for (var i = 0; i < fields.Count; i++)
            {
                if (newValues.TryGetValue(fields[i].Name, out var value))
                {
                    values[i] = value;
                }
            }

            FormatterServices.PopulateObjectMembers(newObj, fields.ToArray(), values);

            return newObj;
        }

        #endregion
    }
}
