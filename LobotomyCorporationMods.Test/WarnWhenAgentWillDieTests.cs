// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using CommandWindow;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Patches;
using UnityEngine;
using Xunit;
using Xunit.Extensions;

namespace LobotomyCorporationMods.Test
{
    public sealed class WarnWhenAgentWillDieTests
    {
        private const string DeadAgentString = "AgentState_Dead";
        private const int StatLevelFive = 100;
        private const int StatLevelFour = 84;
        private const int StatLevelOne = 29;
        private const int StatLevelThree = 64;
        private const int StatLevelTwo = 44;

        public WarnWhenAgentWillDieTests()
        {
            _ = new Harmony_Patch();
            var fileManager = TestExtensions.GetMockFileManager();
            Harmony_Patch.Instance.LoadData(fileManager.Object);
        }

        [Fact]
        public void No_false_positives()
        {
            var commandWindow = InitializeCommandWindow(TestData.DefaultCreatureModel);
            var agentSlot = TestData.DefaultAgentSlot;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            agentSlot.WorkFilterFill.color.Should().NotBe(commandWindow.DeadColor);
            agentSlot.WorkFilterText.text.Should().NotBe(DeadAgentString);
        }

        #region Code Coverage Tests

        [Fact]
        public void AgentSlotPatchSetFilter_Is_Untestable()
        {
            var creature = GetCreature(CreatureIds.Bloodbath);
            InitializeCommandWindow(creature);
            var agentSlot = TestData.DefaultAgentSlot;
            agentSlot.CurrentAgent.primaryStat.hp = 1;

            Action action = () => AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            action.ShouldThrowUnityException();
        }

        #endregion

        #region Happy Teddy Bear Tests

        [Fact]
        public void HappyTeddyBear_Will_Kill_Agent_If_Same_Agent_Sent_Twice_In_A_Row()
        {
            var creature = GetCreature(CreatureIds.HappyTeddyBear);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            creature.script = new HappyTeddy { lastAgent = agent };

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeTrue();
        }

        [Fact]
        public void HappyTeddyBear_Will_Not_Kill_Agent_If_Last_Agent_Was_Different()
        {
            var creature = GetCreature(CreatureIds.HappyTeddyBear);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            var lastAgent = TestData.DefaultAgentModel;
            lastAgent.instanceId += 1L;
            creature.script = new HappyTeddy { lastAgent = lastAgent };

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeFalse();
        }

        #endregion

        #region Bloodbath Tests

        [Fact]
        public void Bloodbath_Will_Kill_Agent_With_Fortitude_Of_One_And_Temperance_Above_One()
        {
            var creature = GetCreature(CreatureIds.Bloodbath);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.hp = StatLevelOne;
            agent.primaryStat.work = StatLevelFive;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeTrue();
        }

        [Fact]
        public void Bloodbath_Will_Kill_Agent_With_Temperance_Of_One_And_Fortitude_Above_One()
        {
            var creature = GetCreature(CreatureIds.Bloodbath);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.hp = StatLevelFive;
            agent.primaryStat.work = StatLevelOne;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(StatLevelTwo)]
        [InlineData(StatLevelThree)]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void Bloodbath_Will_Not_Kill_Agent_With_Fortitude_Greater_Than_One(int fortitude)
        {
            var creature = GetCreature(CreatureIds.Bloodbath);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.hp = fortitude;
            agent.primaryStat.work = StatLevelFive;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(StatLevelTwo)]
        [InlineData(StatLevelThree)]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void Bloodbath_Will_Not_Kill_Agent_With_Temperance_Greater_Than_One(int temperance)
        {
            var creature = GetCreature(CreatureIds.Bloodbath);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.hp = StatLevelFive;
            agent.primaryStat.work = temperance;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeFalse();
        }

        #endregion

        #region Crumbling Armor Tests

        [Fact]
        public void CrumblingArmor_Will_Kill_Agent_With_Fortitude_Of_One()
        {
            var creature = GetCreature(CreatureIds.CrumblingArmor);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.hp = StatLevelOne;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(EquipmentId.CrumblingArmorGift1)]
        [InlineData(EquipmentId.CrumblingArmorGift2)]
        [InlineData(EquipmentId.CrumblingArmorGift3)]
        [InlineData(EquipmentId.CrumblingArmorGift4)]
        public void CrumblingArmor_Will_Kill_Agent_With_Gift_If_Performing_Attachment_Work(EquipmentId equipmentId)
        {
            var creature = GetCreature(CreatureIds.CrumblingArmor);
            const RwbpType SkillType = RwbpType.B;
            var commandWindow = InitializeCommandWindow(creature, SkillType);
            var agent = GetAgentWithGift(equipmentId);
            agent.primaryStat.hp = StatLevelFive;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(StatLevelTwo)]
        [InlineData(StatLevelThree)]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void CrumblingArmor_Will_Not_Kill_Agent_With_Fortitude_Greater_Than_One(int fortitude)
        {
            var creature = GetCreature(CreatureIds.CrumblingArmor);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.hp = fortitude;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(EquipmentId.CrumblingArmorGift1, RwbpType.R)]
        [InlineData(EquipmentId.CrumblingArmorGift1, RwbpType.W)]
        [InlineData(EquipmentId.CrumblingArmorGift1, RwbpType.P)]
        [InlineData(EquipmentId.CrumblingArmorGift2, RwbpType.R)]
        [InlineData(EquipmentId.CrumblingArmorGift2, RwbpType.W)]
        [InlineData(EquipmentId.CrumblingArmorGift2, RwbpType.P)]
        [InlineData(EquipmentId.CrumblingArmorGift3, RwbpType.R)]
        [InlineData(EquipmentId.CrumblingArmorGift3, RwbpType.W)]
        [InlineData(EquipmentId.CrumblingArmorGift3, RwbpType.P)]
        [InlineData(EquipmentId.CrumblingArmorGift4, RwbpType.R)]
        [InlineData(EquipmentId.CrumblingArmorGift4, RwbpType.W)]
        [InlineData(EquipmentId.CrumblingArmorGift4, RwbpType.P)]
        public void CrumblingArmor_Will_Not_Kill_Agent_With_Gift_If_Not_Performing_Attachment_Work(EquipmentId giftId, RwbpType skillType)
        {
            var creature = GetCreature(CreatureIds.CrumblingArmor);
            var commandWindow = InitializeCommandWindow(creature, skillType);
            var agent = GetAgentWithGift(giftId);
            agent.primaryStat.hp = StatLevelFive;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeFalse();
        }

        #endregion

        #region Harmony Tests

        /// <summary>
        ///     Harmony requires the constructor to be public.
        /// </summary>
        [Fact]
        public void Constructor_is_public_and_externally_accessible()
        {
            Action action = () => _ = new Harmony_Patch();
            action.ShouldNotThrow();
        }

        [Fact]
        public void Class_AgentSlot_Method_SetFilter_is_patched_correctly()
        {
            var patch = typeof(AgentSlotPatchSetFilter);
            var originalClass = typeof(AgentSlot);
            const string MethodName = "SetFilter";

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        #endregion

        #region Helper Methods

        [NotNull]
        private static AgentModel GetAgentWithGift(EquipmentId giftId)
        {
            var agent = TestData.DefaultAgentModel;
            var gift = TestData.DefaultEgoGiftModel;
            gift.metaInfo.id = (int)giftId;
            agent.Equipment.gifts.addedGifts.Add(gift);

            return agent;
        }

        [NotNull]
        private static CreatureModel GetCreature(CreatureIds creatureId)
        {
            var creature = TestExtensions.CreateCreatureModel(TestData.DefaultAgentModel, TestData.DefaultCreatureLayer, TestData.DefaultCreatureTypeInfo, TestData.DefaultCreatureObserveInfoModel,
                TestData.DefaultSkillTypeInfo);
            creature.instanceId = (long)creatureId;
            creature.metadataId = (long)creatureId;

            // Need to initialize the CreatureLayer with our new creature
            var creatureUnit = TestExtensions.CreateCreatureUnit();
            TestExtensions.CreateCreatureLayer(new Dictionary<long, CreatureUnit> { { (long)creatureId, creatureUnit } });

            return creature;
        }

        [NotNull]
        private static CommandWindow.CommandWindow InitializeCommandWindow(UnitModel currentTarget, RwbpType rwbpType = (RwbpType)1)
        {
            SkillTypeInfo[] skillTypeInfos = { new SkillTypeInfo { id = (long)rwbpType } };
            var skillTypeList = TestExtensions.CreateSkillTypeList(skillTypeInfos);

            var commandWindow = TestExtensions.CreateCommandWindow(currentTarget, CommandType.Management, (long)rwbpType, skillTypeList);
            commandWindow.DeadColor = Color.red;

            return commandWindow;
        }

        #endregion
    }
}
