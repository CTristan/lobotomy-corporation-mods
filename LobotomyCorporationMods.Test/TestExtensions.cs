// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using CommandWindow;
using Customizing;
using FluentAssertions;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
using Moq;
using UnityEngine;
using UnityEngine.UI;
using WorkerSprite;

namespace LobotomyCorporationMods.Test
{
    [SuppressMessage("ReSharper", "Unity.IncorrectMonoBehaviourInstantiation")]
    internal static class TestExtensions
    {
        [NotNull]
        public static Mock<IFileManager> GetMockFileManager()
        {
            var mockFileManager = new Mock<IFileManager>();
            mockFileManager.Setup(fm => fm.GetFile(It.IsAny<string>())).Returns((string fileName) => fileName.InCurrentDirectory());
            mockFileManager.Setup(fm => fm.ReadAllText(It.IsAny<string>(), It.IsAny<bool>())).Returns((string fileName, bool createIfNotExists) => File.ReadAllText(fileName.InCurrentDirectory()));

            return mockFileManager;
        }

        [NotNull]
        internal static string InCurrentDirectory([NotNull] this string fileName)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), fileName);
        }

        internal static void ShouldThrowUnityException(this Action action, string because = "", params object[] becauseArgs)
        {
            action.ShouldThrow<Exception>().Where(ex => ex is SecurityException || ex is MissingMethodException);
        }

        public static void ValidateHarmonyPatch([NotNull] this MemberInfo patchClass, [NotNull] Type originalClass, string methodName)
        {
            var attribute = Attribute.GetCustomAttribute(patchClass, typeof(HarmonyPatch)) as HarmonyPatch;

            attribute.Should().NotBeNull();
            attribute?.info.originalType.Should().Be(originalClass);
            attribute?.info.methodName.Should().Be(methodName);
        }

        #region Unity Objects

        [NotNull]
        public static AgentData CreateAgentData(AgentName agentName, Appearance appearance)
        {
            return new AgentData { agentName = agentName, appearance = appearance };
        }

        [NotNull]
        public static AgentModel CreateAgentModel(AgentName agentName, List<UnitBuf> bufList, UnitEquipSpace equipment, long instanceId, string name, WorkerPrimaryStat primaryStat,
            WorkerSprite.WorkerSprite spriteData, List<UnitStatBuf> statBufList)
        {
            CreateUninitializedObject<AgentModel>(out var agentModel);

            var fields = GetUninitializedObjectFields(agentModel.GetType());
            var newValues = new Dictionary<string, object>
            {
                { "_agentName", agentName },
                { "_bufList", bufList },
                { "_equipment", equipment },
                { "instanceId", instanceId },
                { "name", name },
                { "primaryStat", primaryStat },
                { "spriteData", spriteData },
                { "_statBufList", statBufList }
            };

            return GetPopulatedUninitializedObject(agentModel, fields, newValues);
        }

        [NotNull]
        public static AgentName CreateAgentName([NotNull] GlobalGameManager globalGameManager, AgentNameTypeInfo metaInfo, Dictionary<string, string> nameDic)
        {
            // Requires an existing GlobalGameManager instance
            Guard.Against.Null(globalGameManager, nameof(globalGameManager));

            CreateUninitializedObject<AgentName>(out var agentName);

            var fields = GetUninitializedObjectFields(agentName.GetType());
            var newValues = new Dictionary<string, object> { { "metaInfo", metaInfo }, { "nameDic", nameDic } };

            return GetPopulatedUninitializedObject(agentName, fields, newValues);
        }

        [NotNull]
        public static AgentNameTypeInfo CreateAgentNameTypeInfo(Dictionary<string, string> nameDic)
        {
            return new AgentNameTypeInfo { nameDic = nameDic };
        }

        [NotNull]
        public static AgentSlot CreateAgentSlot(AgentModel currentAgent, Image workFilterFill, Text workFilterText)
        {
            CreateUninitializedObject<AgentSlot>(out var agentSlot);

            var fields = GetUninitializedObjectFields(agentSlot.GetType());
            var newValues = new Dictionary<string, object> { { "_currentAgent", currentAgent }, { "WorkFilterFill", workFilterFill }, { "WorkFilterText", workFilterText } };

            return GetPopulatedUninitializedObject(agentSlot, fields, newValues);
        }

        [NotNull]
        public static Appearance CreateAppearance([NotNull] WorkerSpriteManager workerSpriteManager, WorkerSprite.WorkerSprite spriteSet)
        {
            // Requires an existing WorkerSpriteManager instance
            Guard.Against.Null(workerSpriteManager, nameof(workerSpriteManager));

            return new Appearance { spriteSet = spriteSet };
        }

        [NotNull]
        public static AppearanceUI CreateAppearanceUI()
        {
            return new AppearanceUI();
        }

        [NotNull]
        public static CommandWindow.CommandWindow CreateCommandWindow(UnitModel currentTarget, CommandType currentWindowType, long selectedWork, [NotNull] SkillTypeList skillTypeList)
        {
            // Requires an existing SkillTypeList instance
            Guard.Against.Null(skillTypeList, nameof(skillTypeList));

            CreateUninitializedObject<CommandWindow.CommandWindow>(out var commandWindow);

            var fields = GetUninitializedObjectFields(commandWindow.GetType());
            var newValues = new Dictionary<string, object> { { "_currentTarget", currentTarget }, { "_currentWindowType", currentWindowType }, { "_selectedWork", selectedWork } };
            commandWindow = GetPopulatedUninitializedObject(commandWindow, fields, newValues);
            newValues.Add("_currentWindow", commandWindow);

            return GetPopulatedUninitializedObject(commandWindow, fields, newValues);
        }

        [NotNull]
        public static CreatureEquipmentMakeInfo CreateCreatureEquipmentMakeInfo(EquipmentTypeInfo equipTypeInfo)
        {
            return new CreatureEquipmentMakeInfo { equipTypeInfo = equipTypeInfo };
        }

        [NotNull]
        public static CreatureLayer CreateCreatureLayer(Dictionary<long, CreatureUnit> creatureDic)
        {
            CreateUninitializedObject<CreatureLayer>(out var creatureLayer);

            var fields = GetUninitializedObjectFields(creatureLayer.GetType());
            var newValues = new Dictionary<string, object> { { "creatureDic", creatureDic } };
            creatureLayer = GetPopulatedUninitializedObject(creatureLayer, fields, newValues);
            newValues.Add("<currentLayer>k__BackingField", creatureLayer);

            creatureLayer = GetPopulatedUninitializedObject(creatureLayer, fields, newValues);

            return creatureLayer;
        }

        [NotNull]
        public static CreatureModel CreateCreatureModel(AgentModel agent, [NotNull] CreatureLayer creatureLayer, CreatureTypeInfo metaInfo, CreatureObserveInfoModel observeInfo, int qliphothCounter,
            SkillTypeInfo skillTypeInfo)
        {
            // Requires an existing CreatureLayer instance
            Guard.Against.Null(creatureLayer, nameof(creatureLayer));

            CreateUninitializedObject<CreatureModel>(out var creatureModel);

            var fields = GetUninitializedObjectFields(creatureModel.GetType());
            var newValues = new Dictionary<string, object> { { "metaInfo", metaInfo }, { "observeInfo", observeInfo }, { "_qliphothCounter", qliphothCounter } };
            var newCreatureModel = GetPopulatedUninitializedObject(creatureModel, fields, newValues);

            // Needed to avoid a circular reference from currentSkill
            var currentSkill = CreateUseSkill(agent, skillTypeInfo, newCreatureModel);
            newValues.Add("_currentSkill", currentSkill);

            return GetPopulatedUninitializedObject(newCreatureModel, fields, newValues);
        }

        [NotNull]
        public static CreatureObserveInfoModel CreateCreatureObserveInfoModel(CreatureTypeInfo metaInfo, Dictionary<string, ObserveRegion> observeRegions)
        {
            CreateUninitializedObject<CreatureObserveInfoModel>(out var creatureObserveInfoModel);

            var fields = GetUninitializedObjectFields(creatureObserveInfoModel.GetType());
            var newValues = new Dictionary<string, object> { { "_metaInfo", metaInfo }, { "observeRegions", observeRegions } };

            return GetPopulatedUninitializedObject(creatureObserveInfoModel, fields, newValues);
        }

        [NotNull]
        public static CreatureTypeInfo CreateCreatureTypeInfo(List<CreatureEquipmentMakeInfo> equipMakeInfos)
        {
            return new CreatureTypeInfo { equipMakeInfos = equipMakeInfos };
        }

        [NotNull]
        public static CreatureUnit CreateCreatureUnit()
        {
            return new CreatureUnit();
        }

        [NotNull]
        public static CustomizingWindow CreateCustomizingWindow(AppearanceUI appearanceUI, AgentModel currentAgent, AgentData currentData, CustomizingType currentWindowType)
        {
            CreateUninitializedObject<CustomizingWindow>(out var customizingWindow);

            var fields = GetUninitializedObjectFields(customizingWindow.GetType());
            var newValues = new Dictionary<string, object>
            {
                { "appearanceUI", appearanceUI }, { "_currentAgent", currentAgent }, { "CurrentData", currentData }, { "_currentWindowType", currentWindowType }
            };

            return GetPopulatedUninitializedObject(customizingWindow, fields, newValues);
        }

        [NotNull]
        public static EGOgiftModel CreateEgoGiftModel(EquipmentTypeInfo metaInfo, EquipmentScriptBase script)
        {
            return new EGOgiftModel { metaInfo = metaInfo, script = script };
        }

        [NotNull]
        public static EquipmentModel CreateEquipmentModel(EquipmentTypeInfo metaInfo)
        {
            return new EquipmentModel { metaInfo = metaInfo };
        }

        [NotNull]
        public static EquipmentScriptBase CreateEquipmentScriptBase(EquipmentModel model)
        {
            CreateUninitializedObject<EquipmentScriptBase>(out var equipmentScriptBase);

            var fields = GetUninitializedObjectFields(equipmentScriptBase.GetType());
            var newValues = new Dictionary<string, object> { { "_model", model } };

            return GetPopulatedUninitializedObject(equipmentScriptBase, fields, newValues);
        }

        [NotNull]
        public static EquipmentTypeInfo CreateEquipmentTypeInfo()
        {
            return new EquipmentTypeInfo();
        }

        [NotNull]
        public static GameObject CreateGameObject()
        {
            CreateUninitializedObject<GameObject>(out var gameObject);

            var fields = GetUninitializedObjectFields(gameObject.GetType());
            var newValues = new Dictionary<string, object> { { "activeSelf", true } };

            return GetPopulatedUninitializedObject(gameObject, fields, newValues);
        }

        [NotNull]
        public static GlobalGameManager CreateGlobalGameManager()
        {
            CreateUninitializedObject<GlobalGameManager>(out var globalGameManager);

            var fields = GetUninitializedObjectFields(globalGameManager.GetType());
            var newValues = new Dictionary<string, object>();
            globalGameManager = GetPopulatedUninitializedObject(globalGameManager, fields, newValues);
            newValues.Add("_instance", globalGameManager);

            return GetPopulatedUninitializedObject(globalGameManager, fields, newValues);
        }

        [NotNull]
        public static Image CreateImage()
        {
            CreateUninitializedObject<Image>(out var image);

            return image;
        }

        [NotNull]
        public static SkillTypeInfo CreateSkillTypeInfo()
        {
            return new SkillTypeInfo();
        }

        [NotNull]
        public static SkillTypeList CreateSkillTypeList(SkillTypeInfo[] list)
        {
            var skillType = SkillTypeList.instance;
            skillType.Init(list);

            return skillType;
        }

        [NotNull]
        public static Sprite CreateSprite(string name)
        {
            CreateUninitializedObject<Sprite>(out var sprite);

            var fields = GetUninitializedObjectFields(sprite.GetType());
            var newValues = new Dictionary<string, object> { { "name", name } };

            return GetPopulatedUninitializedObject(sprite, fields, newValues);
        }

        [NotNull]
        public static Text CreateText()
        {
            CreateUninitializedObject<Text>(out var text);

            return text;
        }

        [NotNull]
        public static UnitEquipSpace CreateUnitEquipSpace()
        {
            return new UnitEquipSpace();
        }

        [NotNull]
        public static UnitModel CreateUnitModel(List<UnitBuf> bufList, List<UnitStatBuf> statBufList)
        {
            CreateUninitializedObject<UnitModel>(out var unitModel);

            var fields = GetUninitializedObjectFields(unitModel.GetType());
            var newValues = new Dictionary<string, object> { { "_bufList", bufList }, { "_statBufList", statBufList } };

            return GetPopulatedUninitializedObject(unitModel, fields, newValues);
        }

        [NotNull]
        public static UseSkill CreateUseSkill(AgentModel agent, SkillTypeInfo skillTypeInfo, [NotNull] CreatureModel targetCreature)
        {
            // Needed to avoid circular reference
            if (targetCreature.currentSkill != null)
            {
                return targetCreature.currentSkill;
            }

            var useSkill = new UseSkill { agent = agent, skillTypeInfo = skillTypeInfo, targetCreature = targetCreature };
            targetCreature.currentSkill = useSkill;

            return useSkill;
        }

        [NotNull]
        public static WorkerBasicSpriteController CreateWorkerBasicSpriteController()
        {
            return new WorkerBasicSpriteController();
        }

        [NotNull]
        public static WorkerPrimaryStat CreateWorkerPrimaryStat()
        {
            return new WorkerPrimaryStat();
        }

        [NotNull]
        public static WorkerSprite.WorkerSprite CreateWorkerSprite()
        {
            return new WorkerSprite.WorkerSprite();
        }

        [NotNull]
        public static WorkerSpriteManager CreateWorkerSpriteManager(WorkerBasicSpriteController basicData)
        {
            CreateUninitializedObject<WorkerSpriteManager>(out var workerSpriteManager);

            var fields = GetUninitializedObjectFields(workerSpriteManager.GetType());
            var newValues = new Dictionary<string, object> { { "basicData", basicData } };
            workerSpriteManager = GetPopulatedUninitializedObject(workerSpriteManager, fields, newValues);
            newValues.Add("_instance", workerSpriteManager);

            return GetPopulatedUninitializedObject(workerSpriteManager, fields, newValues);
        }

        #endregion

        #region Uninitialized Object Functions

        /// <summary>
        ///     Create an uninitialized object without calling a constructor. Needed because some of the classes we need
        ///     to mock either don't have a public constructor or cause a Unity exception.
        /// </summary>
        private static void CreateUninitializedObject<TObject>(out TObject obj)
        {
            obj = (TObject)FormatterServices.GetSafeUninitializedObject(typeof(TObject));
        }

        /// <summary>
        ///     Get the fields for an uninitialized object. Can be used to later initialize the individual fields as needed.
        /// </summary>
        [NotNull]
        private static MemberInfo[] GetUninitializedObjectFields(Type type)
        {
            var fields = new List<MemberInfo>();

            while (type != typeof(object))
            {
                if (type == null)
                {
                    continue;
                }

                var typeFields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static);
                fields.AddRange(typeFields.GetValidFields());
                type = type.BaseType;
            }

            return fields.ToArray();
        }

        /// <summary>
        ///     Returns the fields which are valid and can be used to populate an object.
        /// </summary>
        [NotNull]
        private static IEnumerable<FieldInfo> GetValidFields([NotNull] this IEnumerable<FieldInfo> typeFields)
        {
            var goodFields = new List<FieldInfo>();
            foreach (var typeField in typeFields)
            {
                try
                {
                    if (typeField.FieldHandle.Value != IntPtr.Zero)
                    {
                        goodFields.Add(typeField);
                    }
                }
                catch (NotSupportedException)
                {
                    // Some fields will give an exception when checked, so we won't be able to populate those fields
                }
            }

            return goodFields;
        }

        /// <summary>
        ///     Populate the fields of an uninitialized object with a provided list of objects.
        /// </summary>
        [NotNull]
        private static TObject GetPopulatedUninitializedObject<TObject>([NotNull] TObject obj, [NotNull] MemberInfo[] fields, IDictionary<string, object> newValues)
        {
            CreateUninitializedObject<TObject>(out var newObj);
            var values = FormatterServices.GetObjectData(obj, fields.ToArray());

            for (var i = 0; i < fields.Length; i++)
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
