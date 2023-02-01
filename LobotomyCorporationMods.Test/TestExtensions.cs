// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using Customizing;
using FluentAssertions;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
using NSubstitute;
using UnityEngine;
using WorkerSprite;

namespace LobotomyCorporationMods.Test
{
    [SuppressMessage("ReSharper", "Unity.IncorrectMonoBehaviourInstantiation")]
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
        public static AgentModel CreateAgentModel(AgentName agentName, UnitEquipSpace equipment, long instanceId, string name, WorkerSprite.WorkerSprite spriteData)
        {
            CreateUninitializedObject<AgentModel>(out var agentModel);

            var fields = GetUninitializedObjectFields(agentModel.GetType());
            var newValues = new Dictionary<string, object>
            {
                { "_agentName", agentName },
                { "_equipment", equipment },
                { "instanceId", instanceId },
                { "name", name },
                { "spriteData", spriteData }
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
        public static CreatureEquipmentMakeInfo CreateCreatureEquipmentMakeInfo(EquipmentTypeInfo equipTypeInfo)
        {
            return new CreatureEquipmentMakeInfo { equipTypeInfo = equipTypeInfo };
        }

        [NotNull]
        public static CreatureModel CreateCreatureModel(AgentModel agent, SkillTypeInfo skillTypeInfo, CreatureTypeInfo metaInfo)
        {
            CreateUninitializedObject<CreatureModel>(out var creatureModel);

            var fields = GetUninitializedObjectFields(creatureModel.GetType());
            var newValues = new Dictionary<string, object> { { "metaInfo", metaInfo } };
            var newCreatureModel = GetPopulatedUninitializedObject(creatureModel, fields, newValues);

            // Needed to avoid a circular reference from currentSkill
            var currentSkill = CreateUseSkill(agent, skillTypeInfo, newCreatureModel);
            newValues.Add("_currentSkill", currentSkill);

            return GetPopulatedUninitializedObject(newCreatureModel, fields, newValues);
        }

        [NotNull]
        public static CreatureTypeInfo CreateCreatureTypeInfo(List<CreatureEquipmentMakeInfo> equipMakeInfos)
        {
            return new CreatureTypeInfo { equipMakeInfos = equipMakeInfos };
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
        public static EGOgiftModel CreateEgoGiftModel(EquipmentTypeInfo metaInfo)
        {
            return new EGOgiftModel { metaInfo = metaInfo };
        }

        [NotNull]
        public static EquipmentTypeInfo CreateEquipmentTypeInfo()
        {
            return new EquipmentTypeInfo();
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
        public static SkillTypeInfo CreateSkillTypeInfo()
        {
            return new SkillTypeInfo();
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
        public static UnitEquipSpace CreateUnitEquipSpace()
        {
            return new UnitEquipSpace();
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
        public static WorkerSprite.WorkerSprite CreateWorkerSprite()
        {
            return new WorkerSprite.WorkerSprite();
        }

        [NotNull]
        public static WorkerSpriteManager CreateWorkerSpriteManager(WorkerBasicSpriteController basicData)
        {
            CreateUninitializedObject<WorkerSpriteManager>(out var workerSpriteManager);

            var fields = GetUninitializedObjectFields(workerSpriteManager.GetType());
            var newValues = new Dictionary<string, object> { { "_instance", workerSpriteManager }, { "basicData", basicData } };

            var newWorkerSpriteManager = GetPopulatedUninitializedObject(workerSpriteManager, fields, newValues);
            newValues = new Dictionary<string, object> { { "_instance", newWorkerSpriteManager }, { "basicData", basicData } };

            return GetPopulatedUninitializedObject(newWorkerSpriteManager, fields, newValues);
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
