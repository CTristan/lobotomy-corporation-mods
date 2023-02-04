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
        private const int StatLevelFive = 85;
        private const int StatLevelFour = 65;
        private const int StatLevelOne = 1;
        private const int StatLevelThree = 45;
        private const int StatLevelTwo = 30;

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

        #region Nothing There Tests

        [Theory]
        [InlineData(StatLevelOne)]
        [InlineData(StatLevelTwo)]
        [InlineData(StatLevelThree)]
        public void NothingThere_Will_Kill_Agent_With_Fortitude_Less_Than_Four(int fortitude)
        {
            var creature = GetCreature(CreatureIds.NothingThere);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.hp = fortitude;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void NothingThere_Will_Not_Kill_Agent_With_Fortitude_Greater_Than_Three(int fortitude)
        {
            var creature = GetCreature(CreatureIds.NothingThere);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.hp = fortitude;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeFalse();
        }

        [Fact]
        public void NothingThere_Will_Kill_Agent_If_Disguised()
        {
            var creature = GetCreature(CreatureIds.NothingThere);
            creature.script = new Nothing();
            ((Nothing)creature.script).copiedWorker = TestData.DefaultAgentModel;
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.hp = StatLevelFive;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeTrue();
        }

        [Fact]
        public void NothingThere_Will_Not_Kill_Agent_If_Not_Disguised()
        {
            var creature = GetCreature(CreatureIds.NothingThere);
            creature.script = new Nothing();
            ((Nothing)creature.script).copiedWorker = null;
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.hp = StatLevelFive;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeFalse();
        }

        #endregion

        #region Red Shoes Tests

        [Theory]
        [InlineData(StatLevelOne)]
        [InlineData(StatLevelTwo)]
        public void RedShoes_Will_Kill_Agent_With_Temperance_Less_Than_Three(int temperance)
        {
            var creature = GetCreature(CreatureIds.RedShoes);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.work = temperance;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(StatLevelThree)]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void RedShoes_Will_Kill_Not_Agent_With_Temperance_Greater_Than_Two(int temperance)
        {
            var creature = GetCreature(CreatureIds.RedShoes);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.work = temperance;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeFalse();
        }

        #endregion

        #region Spider Bud Tests

        [Theory]
        [InlineData(RwbpType.R)]
        [InlineData(RwbpType.B)]
        [InlineData(RwbpType.P)]
        public void SpiderBud_Will_Kill_Agent_With_Prudence_Of_One_And_Not_Performing_Insight_Work(RwbpType skillType)
        {
            var creature = GetCreature(CreatureIds.SpiderBud);
            var commandWindow = InitializeCommandWindow(creature, skillType);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.mental = StatLevelOne;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(StatLevelTwo, RwbpType.R)]
        [InlineData(StatLevelTwo, RwbpType.B)]
        [InlineData(StatLevelTwo, RwbpType.P)]
        [InlineData(StatLevelThree, RwbpType.R)]
        [InlineData(StatLevelThree, RwbpType.B)]
        [InlineData(StatLevelThree, RwbpType.P)]
        [InlineData(StatLevelFour, RwbpType.R)]
        [InlineData(StatLevelFour, RwbpType.B)]
        [InlineData(StatLevelFour, RwbpType.P)]
        [InlineData(StatLevelFive, RwbpType.R)]
        [InlineData(StatLevelFive, RwbpType.B)]
        [InlineData(StatLevelFive, RwbpType.P)]
        public void SpiderBud_Will_Kill_Not_Agent_With_Prudence_Greater_Than_One_And_Not_Performing_Insight_Work(int prudence, RwbpType skillType)
        {
            var creature = GetCreature(CreatureIds.SpiderBud);
            var commandWindow = InitializeCommandWindow(creature, skillType);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.mental = prudence;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(StatLevelTwo)]
        [InlineData(StatLevelThree)]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void SpiderBud_Will_Kill_Agent_With_Prudence_Greater_Than_One_And_Performing_Insight_Work(int prudence)
        {
            var creature = GetCreature(CreatureIds.SpiderBud);
            var commandWindow = InitializeCommandWindow(creature, RwbpType.W);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.mental = prudence;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeTrue();
        }

        #endregion

        #region Singing Machine Tests

        /// <summary>
        ///     Agent dies due to high fortitude.
        /// </summary>
        [Theory]
        [InlineData(StatLevelFour, StatLevelThree)]
        [InlineData(StatLevelFour, StatLevelFour)]
        [InlineData(StatLevelFour, StatLevelFive)]
        [InlineData(StatLevelFive, StatLevelThree)]
        [InlineData(StatLevelFive, StatLevelFour)]
        [InlineData(StatLevelFive, StatLevelFive)]
        public void SingingMachine_Will_Kill_Agent_At_Qliphoth_Greater_Than_Zero_With_Fortitude_Greater_Than_Three_And_Temperance_Greater_Than_Two(int fortitude, int temperance)
        {
            const int QliphothCounterOne = 1;
            var creature = GetCreature(CreatureIds.SingingMachine, QliphothCounterOne);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.hp = fortitude;
            agent.primaryStat.work = temperance;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeTrue();
        }

        /// <summary>
        ///     Agent dies due to low temperance.
        /// </summary>
        [Theory]
        [InlineData(StatLevelOne, StatLevelOne)]
        [InlineData(StatLevelOne, StatLevelTwo)]
        [InlineData(StatLevelTwo, StatLevelOne)]
        [InlineData(StatLevelTwo, StatLevelTwo)]
        [InlineData(StatLevelThree, StatLevelOne)]
        [InlineData(StatLevelThree, StatLevelTwo)]
        public void SingingMachine_Will_Kill_Agent_At_Qliphoth_Greater_Than_Zero_With_Fortitude_Less_Than_Four_And_Temperance_Less_Than_Three(int fortitude, int temperance)
        {
            // Same test as high fortitude
            SingingMachine_Will_Kill_Agent_At_Qliphoth_Greater_Than_Zero_With_Fortitude_Greater_Than_Three_And_Temperance_Greater_Than_Two(fortitude, temperance);
        }

        /// <summary>
        ///     Singing Machine's gift gives +8 fortitude, so we need to make sure that the gift won't push an agent's fortitude to
        ///     4.
        /// </summary>
        [Theory]
        [InlineData(StatLevelFour - 1)]
        [InlineData(StatLevelFour - 2)]
        [InlineData(StatLevelFour - 3)]
        [InlineData(StatLevelFour - 4)]
        [InlineData(StatLevelFour - 5)]
        [InlineData(StatLevelFour - 6)]
        [InlineData(StatLevelFour - 7)]
        [InlineData(StatLevelFour - 8)]
        public void SingingMachine_Will_Kill_Agent_At_Qliphoth_Greater_Than_Zero_With_Fortitude_Three_Because_Gift_Will_Make_Fortitude_Greater_Than_Three(int fortitude)
        {
            const int QliphothCounterOne = 1;
            var creature = GetCreature(CreatureIds.SingingMachine, QliphothCounterOne);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.hp = fortitude;
            agent.primaryStat.work = StatLevelThree;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(StatLevelOne, StatLevelThree)]
        [InlineData(StatLevelOne, StatLevelFour)]
        [InlineData(StatLevelOne, StatLevelFive)]
        [InlineData(StatLevelTwo, StatLevelThree)]
        [InlineData(StatLevelTwo, StatLevelFour)]
        [InlineData(StatLevelTwo, StatLevelFive)]
        [InlineData(StatLevelThree, StatLevelThree)]
        [InlineData(StatLevelThree, StatLevelFour)]
        [InlineData(StatLevelThree, StatLevelFive)]
        public void SingingMachine_Will_Not_Kill_Agent_At_Qliphoth_Greater_Than_Zero_With_Fortitude_Less_Than_Four_And_Temperance_Greater_Than_Two(int fortitude, int temperance)
        {
            const int QliphothCounterOne = 1;
            var creature = GetCreature(CreatureIds.SingingMachine, QliphothCounterOne);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.hp = fortitude;
            agent.primaryStat.work = temperance;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(StatLevelOne, StatLevelOne)]
        [InlineData(StatLevelOne, StatLevelTwo)]
        [InlineData(StatLevelOne, StatLevelThree)]
        [InlineData(StatLevelOne, StatLevelFour)]
        [InlineData(StatLevelOne, StatLevelFive)]
        [InlineData(StatLevelTwo, StatLevelOne)]
        [InlineData(StatLevelTwo, StatLevelTwo)]
        [InlineData(StatLevelTwo, StatLevelThree)]
        [InlineData(StatLevelTwo, StatLevelFour)]
        [InlineData(StatLevelTwo, StatLevelFive)]
        [InlineData(StatLevelThree, StatLevelOne)]
        [InlineData(StatLevelThree, StatLevelTwo)]
        [InlineData(StatLevelThree, StatLevelThree)]
        [InlineData(StatLevelThree, StatLevelFour)]
        [InlineData(StatLevelThree, StatLevelFive)]
        [InlineData(StatLevelFour, StatLevelOne)]
        [InlineData(StatLevelFour, StatLevelTwo)]
        [InlineData(StatLevelFour, StatLevelThree)]
        [InlineData(StatLevelFour, StatLevelFour)]
        [InlineData(StatLevelFour, StatLevelFive)]
        [InlineData(StatLevelFive, StatLevelOne)]
        [InlineData(StatLevelFive, StatLevelTwo)]
        [InlineData(StatLevelFive, StatLevelThree)]
        [InlineData(StatLevelFive, StatLevelFour)]
        [InlineData(StatLevelFive, StatLevelFive)]
        public void SingingMachine_Will_Kill_Agent_At_Qliphoth_Zero_Regardless_Of_Fortitude_And_Temperance(int fortitude, int temperance)
        {
            var creature = GetCreature(CreatureIds.SingingMachine);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.hp = fortitude;
            agent.primaryStat.work = temperance;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeTrue();
        }

        #endregion

        #region Void Dream Tests

        [Fact]
        public void VoidDream_Will_Kill_Agent_With_Temperance_Of_One()
        {
            var creature = GetCreature(CreatureIds.VoidDream);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.work = StatLevelOne;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(StatLevelTwo)]
        [InlineData(StatLevelThree)]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void VoidDream_Will_Not_Kill_Agent_With_Temperance_Greater_Than_One(int temperance)
        {
            var creature = GetCreature(CreatureIds.VoidDream);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.work = temperance;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeFalse();
        }

        #endregion

        #region Warm-Hearted Woodsman Tests

        [Fact]
        public void WarmHeartedWoodsman_Will_Kill_Agent_If_Qliphoth_Counter_Is_Zero()
        {
            var creature = GetCreature(CreatureIds.WarmHeartedWoodsman);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeTrue();
        }

        [Fact]
        public void WarmHeartedWoodsman_Will_Not_Kill_Agent_If_Qliphoth_Counter_Is_Greater_Than_Zero()
        {
            const int QliphothCounterOne = 1;
            var creature = GetCreature(CreatureIds.WarmHeartedWoodsman, QliphothCounterOne);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;

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
        private static CreatureModel GetCreature(CreatureIds creatureId, int qliphothCounter = 0)
        {
            var creature = TestExtensions.CreateCreatureModel(TestData.DefaultAgentModel, TestData.DefaultCreatureLayer, TestData.DefaultCreatureTypeInfo, TestData.DefaultCreatureObserveInfoModel,
                qliphothCounter, TestData.DefaultSkillTypeInfo);
            creature.instanceId = (long)creatureId;
            creature.metadataId = (long)creatureId;
            SetMaxObservation(creature);

            // Need to initialize the CreatureLayer with our new creature
            var creatureUnit = TestExtensions.CreateCreatureUnit();
            TestExtensions.CreateCreatureLayer(new Dictionary<long, CreatureUnit> { { (long)creatureId, creatureUnit } });

            return creature;
        }

        private static void SetMaxObservation([NotNull] CreatureModel creature)
        {
            var observeRegions = new List<ObserveInfoData>
            {
                new ObserveInfoData { regionName = "stat" },
                new ObserveInfoData { regionName = "defense" },
                new ObserveInfoData { regionName = "work_r" },
                new ObserveInfoData { regionName = "work_w" },
                new ObserveInfoData { regionName = "work_b" },
                new ObserveInfoData { regionName = "work_p" }
            };
            creature.observeInfo.InitObserveRegion(observeRegions);
            creature.observeInfo.ObserveAll();
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
