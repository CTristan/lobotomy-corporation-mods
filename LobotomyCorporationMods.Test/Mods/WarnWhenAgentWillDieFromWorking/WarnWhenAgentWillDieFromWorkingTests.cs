// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Linq;
using CommandWindow;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Test.Extensions;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Patches;
using Moq;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Test.Mods.WarnWhenAgentWillDieFromWorking
{
    public class WarnWhenAgentWillDieFromWorkingTests
    {
        private const string DeadAgentString = "AgentState_Dead";
        protected const AgentState IdleAgentState = AgentState.IDLE;
        protected const RwbpType SkillTypeAttachment = RwbpType.B;
        protected const RwbpType SkillTypeInsight = RwbpType.W;
        protected const RwbpType SkillTypeRepression = RwbpType.P;
        protected const int StatLevelFive = 85;
        protected const int StatLevelFour = 65;
        protected const int StatLevelOne = 1;
        protected const int StatLevelThree = 45;
        protected const int StatLevelTwo = 30;

        protected WarnWhenAgentWillDieFromWorkingTests()
        {
            _ = new Harmony_Patch();
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);
        }

        private Color DeadAgentColor { get; } = Color.red;
        protected GameManager GameManager { get; } = UnityTestExtensions.CreateGameManager();
        protected Mock<IImageAdapter> MockImageAdapter { get; } = new Mock<IImageAdapter>();
        protected Mock<ITextAdapter> MockTextAdapter { get; } = new Mock<ITextAdapter>();
        protected Mock<IYggdrasilAnimAdapter> MockYggdrasilAnimAdapter { get; } = new Mock<IYggdrasilAnimAdapter>();

        protected Mock<IBeautyBeastAnimAdapter> MockBeautyBeastAnimAdapter { get; } = new Mock<IBeautyBeastAnimAdapter>();

        protected bool AgentWillDie([NotNull] IImageAdapter workFilterFill,
            [NotNull] ITextAdapter workFilterText)
        {
            Guard.Against.Null(workFilterFill, nameof(workFilterFill));
            Guard.Against.Null(workFilterText, nameof(workFilterText));
            var agentWillDie = workFilterFill.Color == DeadAgentColor && workFilterText.Text == DeadAgentString;

            return agentWillDie;
        }

        [NotNull]
        private static AgentModel GetAgentWithGift(EquipmentId giftId = EquipmentId.None,
            IEnumerable<UnitBuf> unitBuffs = null)
        {
            unitBuffs = unitBuffs ?? new List<UnitBuf>();

            var agent = UnityTestExtensions.CreateAgentModel(bufList: unitBuffs.ToList());
            var gift = UnityTestExtensions.CreateEgoGiftModel();
            gift.metaInfo.id = (int)giftId;
            agent.Equipment.gifts.addedGifts.Add(gift);

            return agent;
        }

        [NotNull]
        private static CreatureModel GetCreatureWithGift(CreatureIds creatureId = CreatureIds.OneSin,
            int qliphothCounter = 0,
            bool maxObservation = true)
        {
            var equipmentTypeInfo = UnityTestExtensions.CreateEquipmentTypeInfo();
            equipmentTypeInfo.type = EquipmentTypeInfo.EquipmentType.SPECIAL;

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
        protected AgentSlot InitializeAgentSlot(CreatureIds creatureId,
            IEnumerable<UnitBuf> buffList = null,
            EquipmentId giftId = (EquipmentId)1,
            RwbpType skillType = (RwbpType)1,
            int qliphothCounter = 0)
        {
            buffList = buffList ?? new List<UnitBuf>();
            var creature = GetCreatureWithGift(creatureId, qliphothCounter);
            _ = InitializeCommandWindow(creature, skillType);
            var agent = GetAgentWithGift(giftId, buffList);

            return UnityTestExtensions.CreateAgentSlot(currentAgent: agent);
        }

        [NotNull]
        protected CommandWindow.CommandWindow InitializeCommandWindow([CanBeNull] UnitModel currentTarget = null,
            RwbpType rwbpType = (RwbpType)1)
        {
            currentTarget = currentTarget ?? UnityTestExtensions.CreateCreatureModel();

            // Need existing game instances
            InitializeLocalizeTextDataModel();
            InitializeSkillTypeList(rwbpType);

            var commandWindow = UnityTestExtensions.CreateCommandWindow(currentTarget, CommandType.Management, (long)rwbpType);
            commandWindow.DeadColor = DeadAgentColor;
            CommandWindow.CommandWindow.CurrentWindow.DeadColor = DeadAgentColor;

            return commandWindow;
        }

        private static void InitializeLocalizeTextDataModel()
        {
            var list = new Dictionary<string, string>
            {
                {
                    DeadAgentString, DeadAgentString
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

        protected static void SetupNothingThere([NotNull] AgentSlot agentSlot,
            int fortitude,
            bool isDisguised = false)
        {
            Guard.Against.Null(agentSlot, nameof(agentSlot));
            agentSlot.CurrentAgent.primaryStat.hp = fortitude;

            var creature = (CreatureModel)CommandWindow.CommandWindow.CurrentWindow.CurrentTarget;
            creature.script = new Nothing();
            ((Nothing)creature.script).copiedWorker = isDisguised ? UnityTestExtensions.CreateAgentModel() : null;
        }

        protected void SetupParasiteTree(int numberOfFlowers)
        {
            var mockFlower = new Mock<IGameObjectAdapter>();
            mockFlower.Setup(adapter => adapter.ActiveSelf).Returns(true);

            var mockFlowers = new List<IGameObjectAdapter>();
            for (var i = 0; i < numberOfFlowers; i++)
            {
                mockFlowers.Add(mockFlower.Object);
            }

            MockYggdrasilAnimAdapter.Setup(adapter => adapter.Flowers).Returns(mockFlowers);
        }

        protected void VerifyAgentWillDie([NotNull] AgentSlot agentSlot)
        {
            agentSlot.PatchAfterSetFilter(IdleAgentState, GameManager, MockBeautyBeastAnimAdapter.Object, MockImageAdapter.Object, MockTextAdapter.Object, MockYggdrasilAnimAdapter.Object);

            AgentWillDie(MockImageAdapter.Object, MockTextAdapter.Object).Should().BeTrue();
        }

        protected void VerifyAgentWillNotDie([NotNull] AgentSlot agentSlot)
        {
            agentSlot.PatchAfterSetFilter(IdleAgentState, GameManager, MockBeautyBeastAnimAdapter.Object, MockImageAdapter.Object, MockTextAdapter.Object, MockYggdrasilAnimAdapter.Object);

            AgentWillDie(MockImageAdapter.Object, MockTextAdapter.Object).Should().BeFalse();
        }
    }
}
