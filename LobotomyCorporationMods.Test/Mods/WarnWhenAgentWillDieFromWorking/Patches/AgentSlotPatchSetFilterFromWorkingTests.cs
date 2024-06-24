// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using CommandWindow;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Test.Extensions;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Patches;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.WarnWhenAgentWillDieFromWorking.Patches
{
    public sealed class AgentSlotPatchSetFilterTests : WarnWhenAgentWillDieFromWorkingTests
    {
        private const AgentState IdleAgentState = AgentState.IDLE;
        private const RwbpType SkillTypeAttachment = RwbpType.B;
        private const RwbpType SkillTypeInsight = RwbpType.W;
        private const RwbpType SkillTypeRepression = RwbpType.P;
        private readonly GameManager _gameManager = TestExtensions.CreateGameManager();

        private readonly Mock<IBeautyBeastAnimAdapter>
            _mockBeautyBeastAnimAdapter = new Mock<IBeautyBeastAnimAdapter>();

        private readonly Mock<IImageAdapter> _mockImageAdapter = new Mock<IImageAdapter>();
        private readonly Mock<ITextAdapter> _mockTextAdapter = new Mock<ITextAdapter>();
        private readonly Mock<IYggdrasilAnimAdapter> _mockYggdrasilAnimAdapter = new Mock<IYggdrasilAnimAdapter>();

        public AgentSlotPatchSetFilterTests()
        {
            _mockImageAdapter.SetupProperty(adapter => adapter.Color);
            _mockTextAdapter.SetupProperty(adapter => adapter.Text);

            _gameManager.ManageStarted = true;
        }

        [Fact]
        public void Does_not_error_if_command_window_is_not_Management_window()
        {
            var creature = TestExtensions.CreateCreatureModel();
            _ = TestExtensions.CreateCommandWindow(creature, CommandType.Suppress);
            var agentSlot = TestExtensions.CreateAgentSlot();

            Action action = () =>
                agentSlot.PatchAfterSetFilter(IdleAgentState, _gameManager, _mockBeautyBeastAnimAdapter.Object,
                    _mockImageAdapter.Object, _mockTextAdapter.Object, _mockYggdrasilAnimAdapter.Object);

            action.Should().NotThrow();
        }

        [Fact]
        public void Does_not_error_if_we_are_not_in_Management_phase()
        {
            var creature = TestExtensions.CreateCreatureModel();
            _ = InitializeCommandWindow(creature);
            var agentSlot = TestExtensions.CreateAgentSlot();
            _gameManager.ManageStarted = false;

            Action action = () =>
                agentSlot.PatchAfterSetFilter(IdleAgentState, _gameManager, _mockBeautyBeastAnimAdapter.Object,
                    _mockImageAdapter.Object, _mockTextAdapter.Object, _mockYggdrasilAnimAdapter.Object);

            action.Should().NotThrow();
        }

        [Fact]
        public void Does_not_error_on_first_game_load()
        {
            var creature = TestExtensions.CreateCreatureModel();
            _ = InitializeCommandWindow(creature);
            var agentSlot = TestExtensions.CreateAgentSlot();

            // Send a null game manager to indicate this is our first game load
            Action action = () =>
                agentSlot.PatchAfterSetFilter(IdleAgentState, null, _mockBeautyBeastAnimAdapter.Object,
                    _mockImageAdapter.Object, _mockTextAdapter.Object, _mockYggdrasilAnimAdapter.Object);

            action.Should().NotThrow();
        }

        [Fact]
        public void No_false_positives()
        {
            _ = InitializeCommandWindow(TestExtensions.CreateCreatureModel());
            var agentSlot = TestExtensions.CreateAgentSlot();

            VerifyAgentWillNotDie(agentSlot);
        }

        [Fact]
        public void Tool_does_not_show_if_agent_will_die()
        {
            var tool = TestExtensions.CreateUnitModel();
            _ = InitializeCommandWindow(tool);
            var agentSlot = TestExtensions.CreateAgentSlot();

            VerifyAgentWillNotDie(agentSlot);
        }

        [Theory]
        [InlineData(AgentState.DEAD)]
        [InlineData(AgentState.PANIC)]
        [InlineData(AgentState.UNCONTROLLABLE)]
        public void Uncontrollable_agent_is_not_checked_even_if_they_would_die(AgentState agentState)
        {
            // Arrange
            // Using Fairy Festival as simple agent-would-die scenario
            var buffList = new List<UnitBuf> { TestExtensions.CreateFairyBuf() };
            var agentSlot = InitializeAgentSlot(CreatureIds.OneSin, buffList);

            // Act
            agentSlot.PatchAfterSetFilter(agentState, _gameManager, _mockBeautyBeastAnimAdapter.Object,
                _mockImageAdapter.Object, _mockTextAdapter.Object, _mockYggdrasilAnimAdapter.Object);

            // Assert
            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
        }

        #region Beauty and the Beast Tests

        [Fact]
        public void BeautyAndTheBeast_Will_Not_Kill_Agent_If_Performing_Repression_Work_While_Not_Weak()
        {
            // Arrange
            var agentSlot = InitializeAgentSlot(CreatureIds.BeautyAndTheBeast, skillType: SkillTypeRepression);

            // Mock animation script adapter to avoid Unity errors
            const int NormalState = 0;
            _mockBeautyBeastAnimAdapter.Setup(adapter => adapter.State).Returns(NormalState);

            // Assert
            VerifyAgentWillNotDie(agentSlot);
        }

        [Fact]
        public void BeautyAndTheBeast_Will_Kill_Agent_If_Performing_Repression_Work_While_Weak()
        {
            // Arrange
            var agentSlot = InitializeAgentSlot(CreatureIds.BeautyAndTheBeast, skillType: SkillTypeRepression);

            // Mock animation script adapter to avoid Unity errors
            const int WeakenedState = 1;
            _mockBeautyBeastAnimAdapter.Setup(adapter => adapter.State).Returns(WeakenedState);

            // Assert
            VerifyAgentWillDie(agentSlot);
        }

        [Theory]
        [InlineData(RwbpType.R)]
        [InlineData(RwbpType.W)]
        [InlineData(RwbpType.B)]
        public void BeautyAndTheBeast_Will_Not_Kill_Agent_If_Not_Performing_Repression_Work_While_Weak(
            RwbpType skillType)
        {
            // Arrange
            var agentSlot = InitializeAgentSlot(CreatureIds.BeautyAndTheBeast, skillType: skillType);

            // Mock animation script adapter to avoid Unity errors
            const int WeakenedState = 1;
            _mockBeautyBeastAnimAdapter.Setup(adapter => adapter.State).Returns(WeakenedState);

            // Assert
            VerifyAgentWillNotDie(agentSlot);
        }

        #endregion

        #region Bloodbath Tests

        [Fact]
        public void Bloodbath_Will_Kill_Agent_With_Fortitude_Of_One_And_Temperance_Above_One()
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.Bloodbath);
            agentSlot.CurrentAgent.primaryStat.hp = StatLevelOne;
            agentSlot.CurrentAgent.primaryStat.work = StatLevelFive;

            VerifyAgentWillDie(agentSlot);
        }

        [Fact]
        public void Bloodbath_Will_Kill_Agent_With_Temperance_Of_One_And_Fortitude_Above_One()
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.Bloodbath);
            agentSlot.CurrentAgent.primaryStat.hp = StatLevelFive;
            agentSlot.CurrentAgent.primaryStat.work = StatLevelOne;

            VerifyAgentWillDie(agentSlot);
        }

        [Theory]
        [InlineData(StatLevelTwo)]
        [InlineData(StatLevelThree)]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void Bloodbath_Will_Not_Kill_Agent_With_Fortitude_Greater_Than_One(int fortitude)
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.Bloodbath);
            agentSlot.CurrentAgent.primaryStat.hp = fortitude;
            agentSlot.CurrentAgent.primaryStat.work = StatLevelFive;

            VerifyAgentWillNotDie(agentSlot);
        }

        [Theory]
        [InlineData(StatLevelTwo)]
        [InlineData(StatLevelThree)]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void Bloodbath_Will_Not_Kill_Agent_With_Temperance_Greater_Than_One(int temperance)
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.Bloodbath);
            agentSlot.CurrentAgent.primaryStat.hp = StatLevelFive;
            agentSlot.CurrentAgent.primaryStat.work = temperance;

            VerifyAgentWillNotDie(agentSlot);
        }

        #endregion

        #region Blue Star Tests

        [Theory]
        [InlineData(StatLevelOne, StatLevelFour)]
        [InlineData(StatLevelOne, StatLevelFive)]
        [InlineData(StatLevelTwo, StatLevelFour)]
        [InlineData(StatLevelTwo, StatLevelFive)]
        [InlineData(StatLevelThree, StatLevelFour)]
        [InlineData(StatLevelThree, StatLevelFive)]
        [InlineData(StatLevelFour, StatLevelFour)]
        [InlineData(StatLevelFour, StatLevelFive)]
        public void BlueStar_Will_Kill_Agent_With_Prudence_Less_Than_Five_And_Temperance_Greater_Than_Three(
            int prudence, int temperance)
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.BlueStar);
            agentSlot.CurrentAgent.primaryStat.mental = prudence;
            agentSlot.CurrentAgent.primaryStat.work = temperance;

            VerifyAgentWillDie(agentSlot);
        }

        [Theory]
        [InlineData(StatLevelOne)]
        [InlineData(StatLevelTwo)]
        [InlineData(StatLevelThree)]
        public void BlueStar_Will_Kill_Agent_With_Prudence_Five_And_Temperance_Less_Than_Four(int temperance)
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.BlueStar);
            agentSlot.CurrentAgent.primaryStat.hp = StatLevelFive;
            agentSlot.CurrentAgent.primaryStat.work = temperance;

            VerifyAgentWillDie(agentSlot);
        }

        [Theory]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void BlueStar_Will_Not_Kill_Agent_With_Prudence_Five_And_Temperance_Greater_Than_Three(int temperance)
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.BlueStar);
            agentSlot.CurrentAgent.primaryStat.mental = StatLevelFive;
            agentSlot.CurrentAgent.primaryStat.work = temperance;

            VerifyAgentWillNotDie(agentSlot);
        }

        #endregion

        #region Crumbling Armor Tests

        [Fact]
        public void CrumblingArmor_Will_Kill_Agent_With_Fortitude_Of_One()
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.CrumblingArmor);
            agentSlot.CurrentAgent.primaryStat.hp = StatLevelOne;

            VerifyAgentWillDie(agentSlot);
        }

        [Theory]
        [InlineData(EquipmentId.CrumblingArmorGift1)]
        [InlineData(EquipmentId.CrumblingArmorGift2)]
        [InlineData(EquipmentId.CrumblingArmorGift3)]
        [InlineData(EquipmentId.CrumblingArmorGift4)]
        public void CrumblingArmor_Will_Kill_Agent_With_Gift_If_Performing_Attachment_Work(EquipmentId equipmentId)
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.CrumblingArmor, giftId: equipmentId,
                skillType: SkillTypeAttachment);
            agentSlot.CurrentAgent.primaryStat.hp = StatLevelFive;

            VerifyAgentWillDie(agentSlot);
        }

        [Theory]
        [InlineData(StatLevelTwo)]
        [InlineData(StatLevelThree)]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void CrumblingArmor_Will_Not_Kill_Agent_With_Fortitude_Greater_Than_One(int fortitude)
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.CrumblingArmor);
            agentSlot.CurrentAgent.primaryStat.hp = fortitude;

            VerifyAgentWillNotDie(agentSlot);
        }

        [Theory]
        [InlineData(EquipmentId.CrumblingArmorGift1, RwbpType.R)]
        [InlineData(EquipmentId.CrumblingArmorGift1, RwbpType.W)]
        [InlineData(EquipmentId.CrumblingArmorGift1, SkillTypeRepression)]
        [InlineData(EquipmentId.CrumblingArmorGift2, RwbpType.R)]
        [InlineData(EquipmentId.CrumblingArmorGift2, RwbpType.W)]
        [InlineData(EquipmentId.CrumblingArmorGift2, SkillTypeRepression)]
        [InlineData(EquipmentId.CrumblingArmorGift3, RwbpType.R)]
        [InlineData(EquipmentId.CrumblingArmorGift3, RwbpType.W)]
        [InlineData(EquipmentId.CrumblingArmorGift3, SkillTypeRepression)]
        [InlineData(EquipmentId.CrumblingArmorGift4, RwbpType.R)]
        [InlineData(EquipmentId.CrumblingArmorGift4, RwbpType.W)]
        [InlineData(EquipmentId.CrumblingArmorGift4, SkillTypeRepression)]
        public void CrumblingArmor_Will_Not_Kill_Agent_With_Gift_If_Not_Performing_Attachment_Work(EquipmentId giftId,
            RwbpType skillType)
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.CrumblingArmor, giftId: giftId, skillType: skillType);
            agentSlot.CurrentAgent.primaryStat.hp = StatLevelFive;

            VerifyAgentWillNotDie(agentSlot);
        }

        #endregion

        #region Fairy Festival Tests

        [Fact]
        public void FairyFestival_Will_Kill_Agent_With_Buff_That_Works_On_Another_Creature()
        {
            var buffList = new List<UnitBuf> { TestExtensions.CreateFairyBuf() };
            var agentSlot = InitializeAgentSlot(CreatureIds.OneSin, buffList);

            VerifyAgentWillDie(agentSlot);
        }

        [Fact]
        public void FairyFestival_Will_Not_Kill_Agent_With_Buff_If_Working_On_FairyFestival()
        {
            var buffList = new List<UnitBuf> { TestExtensions.CreateFairyBuf() };
            var agentSlot = InitializeAgentSlot(CreatureIds.FairyFestival, buffList);

            VerifyAgentWillNotDie(agentSlot);
        }

        #endregion

        #region Laetitia Tests

        [Fact]
        public void Laetitia_Will_Kill_Agent_With_Buff_That_Works_On_Another_Creature()
        {
            var buffList = new List<UnitBuf> { TestExtensions.CreateLittleWitchBuf() };
            var agentSlot = InitializeAgentSlot(CreatureIds.OneSin, buffList);

            VerifyAgentWillDie(agentSlot);
        }

        [Fact]
        public void Laetitia_Will_Not_Kill_Agent_With_Buff_If_Working_On_Laetitia()
        {
            var buffList = new List<UnitBuf> { TestExtensions.CreateLittleWitchBuf() };
            var agentSlot = InitializeAgentSlot(CreatureIds.Laetitia, buffList);

            VerifyAgentWillNotDie(agentSlot);
        }

        #endregion

        #region Happy Teddy Bear Tests

        [Fact]
        public void HappyTeddyBear_Will_Kill_Agent_If_Same_Agent_Sent_Twice_In_A_Row()
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.HappyTeddyBear);
            ((CreatureModel)CommandWindow.CommandWindow.CurrentWindow.CurrentTarget).script =
                new HappyTeddy { lastAgent = agentSlot.CurrentAgent };

            VerifyAgentWillDie(agentSlot);
        }

        [Fact]
        public void HappyTeddyBear_Will_Not_Kill_Agent_If_Last_Agent_Was_Different()
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.HappyTeddyBear);
            var lastAgent = TestExtensions.CreateAgentModel();
            lastAgent.instanceId += 1L;
            ((CreatureModel)CommandWindow.CommandWindow.CurrentWindow.CurrentTarget).script =
                new HappyTeddy { lastAgent = lastAgent };

            VerifyAgentWillNotDie(agentSlot);
        }

        [Fact]
        public void HappyTeddyBear_Will_Not_Kill_Agent_If_This_Is_The_First_Agent()
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.HappyTeddyBear);
            ((CreatureModel)CommandWindow.CommandWindow.CurrentWindow.CurrentTarget).script = new HappyTeddy();

            VerifyAgentWillNotDie(agentSlot);
        }

        #endregion

        #region Nothing There Tests

        [Theory]
        [InlineData(StatLevelOne)]
        [InlineData(StatLevelTwo)]
        [InlineData(StatLevelThree)]
        public void NothingThere_Will_Kill_Agent_With_Fortitude_Less_Than_Four_While_Disguised(int fortitude)
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.NothingThere);
            SetupNothingThere(agentSlot, fortitude, true);

            VerifyAgentWillDie(agentSlot);
        }

        [Theory]
        [InlineData(StatLevelOne)]
        [InlineData(StatLevelTwo)]
        [InlineData(StatLevelThree)]
        public void NothingThere_Will_Not_Kill_Agent_With_Fortitude_Less_Than_Four_While_Not_Disguised(int fortitude)
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.NothingThere);
            SetupNothingThere(agentSlot, fortitude);

            VerifyAgentWillNotDie(agentSlot);
        }

        [Theory]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void NothingThere_Will_Not_Kill_Agent_With_Fortitude_Greater_Than_Three_While_Disguised(int fortitude)
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.NothingThere);
            SetupNothingThere(agentSlot, fortitude, true);

            VerifyAgentWillNotDie(agentSlot);
        }

        #endregion

        #region Parasite Tree Tests

        [Fact]
        public void ParasiteTree_Will_Kill_Agent_If_Tree_Has_Four_Flowers_And_Agent_Is_Not_Blessed()
        {
            // Arrange
            const int NumberOfFlowers = 4;
            var agentSlot = InitializeAgentSlot(CreatureIds.ParasiteTree);
            SetupParasiteTree(NumberOfFlowers);

            // Assert
            VerifyAgentWillDie(agentSlot);
        }

        [Fact]
        public void ParasiteTree_Will_Not_Kill_Agent_If_Tree_Has_Four_Flowers_And_Agent_Is_Blessed()
        {
            // Arrange
            const int NumberOfFlowers = 4;
            var parasiteTreeBlessing = TestExtensions.CreateYggdrasilBlessBuf();
            parasiteTreeBlessing.type = UnitBufType.YGGDRASIL_BLESS;
            var buffList = new List<UnitBuf> { parasiteTreeBlessing };
            var agentSlot = InitializeAgentSlot(CreatureIds.ParasiteTree, buffList);
            SetupParasiteTree(NumberOfFlowers);

            // Assert
            VerifyAgentWillNotDie(agentSlot);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void ParasiteTree_Will_Not_Kill_Agent_If_Tree_Has_Less_Than_Four_Flowers(int numberOfFlowers)
        {
            // Arrange
            var agentSlot = InitializeAgentSlot(CreatureIds.ParasiteTree);
            SetupParasiteTree(numberOfFlowers);

            // Assert
            VerifyAgentWillNotDie(agentSlot);
        }

        #endregion

        #region Red Shoes Tests

        [Theory]
        [InlineData(StatLevelOne)]
        [InlineData(StatLevelTwo)]
        public void RedShoes_Will_Kill_Agent_With_Temperance_Less_Than_Three(int temperance)
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.RedShoes);
            agentSlot.CurrentAgent.primaryStat.work = temperance;

            VerifyAgentWillDie(agentSlot);
        }

        [Theory]
        [InlineData(StatLevelThree)]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void RedShoes_Will_Kill_Not_Agent_With_Temperance_Greater_Than_Two(int temperance)
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.RedShoes);
            agentSlot.CurrentAgent.primaryStat.work = temperance;

            VerifyAgentWillNotDie(agentSlot);
        }

        #endregion

        #region Spider Bud Tests

        [Theory]
        [InlineData(RwbpType.R)]
        [InlineData(RwbpType.B)]
        [InlineData(SkillTypeRepression)]
        public void SpiderBud_Will_Kill_Agent_With_Prudence_Of_One_And_Not_Performing_Insight_Work(RwbpType skillType)
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.SpiderBud, skillType: skillType);
            agentSlot.CurrentAgent.primaryStat.mental = StatLevelOne;

            VerifyAgentWillDie(agentSlot);
        }

        [Theory]
        [InlineData(StatLevelTwo, RwbpType.R)]
        [InlineData(StatLevelTwo, RwbpType.B)]
        [InlineData(StatLevelTwo, SkillTypeRepression)]
        [InlineData(StatLevelThree, RwbpType.R)]
        [InlineData(StatLevelThree, RwbpType.B)]
        [InlineData(StatLevelThree, SkillTypeRepression)]
        [InlineData(StatLevelFour, RwbpType.R)]
        [InlineData(StatLevelFour, RwbpType.B)]
        [InlineData(StatLevelFour, SkillTypeRepression)]
        [InlineData(StatLevelFive, RwbpType.R)]
        [InlineData(StatLevelFive, RwbpType.B)]
        [InlineData(StatLevelFive, SkillTypeRepression)]
        public void SpiderBud_Will_Kill_Not_Agent_With_Prudence_Greater_Than_One_And_Not_Performing_Insight_Work(
            int prudence, RwbpType skillType)
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.SpiderBud, skillType: skillType);
            agentSlot.CurrentAgent.primaryStat.mental = prudence;

            VerifyAgentWillNotDie(agentSlot);
        }

        [Theory]
        [InlineData(StatLevelTwo)]
        [InlineData(StatLevelThree)]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void SpiderBud_Will_Kill_Agent_With_Prudence_Greater_Than_One_And_Performing_Insight_Work(int prudence)
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.SpiderBud, skillType: SkillTypeInsight);
            agentSlot.CurrentAgent.primaryStat.mental = prudence;

            VerifyAgentWillDie(agentSlot);
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
        public void
            SingingMachine_Will_Kill_Agent_At_Qliphoth_Greater_Than_Zero_With_Fortitude_Greater_Than_Three_And_Temperance_Greater_Than_Two(
                int fortitude, int temperance)
        {
            const int QliphothCounterOne = 1;
            var agentSlot = InitializeAgentSlot(CreatureIds.SingingMachine, qliphothCounter: QliphothCounterOne);
            agentSlot.CurrentAgent.primaryStat.hp = fortitude;
            agentSlot.CurrentAgent.primaryStat.work = temperance;

            VerifyAgentWillDie(agentSlot);
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
        public void
            SingingMachine_Will_Kill_Agent_At_Qliphoth_Greater_Than_Zero_With_Fortitude_Less_Than_Four_And_Temperance_Less_Than_Three(
                int fortitude, int temperance)
        {
            // Same test as high fortitude
            SingingMachine_Will_Kill_Agent_At_Qliphoth_Greater_Than_Zero_With_Fortitude_Greater_Than_Three_And_Temperance_Greater_Than_Two(
                fortitude, temperance);
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
        public void
            SingingMachine_Will_Kill_Agent_At_Qliphoth_Greater_Than_Zero_With_Fortitude_Three_Because_Gift_Will_Make_Fortitude_Greater_Than_Three(
                int fortitude)
        {
            const int QliphothCounterOne = 1;
            var agentSlot = InitializeAgentSlot(CreatureIds.SingingMachine, qliphothCounter: QliphothCounterOne);
            agentSlot.CurrentAgent.primaryStat.hp = fortitude;
            agentSlot.CurrentAgent.primaryStat.work = StatLevelThree;

            VerifyAgentWillDie(agentSlot);
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
        public void
            SingingMachine_Will_Not_Kill_Agent_At_Qliphoth_Greater_Than_Zero_With_Fortitude_Less_Than_Four_And_Temperance_Greater_Than_Two(
                int fortitude, int temperance)
        {
            const int QliphothCounterOne = 1;
            var agentSlot = InitializeAgentSlot(CreatureIds.SingingMachine, qliphothCounter: QliphothCounterOne);
            agentSlot.CurrentAgent.primaryStat.hp = fortitude;
            agentSlot.CurrentAgent.primaryStat.work = temperance;

            VerifyAgentWillNotDie(agentSlot);
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
        public void SingingMachine_Will_Kill_Agent_At_Qliphoth_Zero_Regardless_Of_Fortitude_And_Temperance(
            int fortitude, int temperance)
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.SingingMachine);
            agentSlot.CurrentAgent.primaryStat.hp = fortitude;
            agentSlot.CurrentAgent.primaryStat.work = temperance;

            VerifyAgentWillDie(agentSlot);
        }

        #endregion

        #region Void Dream Tests

        [Fact]
        public void VoidDream_Will_Kill_Agent_With_Temperance_Of_One()
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.VoidDream);
            agentSlot.CurrentAgent.primaryStat.work = StatLevelOne;

            VerifyAgentWillDie(agentSlot);
        }

        [Theory]
        [InlineData(StatLevelTwo)]
        [InlineData(StatLevelThree)]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void VoidDream_Will_Not_Kill_Agent_With_Temperance_Greater_Than_One(int temperance)
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.VoidDream);
            agentSlot.CurrentAgent.primaryStat.work = temperance;

            VerifyAgentWillNotDie(agentSlot);
        }

        #endregion

        #region Warm-Hearted Woodsman Tests

        [Fact]
        public void WarmHeartedWoodsman_Will_Kill_Agent_If_Qliphoth_Counter_Is_Zero()
        {
            var agentSlot = InitializeAgentSlot(CreatureIds.WarmHeartedWoodsman);

            VerifyAgentWillDie(agentSlot);
        }

        [Fact]
        public void WarmHeartedWoodsman_Will_Not_Kill_Agent_If_Qliphoth_Counter_Is_Greater_Than_Zero()
        {
            const int QliphothCounterOne = 1;
            var agentSlot = InitializeAgentSlot(CreatureIds.WarmHeartedWoodsman, qliphothCounter: QliphothCounterOne);

            VerifyAgentWillNotDie(agentSlot);
        }

        #endregion

        #region Helper Methods

        private static void SetupNothingThere([NotNull] AgentSlot agentSlot, int fortitude, bool isDisguised = false)
        {
            agentSlot.CurrentAgent.primaryStat.hp = fortitude;

            var creature = (CreatureModel)CommandWindow.CommandWindow.CurrentWindow.CurrentTarget;
            creature.script = new Nothing();
            ((Nothing)creature.script).copiedWorker = isDisguised ? TestExtensions.CreateAgentModel() : null;
        }

        private void SetupParasiteTree(int numberOfFlowers)
        {
            var mockFlower = new Mock<IGameObjectAdapter>();
            mockFlower.Setup(adapter => adapter.ActiveSelf).Returns(true);

            var mockFlowers = new List<IGameObjectAdapter>();
            for (var i = 0; i < numberOfFlowers; i++)
            {
                mockFlowers.Add(mockFlower.Object);
            }

            _mockYggdrasilAnimAdapter.Setup(adapter => adapter.Flowers).Returns(mockFlowers);
        }

        [NotNull]
        private AgentSlot InitializeAgentSlot(CreatureIds creatureId, IEnumerable<UnitBuf> buffList = null,
            EquipmentId giftId = (EquipmentId)1, RwbpType skillType = (RwbpType)1,
            int qliphothCounter = 0)
        {
            buffList = buffList ?? new List<UnitBuf>();
            var creature = GetCreature(creatureId, qliphothCounter);
            _ = InitializeCommandWindow(creature, skillType);
            var agent = GetAgentWithGift(giftId, buffList);

            return TestExtensions.CreateAgentSlot(currentAgent: agent);
        }

        private void VerifyAgentWillDie([NotNull] AgentSlot agentSlot)
        {
            agentSlot.PatchAfterSetFilter(IdleAgentState, _gameManager, _mockBeautyBeastAnimAdapter.Object,
                _mockImageAdapter.Object, _mockTextAdapter.Object, _mockYggdrasilAnimAdapter.Object);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
        }

        private void VerifyAgentWillNotDie([NotNull] AgentSlot agentSlot)
        {
            agentSlot.PatchAfterSetFilter(IdleAgentState, _gameManager, _mockBeautyBeastAnimAdapter.Object,
                _mockImageAdapter.Object, _mockTextAdapter.Object, _mockYggdrasilAnimAdapter.Object);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
        }

        #endregion
    }
}
