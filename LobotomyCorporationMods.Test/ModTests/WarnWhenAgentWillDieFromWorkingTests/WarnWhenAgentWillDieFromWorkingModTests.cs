// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common.Enums;
using LobotomyCorporation.Mods.Common.Extensions;
using LobotomyCorporation.Mods.Common.Implementations;
using LobotomyCorporation.Mods.Common.Interfaces.Adapters;
using LobotomyCorporation.Mods.Common.Interfaces.Adapters.BaseClasses;
using LobotomyCorporationMods.Test.Extensions;
using LobotomyCorporationMods.Test.Parameters;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Patches;
using Moq;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.WarnWhenAgentWillDieFromWorkingTests
{
    public class WarnWhenAgentWillDieFromWorkingModTests : IDisposable
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
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);
        }

        private Color DeadAgentColor { get; } = Color.red;
        protected GameManager GameManager { get; } = UnityTestExtensions.CreateGameManager();
        protected Mock<IImageTestAdapter> MockImageTestAdapter { get; } =
            new Mock<IImageTestAdapter>();
        protected Mock<ITextTestAdapter> MockTextTestAdapter { get; } =
            new Mock<ITextTestAdapter>();
        protected Mock<IYggdrasilAnimTestAdapter> MockYggdrasilAnimTestAdapter { get; } =
            new Mock<IYggdrasilAnimTestAdapter>();
        protected Mock<IBeautyBeastAnimTestAdapter> MockBeautyBeastAnimTestAdapter { get; } =
            new Mock<IBeautyBeastAnimTestAdapter>();

        protected bool AgentWillDie(
            [NotNull] IImageTestAdapter workFilterFill,
            [NotNull] ITextTestAdapter workFilterTextTest
        )
        {
            ThrowHelper.ThrowIfNull(workFilterFill, nameof(workFilterFill));
            ThrowHelper.ThrowIfNull(workFilterTextTest, nameof(workFilterTextTest));
            var agentWillDie =
                workFilterFill.Color == DeadAgentColor
                && workFilterTextTest.Text == DeadAgentString;

            return agentWillDie;
        }

        [NotNull]
        private static AgentModel GetAgentWithGift(
            EquipmentIds giftIds = EquipmentIds.None,
            IEnumerable<UnitBuf> unitBuffs = null
        )
        {
            unitBuffs = unitBuffs.EnsureNotNullWithMethod(() => new List<UnitBuf>());

            var agentModelCreationParameters = new AgentModelCreationParameters
            {
                BufList = unitBuffs.ToList(),
            };

            var agent = UnityTestExtensions.CreateAgentModel(agentModelCreationParameters);
            var gift = UnityTestExtensions.CreateEgoGiftModel();
            gift.metaInfo.id = (int)giftIds;
            agent.Equipment.gifts.addedGifts.Add(gift);

            return agent;
        }

        [NotNull]
        protected static AgentSlot InitializeAgentSlot(
            CreatureIds creatureId,
            IEnumerable<UnitBuf> buffList = null,
            EquipmentIds giftIds = (EquipmentIds)1,
            RwbpType skillType = (RwbpType)1,
            int qliphothCounter = 0
        )
        {
            buffList = buffList.EnsureNotNullWithMethod(() => new List<UnitBuf>());

            var creature = TestExtensions.GetCreatureWithGift(
                creatureId,
                qliphothCounter: qliphothCounter
            );
            _ = TestExtensions.InitializeCommandWindowWithAbnormality(
                creature,
                skillType,
                DeadAgentString
            );
            var agent = GetAgentWithGift(giftIds, buffList);

            return UnityTestExtensions.CreateAgentSlot(currentAgent: agent);
        }

        protected static void SetupNothingThere(
            [NotNull] AgentSlot agentSlot,
            int fortitude,
            bool isDisguised = false
        )
        {
            ThrowHelper.ThrowIfNull(agentSlot, nameof(agentSlot));
            agentSlot.CurrentAgent.primaryStat.hp = fortitude;

            var creature = (CreatureModel)CommandWindow.CommandWindow.CurrentWindow.CurrentTarget;
            creature.script = new Nothing();
            ((Nothing)creature.script).copiedWorker = isDisguised
                ? UnityTestExtensions.CreateAgentModel()
                : null;
        }

        protected void SetupParasiteTree(int numberOfFlowers)
        {
            var mockFlower = new Mock<IGameObjectTestAdapter>();
            mockFlower.Setup(adapter => adapter.ActiveSelf).Returns(true);

            var mockFlowers = new List<IGameObjectTestAdapter>();
            for (var i = 0; i < numberOfFlowers; i++)
            {
                mockFlowers.Add(mockFlower.Object);
            }

            MockYggdrasilAnimTestAdapter.Setup(adapter => adapter.Flowers).Returns(mockFlowers);
        }

        protected void VerifyAgentWillDie([NotNull] AgentSlot agentSlot)
        {
            agentSlot.PatchAfterSetFilter(
                IdleAgentState,
                GameManager,
                MockBeautyBeastAnimTestAdapter.Object,
                MockImageTestAdapter.Object,
                MockTextTestAdapter.Object,
                MockYggdrasilAnimTestAdapter.Object
            );

            AgentWillDie(MockImageTestAdapter.Object, MockTextTestAdapter.Object).Should().BeTrue();
        }

        protected void VerifyAgentWillNotDie([NotNull] AgentSlot agentSlot)
        {
            agentSlot.PatchAfterSetFilter(
                IdleAgentState,
                GameManager,
                MockBeautyBeastAnimTestAdapter.Object,
                MockImageTestAdapter.Object,
                MockTextTestAdapter.Object,
                MockYggdrasilAnimTestAdapter.Object
            );

            AgentWillDie(MockImageTestAdapter.Object, MockTextTestAdapter.Object)
                .Should()
                .BeFalse();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnityTestExtensions.ResetStaticFields();
            }
        }
    }
}
