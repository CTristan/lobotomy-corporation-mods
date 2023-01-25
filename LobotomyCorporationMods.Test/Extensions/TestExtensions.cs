// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Customizing;
using FluentAssertions;
using Harmony;
using JetBrains.Annotations;
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

        public static void ValidateHarmonyPatch([NotNull] this MemberInfo patchClass, [NotNull] Type originalClass, string methodName)
        {
            var attribute = Attribute.GetCustomAttribute(patchClass, typeof(HarmonyPatch)) as HarmonyPatch;

            attribute.Should().NotBeNull();
            attribute?.info.originalType.Should().Be(originalClass);
            attribute?.info.methodName.Should().Be(methodName);
        }

        #region Unity Objects

        [NotNull]
        public static CreatureEquipmentMakeInfo CreateCreatureEquipmentMakeInfo(string giftName)
        {
            var info = Substitute.For<CreatureEquipmentMakeInfo>();
            info.equipTypeInfo = new EquipmentTypeInfo { localizeData = new Dictionary<string, string> { { "name", giftName } }, type = EquipmentTypeInfo.EquipmentType.SPECIAL };

            LocalizeTextDataModel.instance?.Init(new Dictionary<string, string> { { giftName, giftName } });

            return info;
        }

        [NotNull]
        public static CustomizingWindow CreateCustomizingWindow()
        {
            CreateUninitializedObject<CustomizingWindow>(out var customizingWindow);

            return customizingWindow;
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
