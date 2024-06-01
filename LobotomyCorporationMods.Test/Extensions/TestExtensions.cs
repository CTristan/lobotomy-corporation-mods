// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using CommandWindow;
using Customizing;
using FluentAssertions;
using Harmony;
using LobotomyCorporationMods.Common.Interfaces;
using Moq;
using UnityEngine;
using UnityEngine.UI;
using WorkerSprite;
using ILogger = LobotomyCorporationMods.Common.Interfaces.ILogger;

#endregion

// ReSharper disable MemberCanBePrivate.Global
namespace LobotomyCorporationMods.Test.Extensions
{
    internal static class TestExtensions
    {
        internal static Mock<IFileManager> GetMockFileManager()
        {
            var mockFileManager = new Mock<IFileManager>();
            _ = mockFileManager.Setup(static fm => fm.GetOrCreateFile(It.IsAny<string>())).Returns(static (string fileName) => fileName.InCurrentDirectory());
            _ = mockFileManager.Setup(static fm => fm.ReadAllText(It.IsAny<string>(), It.IsAny<bool>())).Returns(static (string fileName, bool _) => File.ReadAllText(fileName.InCurrentDirectory()));

            return mockFileManager;
        }

        internal static Mock<ILogger> GetMockLogger()
        {
            var mockLogger = new Mock<ILogger>();

            return mockLogger;
        }

        internal static string InCurrentDirectory(this string fileName)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), fileName);
        }

        internal static void ValidateHarmonyPatch(this MemberInfo patchClass, Type originalClass, string methodName)
        {
            var attribute = Attribute.GetCustomAttribute(patchClass, typeof(HarmonyPatch)) as HarmonyPatch;

            attribute.Should().NotBeNull();
            attribute?.info.originalType.Should().Be(originalClass);
            attribute?.info.methodName.Should().Be(methodName);
        }

        internal static void VerifyExceptionLogged<TException>(this Mock<ILogger> mockLogger, Action action, int numberOfTimes = 1) where TException : Exception
        {
            action.ShouldThrow<TException>();
            mockLogger.Verify(static logger => logger.WriteToLog(It.IsAny<TException>()), Times.Exactly(numberOfTimes));
        }

        #region Unity Objects

        internal static AgentData CreateAgentData(AgentName? agentName = null, Appearance? appearance = null, StatBonus? statBonus = null)
        {
            agentName ??= CreateAgentName();
            appearance ??= CreateAppearance();
            statBonus ??= CreateStatBonus();

            return new AgentData { agentName = agentName, appearance = appearance, statBonus = statBonus };
        }

        internal static AgentInfoWindow CreateAgentInfoWindow(GameObject? customizingBlock = null, CustomizingWindow? customizingWindow = null, AgentInfoWindow.UIComponent? uiComponents = null)
        {
            customizingBlock ??= CreateGameObject();
            customizingWindow ??= CreateCustomizingWindow();
            uiComponents ??= CreateUIComponent();

            CreateUninitializedObject<AgentInfoWindow>(out var agentInfoWindow);

            var fields = GetUninitializedObjectFields(agentInfoWindow.GetType());
            var newValues = new Dictionary<string, object> { { "customizingBlock", customizingBlock }, { "customizingWindow", customizingWindow }, { "UIComponents", uiComponents } };
            agentInfoWindow = GetPopulatedUninitializedObject(agentInfoWindow, fields, newValues);
            newValues.Add("currentWindow", agentInfoWindow);

            return GetPopulatedUninitializedObject(agentInfoWindow, fields, newValues);
        }

        internal static AgentModel CreateAgentModel(AgentName? agentName = null, List<UnitBuf>? bufList = null, UnitEquipSpace? equipment = null, long instanceId = 1L, string name = "",
            WorkerPrimaryStat? primaryStat = null, WorkerSprite.WorkerSprite? spriteData = null, List<UnitStatBuf>? statBufList = null)
        {
            agentName ??= CreateAgentName();
            bufList ??= new List<UnitBuf>();
            equipment ??= CreateUnitEquipSpace();
            primaryStat ??= CreateWorkerPrimaryStat();
            spriteData ??= CreateWorkerSprite();
            statBufList ??= new List<UnitStatBuf>();

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

        internal static AgentName CreateAgentName(AgentNameTypeInfo? metaInfo = null, Dictionary<string, string>? nameDic = null)
        {
            metaInfo ??= CreateAgentNameTypeInfo();
            nameDic ??= new Dictionary<string, string>();

            CreateUninitializedObject<AgentName>(out var agentName);

            var fields = GetUninitializedObjectFields(agentName.GetType());
            var newValues = new Dictionary<string, object> { { "metaInfo", metaInfo }, { "nameDic", nameDic } };

            return GetPopulatedUninitializedObject(agentName, fields, newValues);
        }

        internal static AgentNameTypeInfo CreateAgentNameTypeInfo(Dictionary<string, string>? nameDic = null)
        {
            nameDic ??= new Dictionary<string, string>();

            return new AgentNameTypeInfo { nameDic = nameDic };
        }

        internal static AgentSlot CreateAgentSlot(List<MaskableGraphic>? coloredTargets = null, AgentModel? currentAgent = null, Image? workFilterFill = null, Text? workFilterText = null)
        {
            coloredTargets ??= new List<MaskableGraphic>();
            currentAgent ??= CreateAgentModel();
            workFilterFill ??= CreateImage();
            workFilterText ??= CreateText();

            CreateUninitializedObject<AgentSlot>(out var agentSlot);

            var fields = GetUninitializedObjectFields(agentSlot.GetType());
            var newValues = new Dictionary<string, object>
            {
                { "coloredTargets", coloredTargets }, { "_currentAgent", currentAgent }, { "WorkFilterFill", workFilterFill }, { "WorkFilterText", workFilterText }
            };

            return GetPopulatedUninitializedObject(agentSlot, fields, newValues);
        }

        internal static Appearance CreateAppearance(WorkerSprite.WorkerSprite? spriteSet = null)
        {
            spriteSet ??= CreateWorkerSprite();

            return new Appearance { spriteSet = spriteSet };
        }

        internal static AppearanceUI CreateAppearanceUI()
        {
            CreateUninitializedObject<AppearanceUI>(out var appearanceUi);

            return appearanceUi;
        }

        internal static CommandWindow.CommandWindow CreateCommandWindow(UnitModel? currentTarget = null, CommandType currentWindowType = (CommandType)1, long selectedWork = 0L)
        {
            currentTarget ??= CreateUnitModel();

            CreateUninitializedObject<CommandWindow.CommandWindow>(out var commandWindow);

            var fields = GetUninitializedObjectFields(commandWindow.GetType());
            var newValues = new Dictionary<string, object> { { "_currentTarget", currentTarget }, { "_currentWindowType", currentWindowType }, { "_selectedWork", selectedWork } };
            commandWindow = GetPopulatedUninitializedObject(commandWindow, fields, newValues);
            newValues.Add("_currentWindow", commandWindow);

            return GetPopulatedUninitializedObject(commandWindow, fields, newValues);
        }

        internal static CreatureEquipmentMakeInfo CreateCreatureEquipmentMakeInfo(EquipmentTypeInfo? equipTypeInfo = null)
        {
            equipTypeInfo ??= CreateEquipmentTypeInfo();

            return new CreatureEquipmentMakeInfo { equipTypeInfo = equipTypeInfo };
        }

        internal static CreatureLayer CreateCreatureLayer(Dictionary<long, CreatureUnit>? creatureDic = null)
        {
            creatureDic ??= new Dictionary<long, CreatureUnit>();

            CreateUninitializedObject<CreatureLayer>(out var creatureLayer);

            var fields = GetUninitializedObjectFields(creatureLayer.GetType());
            var newValues = new Dictionary<string, object> { { "creatureDic", creatureDic } };
            creatureLayer = GetPopulatedUninitializedObject(creatureLayer, fields, newValues);
            newValues.Add("<currentLayer>k__BackingField", creatureLayer);

            creatureLayer = GetPopulatedUninitializedObject(creatureLayer, fields, newValues);

            return creatureLayer;
        }

        internal static CreatureModel CreateCreatureModel(AgentModel? agent = null, CreatureTypeInfo? metaInfo = null, CreatureObserveInfoModel? observeInfo = null, int qliphothCounter = 1,
            SkillTypeInfo? skillTypeInfo = null)
        {
            agent ??= CreateAgentModel();
            metaInfo ??= CreateCreatureTypeInfo();
            observeInfo ??= CreateCreatureObserveInfoModel();
            skillTypeInfo ??= CreateSkillTypeInfo();

            CreateUninitializedObject<CreatureModel>(out var creatureModel);

            var fields = GetUninitializedObjectFields(creatureModel.GetType());
            var newValues = new Dictionary<string, object> { { "metaInfo", metaInfo }, { "observeInfo", observeInfo }, { "_qliphothCounter", qliphothCounter } };
            var newCreatureModel = GetPopulatedUninitializedObject(creatureModel, fields, newValues);

            // Needed to avoid a circular reference from currentSkill
            var currentSkill = CreateUseSkill(agent, skillTypeInfo, newCreatureModel);
            newValues.Add("_currentSkill", currentSkill);

            return GetPopulatedUninitializedObject(newCreatureModel, fields, newValues);
        }

        internal static CreatureObserveInfoModel CreateCreatureObserveInfoModel(CreatureTypeInfo? metaInfo = null, Dictionary<string, ObserveRegion>? observeRegions = null)
        {
            metaInfo ??= CreateCreatureTypeInfo();
            observeRegions ??= new Dictionary<string, ObserveRegion>();

            CreateUninitializedObject<CreatureObserveInfoModel>(out var creatureObserveInfoModel);

            var fields = GetUninitializedObjectFields(creatureObserveInfoModel.GetType());
            var newValues = new Dictionary<string, object> { { "_metaInfo", metaInfo }, { "observeRegions", observeRegions } };

            return GetPopulatedUninitializedObject(creatureObserveInfoModel, fields, newValues);
        }

        internal static CreatureTypeInfo CreateCreatureTypeInfo(List<CreatureEquipmentMakeInfo>? equipMakeInfos = null)
        {
            equipMakeInfos ??= new List<CreatureEquipmentMakeInfo>();

            return new CreatureTypeInfo { equipMakeInfos = equipMakeInfos };
        }

        internal static CreatureUnit CreateCreatureUnit()
        {
            CreateUninitializedObject<CreatureUnit>(out var creatureUnit);

            return creatureUnit;
        }

        internal static CustomizingWindow CreateCustomizingWindow(GameObject? appearanceBlock = null, AppearanceUI? appearanceUI = null, AgentModel? currentAgent = null, AgentData? currentData = null,
            CustomizingType currentWindowType = (CustomizingType)1)
        {
            appearanceBlock ??= CreateGameObject();
            appearanceUI ??= CreateAppearanceUI();
            currentAgent ??= CreateAgentModel();
            currentData ??= CreateAgentData();

            CreateUninitializedObject<CustomizingWindow>(out var customizingWindow);

            var fields = GetUninitializedObjectFields(customizingWindow.GetType());
            var newValues = new Dictionary<string, object>
            {
                { "appearanceBlock", appearanceBlock },
                { "appearanceUI", appearanceUI },
                { "_currentAgent", currentAgent },
                { "CurrentData", currentData },
                { "_currentWindowType", currentWindowType }
            };
            customizingWindow = GetPopulatedUninitializedObject(customizingWindow, fields, newValues);
            newValues.Add("_currentWindow", customizingWindow);

            return GetPopulatedUninitializedObject(customizingWindow, fields, newValues);
        }

        internal static EGOgiftModel CreateEgoGiftModel(EquipmentTypeInfo? metaInfo = null, EquipmentScriptBase? script = null)
        {
            metaInfo ??= CreateEquipmentTypeInfo();
            script ??= CreateEquipmentScriptBase();

            return new EGOgiftModel { metaInfo = metaInfo, script = script };
        }

        internal static EquipmentModel CreateEquipmentModel(EquipmentTypeInfo? metaInfo = null)
        {
            metaInfo ??= CreateEquipmentTypeInfo();

            return new EquipmentModel { metaInfo = metaInfo };
        }

        internal static EquipmentScriptBase CreateEquipmentScriptBase(EquipmentModel? model = null)
        {
            model ??= CreateEquipmentModel();

            CreateUninitializedObject<EquipmentScriptBase>(out var equipmentScriptBase);

            var fields = GetUninitializedObjectFields(equipmentScriptBase.GetType());
            var newValues = new Dictionary<string, object> { { "_model", model } };

            return GetPopulatedUninitializedObject(equipmentScriptBase, fields, newValues);
        }

        internal static EquipmentTypeInfo CreateEquipmentTypeInfo()
        {
            return new EquipmentTypeInfo();
        }

        internal static FairyBuf CreateFairyBuf()
        {
            CreateUninitializedObject<FairyBuf>(out var fairyBuf);

            return fairyBuf;
        }

        internal static GameManager CreateGameManager()
        {
            CreateUninitializedObject<GameManager>(out var gameManager);

            var fields = GetUninitializedObjectFields(gameManager.GetType());
            var newValues = new Dictionary<string, object>();
            gameManager = GetPopulatedUninitializedObject(gameManager, fields, newValues);
            newValues.Add("_currentGameManager", gameManager);

            return GetPopulatedUninitializedObject(gameManager, fields, newValues);
        }


        internal static GameObject CreateGameObject()
        {
            CreateUninitializedObject<GameObject>(out var gameObject);

            return gameObject;
        }

        internal static Image CreateImage()
        {
            CreateUninitializedObject<Image>(out var image);

            return image;
        }

        internal static LittleWitchBuf CreateLittleWitchBuf()
        {
            CreateUninitializedObject<LittleWitchBuf>(out var littleWitchBuf);

            return littleWitchBuf;
        }

        internal static LocalizeTextDataModel CreateLocalizeTextDataModel(Dictionary<string, string>? list = null)
        {
            list ??= new Dictionary<string, string>();

            CreateUninitializedObject<LocalizeTextDataModel>(out var localizeTextDataModel);

            var fields = GetUninitializedObjectFields(localizeTextDataModel.GetType());
            var newValues = new Dictionary<string, object> { { "_list", list } };
            localizeTextDataModel = GetPopulatedUninitializedObject(localizeTextDataModel, fields, newValues);
            newValues.Add("_instance", localizeTextDataModel);

            return GetPopulatedUninitializedObject(localizeTextDataModel, fields, newValues);
        }

        internal static SkillTypeInfo CreateSkillTypeInfo()
        {
            return new SkillTypeInfo();
        }

        internal static SkillTypeList CreateSkillTypeList(SkillTypeInfo[]? list = null)
        {
            list ??= new SkillTypeInfo[0];

            var skillType = SkillTypeList.instance;
            skillType.Init(list);

            return skillType;
        }

        internal static Sprite CreateSprite(string name = "")
        {
            CreateUninitializedObject<Sprite>(out var sprite);

            var fields = GetUninitializedObjectFields(sprite.GetType());
            var newValues = new Dictionary<string, object> { { "name", name } };

            return GetPopulatedUninitializedObject(sprite, fields, newValues);
        }

        internal static StatBonus CreateStatBonus()
        {
            return new StatBonus();
        }

        internal static Text CreateText()
        {
            CreateUninitializedObject<Text>(out var text);

            return text;
        }

        internal static AgentInfoWindow.UIComponent CreateUIComponent()
        {
            return new AgentInfoWindow.UIComponent();
        }

        internal static UnitEquipSpace CreateUnitEquipSpace()
        {
            return new UnitEquipSpace();
        }

        internal static UnitModel CreateUnitModel(List<UnitBuf>? bufList = null, List<UnitStatBuf>? statBufList = null)
        {
            bufList ??= new List<UnitBuf>();
            statBufList ??= new List<UnitStatBuf>();

            CreateUninitializedObject<UnitModel>(out var unitModel);

            var fields = GetUninitializedObjectFields(unitModel.GetType());
            var newValues = new Dictionary<string, object> { { "_bufList", bufList }, { "_statBufList", statBufList } };

            return GetPopulatedUninitializedObject(unitModel, fields, newValues);
        }

        internal static UnitStatBuf CreateUnitStatBuf(WorkerPrimaryStatBonus? primaryStat = null)
        {
            primaryStat ??= CreateWorkerPrimaryStatBonus();

            CreateUninitializedObject<UnitStatBuf>(out var unitStatBuf);

            var fields = GetUninitializedObjectFields(unitStatBuf.GetType());
            var newValues = new Dictionary<string, object> { { "primaryStat", primaryStat } };

            return GetPopulatedUninitializedObject(unitStatBuf, fields, newValues);
        }

        internal static UseSkill CreateUseSkill(AgentModel? agent = null, SkillTypeInfo? skillTypeInfo = null, CreatureModel? targetCreature = null)
        {
            agent ??= CreateAgentModel();
            skillTypeInfo ??= CreateSkillTypeInfo();
            targetCreature ??= CreateCreatureModel();

            // Needed to avoid circular reference
            if (targetCreature.currentSkill is not null)
            {
                return targetCreature.currentSkill;
            }

            var useSkill = new UseSkill { agent = agent, skillTypeInfo = skillTypeInfo, targetCreature = targetCreature };
            targetCreature.currentSkill = useSkill;

            return useSkill;
        }

        internal static WorkerBasicSpriteController CreateWorkerBasicSpriteController()
        {
            return new WorkerBasicSpriteController();
        }

        internal static WorkerPrimaryStat CreateWorkerPrimaryStat()
        {
            return new WorkerPrimaryStat();
        }

        internal static WorkerPrimaryStatBonus CreateWorkerPrimaryStatBonus()
        {
            return new WorkerPrimaryStatBonus();
        }

        internal static WorkerSprite.WorkerSprite CreateWorkerSprite()
        {
            return new WorkerSprite.WorkerSprite();
        }

        internal static WorkerSpriteManager CreateWorkerSpriteManager(WorkerBasicSpriteController? basicData = null)
        {
            basicData ??= CreateWorkerBasicSpriteController();

            CreateUninitializedObject<WorkerSpriteManager>(out var workerSpriteManager);

            var fields = GetUninitializedObjectFields(workerSpriteManager.GetType());
            var newValues = new Dictionary<string, object> { { "basicData", basicData } };
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
        private static MemberInfo[] GetUninitializedObjectFields(Type type)
        {
            var fields = new List<MemberInfo>();

            while (type != typeof(object))
            {
                var typeFields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static);
                fields.AddRange(typeFields.GetValidFields());

                if (type.BaseType != null)
                {
                    type = type.BaseType;
                }
            }

            return fields.ToArray();
        }

        /// <summary>
        ///     Returns the fields which are valid and can be used to populate an object.
        /// </summary>
        private static List<MemberInfo> GetValidFields(this IEnumerable<FieldInfo> typeFields)
        {
            var goodFields = new List<MemberInfo>();
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
        private static TObject GetPopulatedUninitializedObject<TObject>(TObject obj, MemberInfo[] fields, Dictionary<string, object> newValues) where TObject : class
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
