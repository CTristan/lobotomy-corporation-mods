// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using CommandWindow;
using FluentAssertions;
using LobotomyCorporationMods.Common.Enums;
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
            Harmony_Patch.Instance.LoadData(mockLogger.Object);
        }

        private Color DeadAgentColor { get; } = Color.red;
        protected GameManager GameManager { get; } = TestUnityExtensions.CreateGameManager();
        protected Mock<IBeautyBeastAnimAdapter> MockBeautyBeastAnimAdapter { get; } = new();
        protected Mock<IImageAdapter> MockImageAdapter { get; } = new();
        protected Mock<ITextAdapter> MockTextAdapter { get; } = new();
        protected Mock<IYggdrasilAnimAdapter> MockYggdrasilAnimAdapter { get; } = new();

        private bool AgentWillDie(IImageAdapter workFilterFill, ITextAdapter workFilterText)
        {
            var agentWillDie = workFilterFill.Color == DeadAgentColor && workFilterText.Text == DeadAgentString;

            return agentWillDie;
        }

        protected static AgentSlot InitializeAgentSlot(CreatureIds creatureId = CreatureIds.OneSin, IEnumerable<UnitBuf>? unitBufList = null, EquipmentId giftId = (EquipmentId)1,
            RwbpType skillType = (RwbpType)1, int qliphothCounter = 0)
        {
            unitBufList ??= new List<UnitBuf>();

            var creature = TestExtensions.GetCreatureWithGift(creatureId, qliphothCounter: qliphothCounter);
            _ = TestExtensions.InitializeCommandWindow(creature, skillType);
            var agent = TestExtensions.GetAgentWithGift(giftId, unitBufList);

            return TestUnityExtensions.CreateAgentSlot(currentAgent: agent);
        }

        protected static void SetupNothingThere(AgentSlot agentSlot, int fortitude)
        {
            SetupNothingThere(agentSlot, fortitude, false);
        }

        protected static void SetupNothingThere(AgentSlot agentSlot, int fortitude, bool isDisguised)
        {
            agentSlot.CurrentAgent.primaryStat.hp = fortitude;

            var creature = (CreatureModel)CommandWindow.CommandWindow.CurrentWindow.CurrentTarget;
            creature.script = new Nothing();
            ((Nothing)creature.script).copiedWorker = isDisguised ? TestUnityExtensions.CreateAgentModel() : null;
        }

        protected void SetupParasiteTree(int numberOfFlowers)
        {
            var mockFlower = new Mock<IGameObjectAdapter>();
            mockFlower.Setup(static adapter => adapter.ActiveSelf).Returns(true);

            var mockFlowers = new List<IGameObjectAdapter>();
            for (var i = 0; i < numberOfFlowers; i++)
            {
                mockFlowers.Add(mockFlower.Object);
            }

            MockYggdrasilAnimAdapter.Setup(static adapter => adapter.Flowers).Returns(mockFlowers);
        }

        protected void VerifyAgentWillDie(AgentSlot agentSlot)
        {
            agentSlot.PatchAfterSetFilter(IdleAgentState, GameManager, MockBeautyBeastAnimAdapter.Object, MockImageAdapter.Object, MockTextAdapter.Object, MockYggdrasilAnimAdapter.Object);

            AgentWillDie(MockImageAdapter.Object, MockTextAdapter.Object).Should().BeTrue();
        }

        protected void VerifyAgentWillNotDie(AgentSlot agentSlot)
        {
            agentSlot.PatchAfterSetFilter(IdleAgentState, GameManager, MockBeautyBeastAnimAdapter.Object, MockImageAdapter.Object, MockTextAdapter.Object, MockYggdrasilAnimAdapter.Object);

            AgentWillDie(MockImageAdapter.Object, MockTextAdapter.Object).Should().BeFalse();
        }
    }
}
