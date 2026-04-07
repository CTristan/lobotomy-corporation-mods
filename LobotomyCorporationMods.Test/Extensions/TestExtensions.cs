// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AwesomeAssertions;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common.Enums;
using LobotomyCorporation.Mods.Common.Extensions;
using LobotomyCorporation.Mods.Common.Interfaces;
using LobotomyCorporationMods.Test.Parameters;
using Moq;
using UnityEngine;
using ILogger = LobotomyCorporation.Mods.Common.Interfaces.ILogger;

#endregion

// ReSharper disable MemberCanBePrivate.Global
namespace LobotomyCorporationMods.Test.Extensions
{
    public static class TestExtensions
    {
        [NotNull]
        internal static AgentModel GetAgentWithGift(EquipmentIds giftId = EquipmentIds.None,
            EGOgiftAttachRegion attachPosition = EGOgiftAttachRegion.HEAD,
            EGOgiftAttachType attachType = 0,
            IEnumerable<UnitBuf>? unitBuffs = null)
        {
            unitBuffs = unitBuffs.EnsureNotNullWithMethod(() => []);

            AgentModelCreationParameters agentModelCreationParameters = new()
            {
                BufList = [.. unitBuffs!],
            };

            var agent = UnityTestExtensions.CreateAgentModel(agentModelCreationParameters);
            var gift = UnityTestExtensions.CreateEgoGiftModel();
            gift.metaInfo.id = (int)giftId;
            gift.metaInfo.attachPos = attachPosition.ToString();
            gift.metaInfo.attachType = attachType;
            agent.Equipment.gifts!.addedGifts!.Add(gift);

            return agent;
        }

        [NotNull]
        internal static CreatureModel GetCreatureWithGift(CreatureIds creatureId = CreatureIds.OneSin,
            EquipmentIds giftId = EquipmentIds.None,
            EGOgiftAttachRegion attachPosition = EGOgiftAttachRegion.HEAD,
            EGOgiftAttachType giftAttachType = 0,
            int qliphothCounter = 0,
            bool maxObservation = true)
        {
            var equipmentTypeInfo = UnityTestExtensions.CreateEquipmentTypeInfo();
            equipmentTypeInfo.id = (int)giftId;
            equipmentTypeInfo.type = EquipmentTypeInfo.EquipmentType.SPECIAL;
            equipmentTypeInfo.attachPos = attachPosition.ToString();
            equipmentTypeInfo.attachType = giftAttachType;

            var creatureEquipmentMakeInfo = UnityTestExtensions.CreateCreatureEquipmentMakeInfo(equipmentTypeInfo);
            var creatureTypeInfo = UnityTestExtensions.CreateCreatureTypeInfo(
            [
                creatureEquipmentMakeInfo,
            ]);

            var creature = UnityTestExtensions.CreateCreatureModel(metaInfo: creatureTypeInfo, qliphothCounter: qliphothCounter);
            creature.instanceId = (long)creatureId;
            creature.metadataId = (long)creatureId;

            if (maxObservation)
            {
                SetMaxObservation(creature);
            }

            // Need to initialize the CreatureLayer with our new creature
            var creatureUnit = UnityTestExtensions.CreateCreatureUnit();
            _ = UnityTestExtensions.CreateCreatureLayer(new Dictionary<long, CreatureUnit>
            {
                {
                    (long)creatureId, creatureUnit
                },
            });

            return creature;
        }

        [NotNull]
        internal static Mock<IFileManager> GetMockFileManager()
        {
            Mock<IFileManager> mockFileManager = new();
            _ = mockFileManager.Setup(fm => fm.GetFile(It.IsAny<string>())).Returns((string fileName) => fileName.InCurrentDirectory());
            _ = mockFileManager.Setup(fm => fm.ReadAllText(It.IsAny<string>(), It.IsAny<bool>())).Returns((string fileName,
                bool _) => File.ReadAllText(fileName.InCurrentDirectory()));

            _ = mockFileManager.Setup(fm => fm.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((path,
                contents) =>
            {
                var directory = Path.GetDirectoryName(path);
                if (directory.IsNotNull() && !Directory.Exists(directory))
                {
                    _ = Directory.CreateDirectory(directory!);
                }

                File.WriteAllText(path, contents);
            });

            return mockFileManager;
        }

        [NotNull]
        internal static Mock<ILogger> GetMockLogger()
        {
            Mock<ILogger> mockLogger = new();

            return mockLogger;
        }

        [NotNull]
        internal static string InCurrentDirectory([NotNull] this string fileName)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), fileName);
        }

        [NotNull]
        internal static CommandWindow.CommandWindow InitializeCommandWindowWithAbnormality([CanBeNull] UnitModel? currentTarget = null,
            RwbpType rwbpType = (RwbpType)1,
            [NotNull] string textValue = "")
        {
            currentTarget = currentTarget.EnsureNotNullWithMethod(() => UnityTestExtensions.CreateCreatureModel());

            var deadAgentColor = Color.red;

            // Need existing game instances
            InitializeLocalizeTextDataModel(textValue);
            InitializeSkillTypeList(rwbpType);

            var commandWindow = UnityTestExtensions.CreateCommandWindow(currentTarget, CommandType.Management, (long)rwbpType);
            commandWindow.DeadColor = deadAgentColor;
            CommandWindow.CommandWindow.CurrentWindow.DeadColor = deadAgentColor;

            return commandWindow;
        }

        private static void InitializeLocalizeTextDataModel([NotNull] string value)
        {
            Dictionary<string, string> list = new()
            {
                {
                    value, value
                },
            };

            _ = UnityTestExtensions.CreateLocalizeTextDataModel(list);
        }

        private static void InitializeSkillTypeList(RwbpType rwbpType)
        {
            SkillTypeInfo[] skillTypeInfos =
            [
                new SkillTypeInfo
                {
                    id = (long)rwbpType,
                },
            ];

            _ = UnityTestExtensions.CreateSkillTypeList(skillTypeInfos);
        }

        private static void SetMaxObservation([NotNull] CreatureModel creature)
        {
            List<ObserveInfoData> observeRegions =
            [
                new() {
                    regionName = "stat",
                },
                new() {
                    regionName = "defense",
                },
                new() {
                    regionName = "work_r",
                },
                new() {
                    regionName = "work_w",
                },
                new() {
                    regionName = "work_b",
                },
                new() {
                    regionName = "work_p",
                },
            ];

            creature.observeInfo.InitObserveRegion(observeRegions);
            creature.observeInfo.ObserveAll();
        }

        internal static void ValidateHarmonyPatch([NotNull] this MemberInfo patchClass,
            Type originalClass,
            string methodName)
        {
            // Use reflection to access HarmonyPatch attributes since we can't reference 0Harmony.dll in net10.0
            var declaringAssembly = patchClass.DeclaringType?.Assembly
                ?? (patchClass is Type t ? t.Assembly : null);

            _ = declaringAssembly.Should().NotBeNull("Unable to determine assembly from MemberInfo");

            // Search all loaded assemblies to find HarmonyPatch attribute type
            var harmonyPatchAttributeType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("HarmonyLib.HarmonyPatch") ?? a.GetType("Harmony.HarmonyPatch"))
                .FirstOrDefault(t => t != null);

            _ = harmonyPatchAttributeType.Should().NotBeNull("HarmonyPatch attribute type not found in loaded assemblies");

            Attribute? attribute = Attribute.GetCustomAttribute(patchClass, harmonyPatchAttributeType);
            _ = attribute.Should().NotBeNull("HarmonyPatch attribute not found on member");

            // Harmony 1.x stores patch info in a public 'info' field of type HarmonyMethod
            var infoField = harmonyPatchAttributeType?.GetField("info", BindingFlags.Public | BindingFlags.Instance);
            _ = infoField.Should().NotBeNull("HarmonyPatch info field not found");

            var info = infoField?.GetValue(attribute);
            _ = info.Should().NotBeNull("HarmonyPatch info is null");

            // HarmonyMethod has public originalType and methodName fields
            var originalTypeField = info?.GetType().GetField("originalType", BindingFlags.Public | BindingFlags.Instance);
            var methodNameField = info?.GetType().GetField("methodName", BindingFlags.Public | BindingFlags.Instance);

            _ = originalTypeField.Should().NotBeNull("HarmonyMethod originalType field not found");
            _ = methodNameField.Should().NotBeNull("HarmonyMethod methodName field not found");

            Type? originalTypeValue = originalTypeField?.GetValue(info) as Type;
            var methodNameValue = methodNameField?.GetValue(info) as string;

            _ = originalTypeValue.Should().Be(originalClass);
            _ = methodNameValue.Should().Be(methodName);
        }

        internal static void VerifyArgumentNullException([NotNull] this Mock<ILogger> mockLogger,
            Action action,
            Times? numberOfTimes = null)
        {
            _ = action.Should().Throw<ArgumentNullException>();
            mockLogger.Verify(logger => logger.WriteException(It.IsAny<ArgumentNullException>()), numberOfTimes ?? Times.Once());
        }

        internal static void VerifyNullReferenceException([NotNull] this Mock<ILogger> mockLogger,
            Action action,
            Times? numberOfTimes = null)
        {
            _ = action.Should().Throw<NullReferenceException>();
            mockLogger.Verify(logger => logger.WriteException(It.IsAny<NullReferenceException>()), numberOfTimes ?? Times.Once());
        }
    }
}
