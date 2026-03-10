// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using AwesomeAssertions;
using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using LobotomyCorporationMods.Test.Extensions;
using LobotomyCorporationMods.Test.Parameters;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Patches;
using Moq;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.WarnWhenAgentWillDieFromWorkingTests
{
    public class WarnWhenAgentWillDieFromWorkingModTests
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

        protected WarnWhenAgentWillDieFromWorkingModTests()
        {
            _ = new Harmony_Patch();
            Mock<Common.Interfaces.ILogger> mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);
        }

        private Color DeadAgentColor { get; } = Color.red;
        protected GameManager GameManager { get; } = UnityTestExtensions.CreateGameManager();
        protected Mock<IImageTestAdapter> MockImageTestAdapter { get; } = new Mock<IImageTestAdapter>();
        protected Mock<ITextTestAdapter> MockTextTestAdapter { get; } = new Mock<ITextTestAdapter>();
        protected Mock<IYggdrasilAnimTestAdapter> MockYggdrasilAnimTestAdapter { get; } = new Mock<IYggdrasilAnimTestAdapter>();
        protected Mock<IBeautyBeastAnimTestAdapter> MockBeautyBeastAnimTestAdapter { get; } = new Mock<IBeautyBeastAnimTestAdapter>();

        protected bool AgentWillDie([NotNull] IImageTestAdapter workFilterFill,
            [NotNull] ITextTestAdapter workFilterTextTest)
        {
            _ = Guard.Against.Null(workFilterFill, nameof(workFilterFill));
            _ = Guard.Against.Null(workFilterTextTest, nameof(workFilterTextTest));
            bool agentWillDie = workFilterFill.Color == DeadAgentColor && workFilterTextTest.Text == DeadAgentString;

            return agentWillDie;
        }

        [NotNull]
        private static AgentModel GetAgentWithGift(EquipmentIds giftIds = EquipmentIds.None,
            IEnumerable<UnitBuf>? unitBuffs = null)
        {
            unitBuffs = unitBuffs.EnsureNotNullWithMethod(() => []);

            AgentModelCreationParameters agentModelCreationParameters = new()
            {
                BufList = [.. unitBuffs],
            };

            AgentModel agent = UnityTestExtensions.CreateAgentModel(agentModelCreationParameters);
            EGOgiftModel gift = UnityTestExtensions.CreateEgoGiftModel();
            gift.metaInfo.id = (int)giftIds;
            agent.Equipment.gifts.addedGifts.Add(gift);

            return agent;
        }

        [NotNull]
        protected static AgentSlot InitializeAgentSlot(CreatureIds creatureId,
            IEnumerable<UnitBuf>? buffList = null,
            EquipmentIds giftIds = (EquipmentIds)1,
            RwbpType skillType = (RwbpType)1,
            int qliphothCounter = 0)
        {
            buffList = buffList.EnsureNotNullWithMethod(() => []);

            CreatureModel creature = TestExtensions.GetCreatureWithGift(creatureId, qliphothCounter: qliphothCounter);
            _ = TestExtensions.InitializeCommandWindowWithAbnormality(creature, skillType, DeadAgentString);
            AgentModel agent = GetAgentWithGift(giftIds, buffList);

            return UnityTestExtensions.CreateAgentSlot(currentAgent: agent);
        }

        protected static void SetupNothingThere([NotNull] AgentSlot agentSlot,
            int fortitude,
            bool isDisguised = false)
        {
            _ = Guard.Against.Null(agentSlot, nameof(agentSlot));
            agentSlot.CurrentAgent.primaryStat.hp = fortitude;

            CreatureModel creature = (CreatureModel)CommandWindow.CommandWindow.CurrentWindow.CurrentTarget;
            creature.script = new Nothing();
            ((Nothing)creature.script).copiedWorker = isDisguised ? UnityTestExtensions.CreateAgentModel() : null;
        }

        protected void SetupParasiteTree(int numberOfFlowers)
        {
            Mock<IGameObjectTestAdapter> mockFlower = new();
            _ = mockFlower.Setup(adapter => adapter.ActiveSelf).Returns(true);

            List<IGameObjectTestAdapter> mockFlowers = [];
            for (int i = 0; i < numberOfFlowers; i++)
            {
                mockFlowers.Add(mockFlower.Object);
            }

            _ = MockYggdrasilAnimTestAdapter.Setup(adapter => adapter.Flowers).Returns(mockFlowers);
        }

        protected void VerifyAgentWillDie([NotNull] AgentSlot agentSlot)
        {
            agentSlot.PatchAfterSetFilter(IdleAgentState, GameManager, MockBeautyBeastAnimTestAdapter.Object, MockImageTestAdapter.Object, MockTextTestAdapter.Object,
                MockYggdrasilAnimTestAdapter.Object);

            _ = AgentWillDie(MockImageTestAdapter.Object, MockTextTestAdapter.Object).Should().BeTrue();
        }

        protected void VerifyAgentWillNotDie([NotNull] AgentSlot agentSlot)
        {
            agentSlot.PatchAfterSetFilter(IdleAgentState, GameManager, MockBeautyBeastAnimTestAdapter.Object, MockImageTestAdapter.Object, MockTextTestAdapter.Object,
                MockYggdrasilAnimTestAdapter.Object);

            _ = AgentWillDie(MockImageTestAdapter.Object, MockTextTestAdapter.Object).Should().BeFalse();
        }
    }
}
