// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Test.Parameters;
using Moq;
using UnityEngine;
using ILogger = LobotomyCorporationMods.Common.Interfaces.ILogger;

#endregion

// ReSharper disable MemberCanBePrivate.Global
namespace LobotomyCorporationMods.Test.Extensions
{
    internal static class TestExtensions
    {
        [NotNull]
        internal static AgentModel GetAgentWithGift(EquipmentIds giftId = EquipmentIds.None,
            EGOgiftAttachRegion attachPosition = EGOgiftAttachRegion.HEAD,
            EGOgiftAttachType attachType = 0,
            IEnumerable<UnitBuf> unitBuffs = null)
        {
            unitBuffs = unitBuffs.EnsureNotNullWithMethod(() => new List<UnitBuf>());

            var agentModelCreationParameters = new AgentModelCreationParameters
            {
                BufList = unitBuffs.ToList(),
            };

            var agent = UnityTestExtensions.CreateAgentModel(agentModelCreationParameters);
            var gift = UnityTestExtensions.CreateEgoGiftModel();
            gift.metaInfo.id = (int)giftId;
            gift.metaInfo.attachPos = attachPosition.ToString();
            gift.metaInfo.attachType = attachType;
            agent.Equipment.gifts.addedGifts.Add(gift);

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
            var creatureTypeInfo = UnityTestExtensions.CreateCreatureTypeInfo(new List<CreatureEquipmentMakeInfo>
            {
                creatureEquipmentMakeInfo,
            });

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
            var mockFileManager = new Mock<IFileManager>();
            _ = mockFileManager.Setup(fm => fm.GetFile(It.IsAny<string>())).Returns((string fileName) => fileName.InCurrentDirectory());
            _ = mockFileManager.Setup(fm => fm.ReadAllText(It.IsAny<string>(), It.IsAny<bool>())).Returns((string fileName,
                bool _) => File.ReadAllText(fileName.InCurrentDirectory()));
            _ = mockFileManager.Setup(fm => fm.WriteAllText(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Callback<string, string, bool>((path,
                contents,
                append) =>
            {
                var directory = Path.GetDirectoryName(path);
                if (directory.IsNotNull() && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (append)
                {
                    File.AppendAllText(path, contents);
                }
                else
                {
                    File.WriteAllText(path, contents);
                }
            });

            return mockFileManager;
        }

        [NotNull]
        internal static Mock<ILogger> GetMockLogger()
        {
            var mockLogger = new Mock<ILogger>();

            return mockLogger;
        }

        [NotNull]
        internal static string InCurrentDirectory([NotNull] this string fileName)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), fileName);
        }

        [NotNull]
        internal static CommandWindow.CommandWindow InitializeCommandWindowWithAbnormality([CanBeNull] UnitModel currentTarget = null,
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
            var list = new Dictionary<string, string>
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
            {
                new SkillTypeInfo
                {
                    id = (long)rwbpType,
                },
            };

            _ = UnityTestExtensions.CreateSkillTypeList(skillTypeInfos);
        }

        private static void SetMaxObservation([NotNull] CreatureModel creature)
        {
            var observeRegions = new List<ObserveInfoData>
            {
                new ObserveInfoData
                {
                    regionName = "stat",
                },
                new ObserveInfoData
                {
                    regionName = "defense",
                },
                new ObserveInfoData
                {
                    regionName = "work_r",
                },
                new ObserveInfoData
                {
                    regionName = "work_w",
                },
                new ObserveInfoData
                {
                    regionName = "work_b",
                },
                new ObserveInfoData
                {
                    regionName = "work_p",
                },
            };

            creature.observeInfo.InitObserveRegion(observeRegions);
            creature.observeInfo.ObserveAll();
        }

        internal static void ValidateHarmonyPatch([NotNull] this MemberInfo patchClass,
            Type originalClass,
            string methodName)
        {
            var attribute = Attribute.GetCustomAttribute(patchClass, typeof(HarmonyPatch)) as HarmonyPatch;

            attribute.Should().NotBeNull();
            attribute?.info.originalType.Should().Be(originalClass);
            attribute?.info.methodName.Should().Be(methodName);
        }

        internal static void VerifyArgumentNullException([NotNull] this Mock<ILogger> mockLogger,
            Action action,
            Times? numberOfTimes = null)
        {
            action.Should().Throw<ArgumentNullException>();
            mockLogger.Verify(logger => logger.WriteException(It.IsAny<ArgumentNullException>()), numberOfTimes ?? Times.Once());
        }

        internal static void VerifyNullReferenceException([NotNull] this Mock<ILogger> mockLogger,
            Action action,
            Times? numberOfTimes = null)
        {
            action.Should().Throw<NullReferenceException>();
            mockLogger.Verify(logger => logger.WriteException(It.IsAny<NullReferenceException>()), numberOfTimes ?? Times.Once());
        }
    }
}
