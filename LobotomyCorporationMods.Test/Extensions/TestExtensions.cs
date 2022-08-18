// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using CommandWindow;
using FluentAssertions;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
using NSubstitute;

// ReSharper disable once CheckNamespace
namespace LobotomyCorporationMods.Test
{
    internal static class TestExtensions
    {
        [NotNull]
        public static IFileManager CreateFileManager()
        {
            var fileManager = Substitute.For<IFileManager>(null);
            fileManager.GetFile(default).ReturnsForAnyArgs(x => x.Arg<string>().InCurrentDirectory());
            fileManager.ReadAllText(default, default).ReturnsForAnyArgs(x => File.ReadAllText(x.Arg<string>().InCurrentDirectory()));

            return fileManager;
        }

        [NotNull]
        internal static string InCurrentDirectory([NotNull] this string fileName)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), fileName);
        }

        /// <summary>
        ///     Depending on the environment, the same test may return a different exception. Both exception types appear for the
        ///     same reason (trying to use a Unity-specific method), so we'll check if the exception we get is either of those
        ///     types to verify our test isn't getting an exception for some other reason.
        /// </summary>
        public static bool AssertIsUnityException(Exception exception)
        {
            return exception is SecurityException || exception is MissingMethodException;
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
        public static AgentModel CreateAgentModel(long instanceId)
        {
            CreateUninitializedObject<AgentModel>(out var agentModel);
            var fields = GetUninitializedObjectFields(agentModel.GetType());
            var newValues = new Dictionary<string, object> { { "instanceId", instanceId } };

            return GetPopulatedUninitializedObject(agentModel, fields, newValues);
        }

        [NotNull]
        public static AgentSlot CreateAgentSlot(AgentState agentState)
        {
            CreateUninitializedObject<AgentSlot>(out var agentSlot);
            var fields = GetUninitializedObjectFields(agentSlot.GetType());
            var newValues = new Dictionary<string, object> { { "_state", agentState } };

            return GetPopulatedUninitializedObject(agentSlot, fields, newValues);
        }

        /// <summary>
        ///     Creates a CommandWindow populated with necessary fields.
        /// </summary>
        /// <param name="currentTarget">The object we're interacting with, usually an abnormality or usable item.</param>
        /// <param name="currentWindowType">What type of command we're issuing (Management, suppression, usable item)</param>
        /// <param name="selectedWork">
        ///     Can be either a work type or null if we want to test the command window before a work type
        ///     is selected.
        /// </param>
        [NotNull]
        public static CommandWindow.CommandWindow CreateCommandWindow(UnitModel currentTarget, CommandType currentWindowType, RwbpType? selectedWork)
        {
            var convertedSelectedWork = selectedWork != null ? (long)selectedWork : -1L;

            // Need to populate an instance of SkillTypeList with our selected work because that's what the Command Window uses to check the work type.
            if (convertedSelectedWork > -1L)
            {
                var skillTypeInfos = new List<SkillTypeInfo> { new SkillTypeInfo { id = convertedSelectedWork } };
                _ = CreateSkillTypeList(skillTypeInfos);
            }

            CreateUninitializedObject<CommandWindow.CommandWindow>(out var commandWindow);
            var fields = GetUninitializedObjectFields(commandWindow.GetType());
            var newValues = new Dictionary<string, object> { { "_currentTarget", currentTarget }, { "_currentWindowType", currentWindowType }, { "_selectedWork", convertedSelectedWork } };

            return GetPopulatedUninitializedObject(commandWindow, fields, newValues);
        }

        [NotNull]
        public static CreatureEquipmentMakeInfo CreateCreatureEquipmentMakeInfo(string giftName)
        {
            var info = Substitute.For<CreatureEquipmentMakeInfo>();
            info.equipTypeInfo = new EquipmentTypeInfo { localizeData = new Dictionary<string, string> { { "name", giftName } }, type = EquipmentTypeInfo.EquipmentType.SPECIAL };

            LocalizeTextDataModel.instance?.Init(new Dictionary<string, string> { { giftName, giftName } });

            return info;
        }

        [NotNull]
        public static CreatureModel CreateCreatureModel(long instanceId, CreatureObserveInfoModel observeInfo, CreatureUnit unit)
        {
            CreateUninitializedObject<CreatureModel>(out var creatureModel);
            var fields = GetUninitializedObjectFields(creatureModel.GetType());
            var newValues = new Dictionary<string, object> { { "instanceId", instanceId }, { "observeInfo", observeInfo }, { "_unit", unit } };

            return GetPopulatedUninitializedObject(creatureModel, fields, newValues);
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
        public static CreatureOverloadManager CreateCreatureOverloadManager(int qliphothOverloadLevel)
        {
            CreateUninitializedObject<CreatureOverloadManager>(out var creatureOverloadManager);
            var fields = GetUninitializedObjectFields(creatureOverloadManager.GetType());
            var newValues = new Dictionary<string, object> { { "qliphothOverloadLevel", qliphothOverloadLevel } };

            return GetPopulatedUninitializedObject(creatureOverloadManager, fields, newValues);
        }

        [NotNull]
        public static CreatureTypeInfo CreateCreatureTypeInfo()
        {
            CreateUninitializedObject<CreatureTypeInfo>(out var creatureTypeInfo);

            return creatureTypeInfo;
        }

        [NotNull]
        public static CreatureUnit CreateCreatureUnit(IsolateRoom room)
        {
            CreateUninitializedObject<CreatureUnit>(out var creatureUnit);
            var fields = GetUninitializedObjectFields(creatureUnit.GetType());
            var newValues = new Dictionary<string, object> { { "room", room } };

            return GetPopulatedUninitializedObject(creatureUnit, fields, newValues);
        }

        [NotNull]
        public static EnergyModel CreateEnergyModel(GlobalGameManager globalGameManager, float energy)
        {
            // Requires a GlobalGameManager instance
            Guard.Against.Null(globalGameManager, nameof(globalGameManager));

            CreateUninitializedObject<EnergyModel>(out var energyModel);
            var fields = GetUninitializedObjectFields(energyModel.GetType());
            var newValues = new Dictionary<string, object> { { "energy", energy } };

            return GetPopulatedUninitializedObject(energyModel, fields, newValues);
        }

        [NotNull]
        public static GlobalGameManager CreateGlobalGameManager()
        {
            CreateUninitializedObject<GlobalGameManager>(out var globalGameManager);

            var fields = GetUninitializedObjectFields(globalGameManager.GetType());
            var newValues = new Dictionary<string, object> { { "_instance", globalGameManager } };

            var newGlobalGameManager = GetPopulatedUninitializedObject(globalGameManager, fields, newValues);
            newValues = new Dictionary<string, object> { { "_instance", newGlobalGameManager } };

            return GetPopulatedUninitializedObject(newGlobalGameManager, fields, newValues);
        }

        [NotNull]
        public static IsolateRoom CreateIsolateRoom(IsolateOverload overloadUI)
        {
            CreateUninitializedObject<IsolateRoom>(out var isolateRoom);
            var fields = GetUninitializedObjectFields(isolateRoom.GetType());
            var newValues = new Dictionary<string, object> { { "overloadUI", overloadUI } };

            return GetPopulatedUninitializedObject(isolateRoom, fields, newValues);
        }

        [NotNull]
        public static IsolateOverload CreateIsolateOverload(bool isActivated)
        {
            CreateUninitializedObject<IsolateOverload>(out var isolateOverload);
            var fields = GetUninitializedObjectFields(isolateOverload.GetType());
            var newValues = new Dictionary<string, object> { { "_isActivated", isActivated } };

            return GetPopulatedUninitializedObject(isolateOverload, fields, newValues);
        }

        [NotNull]
        public static PlayerModel CreatePlayerModel()
        {
            CreateUninitializedObject<PlayerModel>(out var playerModel);

            return playerModel;
        }

        [NotNull]
        private static SkillTypeList CreateSkillTypeList(List<SkillTypeInfo> skillTypeInfos)
        {
            CreateUninitializedObject<SkillTypeList>(out var skillTypeList);
            var fields = GetUninitializedObjectFields(skillTypeList.GetType());
            var newValues = new Dictionary<string, object> { { "_instance", skillTypeList }, { "_list", skillTypeInfos } };

            var newSkillTypeList = GetPopulatedUninitializedObject(skillTypeList, fields, newValues);
            newValues = new Dictionary<string, object> { { "_instance", newSkillTypeList } };

            return GetPopulatedUninitializedObject(newSkillTypeList, fields, newValues);
        }

        [NotNull]
        public static StageTypeInfo CreateStageTypeInfo(int[] energyVal)
        {
            CreateUninitializedObject<StageTypeInfo>(out var stageTypeInfo);
            stageTypeInfo.energyVal = energyVal;

            return stageTypeInfo;
        }

        [NotNull]
        public static UnitModel CreateUnitModel(long unitId)
        {
            CreateUninitializedObject<UnitModel>(out var unitModel);
            unitModel.instanceId = unitId;

            return unitModel;
        }

        [NotNull]
        public static UseSkill CreateUseSkill(string giftName, long agentId, int numberOfSuccesses)
        {
            var useSkill = Substitute.For<UseSkill>();
            CreateUninitializedObject(out useSkill.agent);
            useSkill.agent.instanceId = agentId;
            CreateUninitializedObject(out useSkill.targetCreature);
            useSkill.targetCreature.metaInfo = new CreatureTypeInfo { equipMakeInfos = new List<CreatureEquipmentMakeInfo> { CreateCreatureEquipmentMakeInfo(giftName) } };
            useSkill.successCount = numberOfSuccesses;

            return useSkill;
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
                catch
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
                if (newValues.ContainsKey(fields[i].Name))
                {
                    values[i] = newValues[fields[i].Name];
                }
            }

            FormatterServices.PopulateObjectMembers(newObj, fields.ToArray(), values);

            return newObj;
        }

        #endregion
    }
}
