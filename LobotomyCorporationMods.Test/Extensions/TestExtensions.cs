// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Harmony;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Interfaces;
using Moq;
using UnityEngine;
using ILogger = LobotomyCorporationMods.Common.Interfaces.ILogger;

#endregion

namespace LobotomyCorporationMods.Test.Extensions
{
    public static class TestExtensions
    {
        internal static AgentModel GetAgentWithGift(EquipmentId giftId = EquipmentId.None, IEnumerable<UnitBuf>? unitBuffs = null)
        {
            unitBuffs ??= new List<UnitBuf>();

            var agent = TestUnityExtensions.CreateAgentModel(bufList: unitBuffs.ToList());
            var gift = TestUnityExtensions.CreateEgoGiftModel();
            gift.metaInfo.id = (int)giftId;
            agent.Equipment.gifts.addedGifts.Add(gift);

            return agent;
        }

        internal static CreatureModel GetCreatureWithGift(CreatureIds creatureId = CreatureIds.OneSin, EquipmentId giftId = EquipmentId.None, int qliphothCounter = 0, bool maxObservation = true)
        {
            var equipmentTypeInfo = TestUnityExtensions.CreateEquipmentTypeInfo();
            equipmentTypeInfo.type = EquipmentTypeInfo.EquipmentType.SPECIAL;

            var creatureEquipmentMakeInfo = TestUnityExtensions.CreateCreatureEquipmentMakeInfo(equipmentTypeInfo);
            var creatureTypeInfo = TestUnityExtensions.CreateCreatureTypeInfo(new List<CreatureEquipmentMakeInfo> { creatureEquipmentMakeInfo });
            var creature = TestUnityExtensions.CreateCreatureModel(metaInfo: creatureTypeInfo, qliphothCounter: qliphothCounter);
            creature.instanceId = (long)creatureId;
            creature.metadataId = (long)creatureId;

            if (maxObservation)
            {
                SetMaxObservation(creature);
            }

            // Need to initialize the CreatureLayer with our new creature
            var creatureUnit = TestUnityExtensions.CreateCreatureUnit();
            TestUnityExtensions.CreateCreatureLayer(new Dictionary<long, CreatureUnit> { { (long)creatureId, creatureUnit } });

            return creature;
        }

        internal static Mock<IFileManager> GetMockFileManager()
        {
            var mockFileManager = new Mock<IFileManager>();
            _ = mockFileManager.Setup(static fm => fm.GetFile(It.IsAny<string>())).Returns(static (string fileName) => fileName.InCurrentDirectory());
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

        internal static CommandWindow.CommandWindow InitializeCommandWindow(UnitModel? currentTarget = null, RwbpType rwbpType = (RwbpType)1)
        {
            currentTarget ??= TestUnityExtensions.CreateCreatureModel();
            var deadAgentColor = Color.red;

            // Need existing game instances
            InitializeLocalizeTextDataModel();
            InitializeSkillTypeList(rwbpType);

            var commandWindow = TestUnityExtensions.CreateCommandWindow(currentTarget, CommandType.Management, (long)rwbpType);
            commandWindow.DeadColor = deadAgentColor;
            CommandWindow.CommandWindow.CurrentWindow.DeadColor = deadAgentColor;

            return commandWindow;
        }

        private static void InitializeLocalizeTextDataModel()
        {
            const string DeadAgentString = "AgentState_Dead";
            var list = new Dictionary<string, string> { { DeadAgentString, DeadAgentString } };

            _ = TestUnityExtensions.CreateLocalizeTextDataModel(list);
        }

        private static void InitializeSkillTypeList(RwbpType rwbpType)
        {
            SkillTypeInfo[] skillTypeInfos = { new() { id = (long)rwbpType } };
            _ = TestUnityExtensions.CreateSkillTypeList(skillTypeInfos);
        }

        private static void SetMaxObservation(CreatureModel creature)
        {
            var observeRegions = new List<ObserveInfoData>
            {
                new() { regionName = "stat" },
                new() { regionName = "defense" },
                new() { regionName = "work_r" },
                new() { regionName = "work_w" },
                new() { regionName = "work_b" },
                new() { regionName = "work_p" }
            };
            creature.observeInfo.InitObserveRegion(observeRegions);
            creature.observeInfo.ObserveAll();
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
            action.Should().Throw<TException>();
            mockLogger.Verify(static logger => logger.WriteToLog(It.IsAny<TException>()), Times.Exactly(numberOfTimes));
        }
    }
}
