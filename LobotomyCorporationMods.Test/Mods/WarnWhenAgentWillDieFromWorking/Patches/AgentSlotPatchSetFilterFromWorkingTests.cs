// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using CommandWindow;
using FluentAssertions;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Test.Extensions;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Patches;
using Moq;
using Xunit;
using Xunit.Extensions;

#endregion

namespace LobotomyCorporationMods.Test.Mods.WarnWhenAgentWillDieFromWorking.Patches
{
    public sealed class AgentSlotPatchSetFilterTests : WarnWhenAgentWillDieFromWorkingTests
    {
        private readonly Mock<IImageAdapter> _mockImageAdapter;
        private readonly Mock<ITextAdapter> _mockTextAdapter;

        public AgentSlotPatchSetFilterTests()
        {
            _mockImageAdapter = new Mock<IImageAdapter>();
            _mockImageAdapter.SetupProperty(static adapter => adapter.Color);
            AgentSlotPatchSetFilter.WorkFilterFillAdapter = _mockImageAdapter.Object;

            _mockTextAdapter = new Mock<ITextAdapter>();
            _mockTextAdapter.SetupProperty(static adapter => adapter.Text);
            AgentSlotPatchSetFilter.WorkFilterTextAdapter = _mockTextAdapter.Object;
        }

        [Fact]
        public void No_false_positives()
        {
            var creature = TestExtensions.CreateCreatureModel();
            _ = InitializeCommandWindow(creature);
            var agentSlot = TestExtensions.CreateAgentSlot();
            AgentSlotPatchSetFilter.BeastAnimAdapter = new Mock<IBeautyBeastAnimAdapter>().Object;
            AgentSlotPatchSetFilter.GameObjectAdapter = new Mock<IGameObjectAdapter>().Object;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            agentSlot.WorkFilterFill.color.Should().NotBe(DeadAgentColor);
            agentSlot.WorkFilterText.text.Should().NotBe(DeadAgentString);
        }

        [Fact]
        public void Tool_does_not_show_if_agent_will_die()
        {
            var tool = TestExtensions.CreateUnitModel();
            _ = InitializeCommandWindow(tool);
            var agentSlot = TestExtensions.CreateAgentSlot();

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            agentSlot.WorkFilterFill.color.Should().NotBe(DeadAgentColor);
            agentSlot.WorkFilterText.text.Should().NotBe(DeadAgentString);
        }

        #region Beauty and the Beast Tests

        [Fact]
        public void BeautyAndTheBeast_Will_Not_Kill_Agent_If_Performing_Repression_Work_While_Not_Weak()
        {
            // Arrange
            var creature = GetCreature(CreatureIds.BeautyAndTheBeast);
            _ = InitializeCommandWindow(creature, RwbpType.P);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);

            // Mock animation script adapter to avoid Unity errors
            const int NormalState = 0;
            var mockAnimAdapter = new Mock<IBeautyBeastAnimAdapter>();
            mockAnimAdapter.Setup(static adapter => adapter.State).Returns(NormalState);
            AgentSlotPatchSetFilter.BeastAnimAdapter = mockAnimAdapter.Object;

            // Act
            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            // Assert
            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
        }

        [Fact]
        public void BeautyAndTheBeast_Will_Kill_Agent_If_Performing_Repression_Work_While_Weak()
        {
            // Arrange
            var creature = GetCreature(CreatureIds.BeautyAndTheBeast);
            _ = InitializeCommandWindow(creature, RwbpType.P);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);

            // Mock animation script adapter to avoid Unity errors
            const int WeakenedState = 1;
            var mockAnimAdapter = new Mock<IBeautyBeastAnimAdapter>();
            mockAnimAdapter.Setup(static adapter => adapter.State).Returns(WeakenedState);
            AgentSlotPatchSetFilter.BeastAnimAdapter = mockAnimAdapter.Object;

            // Act
            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            // Assert
            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
        }

        [Theory]
        [InlineData(RwbpType.R)]
        [InlineData(RwbpType.W)]
        [InlineData(RwbpType.B)]
        public void BeautyAndTheBeast_Will_Not_Kill_Agent_If_Not_Performing_Repression_Work_While_Weak(RwbpType skillType)
        {
            // Arrange
            var creature = GetCreature(CreatureIds.BeautyAndTheBeast);
            _ = InitializeCommandWindow(creature, skillType);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);

            // Mock animation script adapter to avoid Unity errors
            const int WeakenedState = 1;
            var mockAnimAdapter = new Mock<IBeautyBeastAnimAdapter>();
            mockAnimAdapter.Setup(static adapter => adapter.State).Returns(WeakenedState);
            AgentSlotPatchSetFilter.BeastAnimAdapter = mockAnimAdapter.Object;

            // Act
            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            // Assert
            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
        }

        #endregion

        #region Bloodbath Tests

        [Fact]
        public void Bloodbath_Will_Kill_Agent_With_Fortitude_Of_One_And_Temperance_Above_One()
        {
            var creature = GetCreature(CreatureIds.Bloodbath);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.hp = StatLevelOne;
            agent.primaryStat.work = StatLevelFive;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
        }

        [Fact]
        public void Bloodbath_Will_Kill_Agent_With_Temperance_Of_One_And_Fortitude_Above_One()
        {
            var creature = GetCreature(CreatureIds.Bloodbath);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.hp = StatLevelFive;
            agent.primaryStat.work = StatLevelOne;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
        }

        [Theory]
        [InlineData(StatLevelTwo)]
        [InlineData(StatLevelThree)]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void Bloodbath_Will_Not_Kill_Agent_With_Fortitude_Greater_Than_One(int fortitude)
        {
            var creature = GetCreature(CreatureIds.Bloodbath);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.hp = fortitude;
            agent.primaryStat.work = StatLevelFive;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
        }

        [Theory]
        [InlineData(StatLevelTwo)]
        [InlineData(StatLevelThree)]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void Bloodbath_Will_Not_Kill_Agent_With_Temperance_Greater_Than_One(int temperance)
        {
            var creature = GetCreature(CreatureIds.Bloodbath);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.hp = StatLevelFive;
            agent.primaryStat.work = temperance;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
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
        public void BlueStar_Will_Kill_Agent_With_Prudence_Less_Than_Five_And_Temperance_Greater_Than_Three(int prudence, int temperance)
        {
            var creature = GetCreature(CreatureIds.BlueStar);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.mental = prudence;
            agent.primaryStat.work = temperance;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
        }

        [Theory]
        [InlineData(StatLevelOne)]
        [InlineData(StatLevelTwo)]
        [InlineData(StatLevelThree)]
        public void BlueStar_Will_Kill_Agent_With_Prudence_Five_And_Temperance_Less_Than_Four(int temperance)
        {
            var creature = GetCreature(CreatureIds.BlueStar);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.hp = StatLevelFive;
            agent.primaryStat.work = temperance;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
        }

        [Theory]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void BlueStar_Will_Not_Kill_Agent_With_Prudence_Five_And_Temperance_Greater_Than_Three(int temperance)
        {
            var creature = GetCreature(CreatureIds.BlueStar);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.mental = StatLevelFive;
            agent.primaryStat.work = temperance;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
        }

        #endregion

        #region Crumbling Armor Tests

        [Fact]
        public void CrumblingArmor_Will_Kill_Agent_With_Fortitude_Of_One()
        {
            var creature = GetCreature(CreatureIds.CrumblingArmor);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.hp = StatLevelOne;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
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
            _ = InitializeCommandWindow(creature, SkillType);
            var agent = GetAgentWithGift(equipmentId);
            agent.primaryStat.hp = StatLevelFive;
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
        }

        [Theory]
        [InlineData(StatLevelTwo)]
        [InlineData(StatLevelThree)]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void CrumblingArmor_Will_Not_Kill_Agent_With_Fortitude_Greater_Than_One(int fortitude)
        {
            var creature = GetCreature(CreatureIds.CrumblingArmor);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.hp = fortitude;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
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
            _ = InitializeCommandWindow(creature, skillType);
            var agent = GetAgentWithGift(giftId);
            agent.primaryStat.hp = StatLevelFive;
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
        }

        #endregion

        #region Fairy Festival Tests

        [Fact]
        public void FairyFestival_Will_Kill_Agent_With_Buff_That_Works_On_Another_Creature()
        {
            var creature = GetCreature(CreatureIds.OneSin);
            _ = InitializeCommandWindow(creature);
            var buffList = new List<UnitBuf> { TestExtensions.CreateFairyBuf() };
            var agent = TestExtensions.CreateAgentModel(bufList: buffList);
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
        }

        [Fact]
        public void FairyFestival_Will_Not_Kill_Agent_With_Buff_If_Working_On_FairyFestival()
        {
            var creature = GetCreature(CreatureIds.FairyFestival);
            _ = InitializeCommandWindow(creature);
            var buffList = new List<UnitBuf> { TestExtensions.CreateFairyBuf() };
            var agent = TestExtensions.CreateAgentModel(bufList: buffList);
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
        }

        #endregion

        #region Laetitia Tests

        [Fact]
        public void Laetitia_Will_Kill_Agent_With_Buff_That_Works_On_Another_Creature()
        {
            var creature = GetCreature(CreatureIds.OneSin);
            _ = InitializeCommandWindow(creature);
            var buffList = new List<UnitBuf> { TestExtensions.CreateLittleWitchBuf() };
            var agent = TestExtensions.CreateAgentModel(bufList: buffList);
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
        }

        [Fact]
        public void Laetitia_Will_Not_Kill_Agent_With_Buff_If_Working_On_Laetitia()
        {
            var creature = GetCreature(CreatureIds.Laetitia);
            _ = InitializeCommandWindow(creature);
            var buffList = new List<UnitBuf> { TestExtensions.CreateLittleWitchBuf() };
            var agent = TestExtensions.CreateAgentModel(bufList: buffList);
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
        }

        #endregion

        #region Happy Teddy Bear Tests

        [Fact]
        public void HappyTeddyBear_Will_Kill_Agent_If_Same_Agent_Sent_Twice_In_A_Row()
        {
            var creature = GetCreature(CreatureIds.HappyTeddyBear);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            creature.script = new HappyTeddy { lastAgent = agent };

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
        }

        [Fact]
        public void HappyTeddyBear_Will_Not_Kill_Agent_If_Last_Agent_Was_Different()
        {
            var creature = GetCreature(CreatureIds.HappyTeddyBear);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            var lastAgent = TestExtensions.CreateAgentModel();
            lastAgent.instanceId += 1L;
            creature.script = new HappyTeddy { lastAgent = lastAgent };

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
        }

        [Fact]
        public void HappyTeddyBear_Will_Not_Kill_Agent_If_This_Is_The_First_Agent()
        {
            var creature = GetCreature(CreatureIds.HappyTeddyBear);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            creature.script = new HappyTeddy();

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
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
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.hp = fortitude;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
        }

        [Theory]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void NothingThere_Will_Not_Kill_Agent_With_Fortitude_Greater_Than_Three(int fortitude)
        {
            var creature = GetCreature(CreatureIds.NothingThere);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.hp = fortitude;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
        }

        [Fact]
        public void NothingThere_Will_Kill_Agent_If_Disguised()
        {
            var creature = GetCreature(CreatureIds.NothingThere);
            creature.script = new Nothing();
            ((Nothing)creature.script).copiedWorker = TestExtensions.CreateAgentModel();
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.hp = StatLevelFive;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
        }

        [Fact]
        public void NothingThere_Will_Not_Kill_Agent_If_Not_Disguised()
        {
            var creature = GetCreature(CreatureIds.NothingThere);
            creature.script = new Nothing();
            ((Nothing)creature.script).copiedWorker = null;
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.hp = StatLevelFive;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
        }

        #endregion

        #region Parasite Tree Tests

        [Fact]
        public void ParasiteTree_Will_Kill_Agent_If_Tree_Has_Four_Flowers_And_Agent_Is_Not_Blessed()
        {
            // Arrange
            var creature = GetCreature(CreatureIds.ParasiteTree);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);

            // Mock game object adapter to avoid Unity errors
            var mockFlower = new Mock<IGameObjectAdapter>();
            mockFlower.Setup(static adapter => adapter.ActiveSelf).Returns(true);

            var mockGameAdapter = new Mock<IGameObjectAdapter>();
            mockGameAdapter.Setup(static adapter => adapter.ActiveSelf).Returns(true);

            AgentSlotPatchSetFilter.GameObjectAdapter = mockGameAdapter.Object;

            // Act
            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            // Assert
            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
        }

        [Fact]
        public void ParasiteTree_Will_Not_Kill_Agent_If_Tree_Has_Four_Flowers_And_Agent_Is_Blessed()
        {
            // Arrange
            var creature = GetCreature(CreatureIds.ParasiteTree);
            _ = InitializeCommandWindow(creature);

            var parasiteTreeBlessing = TestExtensions.CreateYggdrasilBlessBuf();
            parasiteTreeBlessing.type = UnitBufType.YGGDRASIL_BLESS;

            var buffList = new List<UnitBuf> { parasiteTreeBlessing };
            var agent = TestExtensions.CreateAgentModel(bufList: buffList);
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);

            // Mock game object adapter to avoid Unity errors
            var mockGameObjectAdapter = new Mock<IGameObjectAdapter>();
            mockGameObjectAdapter.Setup(static adapter => adapter.ActiveSelf).Returns(true);

            AgentSlotPatchSetFilter.GameObjectAdapter = mockGameObjectAdapter.Object;

            // Act
            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            // Assert
            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void ParasiteTree_Will_Not_Kill_Agent_If_Tree_Has_Less_Than_Four_Flowers(int numberOfFlowers)
        {
            // Arrange
            var creature = GetCreature(CreatureIds.ParasiteTree);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);

            // Mock game object adapter to avoid Unity errors
            var mockGameObjectAdapter = new Mock<IGameObjectAdapter>();
            mockGameObjectAdapter.Setup(static adapter => adapter.ActiveSelf).Returns(true);
            AgentSlotPatchSetFilter.GameObjectAdapter = mockGameObjectAdapter.Object;

            // Act
            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            // Assert
            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
        }

        #endregion

        #region Red Shoes Tests

        [Theory]
        [InlineData(StatLevelOne)]
        [InlineData(StatLevelTwo)]
        public void RedShoes_Will_Kill_Agent_With_Temperance_Less_Than_Three(int temperance)
        {
            var creature = GetCreature(CreatureIds.RedShoes);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.work = temperance;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
        }

        [Theory]
        [InlineData(StatLevelThree)]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void RedShoes_Will_Kill_Not_Agent_With_Temperance_Greater_Than_Two(int temperance)
        {
            var creature = GetCreature(CreatureIds.RedShoes);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.work = temperance;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
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
            _ = InitializeCommandWindow(creature, skillType);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.mental = StatLevelOne;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
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
            _ = InitializeCommandWindow(creature, skillType);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.mental = prudence;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
        }

        [Theory]
        [InlineData(StatLevelTwo)]
        [InlineData(StatLevelThree)]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void SpiderBud_Will_Kill_Agent_With_Prudence_Greater_Than_One_And_Performing_Insight_Work(int prudence)
        {
            var creature = GetCreature(CreatureIds.SpiderBud);
            _ = InitializeCommandWindow(creature, RwbpType.W);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.mental = prudence;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
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
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.hp = fortitude;
            agent.primaryStat.work = temperance;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
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
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.hp = fortitude;
            agent.primaryStat.work = StatLevelThree;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
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
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.hp = fortitude;
            agent.primaryStat.work = temperance;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
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
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.hp = fortitude;
            agent.primaryStat.work = temperance;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
        }

        #endregion

        #region Void Dream Tests

        [Fact]
        public void VoidDream_Will_Kill_Agent_With_Temperance_Of_One()
        {
            var creature = GetCreature(CreatureIds.VoidDream);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.work = StatLevelOne;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
        }

        [Theory]
        [InlineData(StatLevelTwo)]
        [InlineData(StatLevelThree)]
        [InlineData(StatLevelFour)]
        [InlineData(StatLevelFive)]
        public void VoidDream_Will_Not_Kill_Agent_With_Temperance_Greater_Than_One(int temperance)
        {
            var creature = GetCreature(CreatureIds.VoidDream);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);
            agent.primaryStat.work = temperance;

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
        }

        #endregion

        #region Warm-Hearted Woodsman Tests

        [Fact]
        public void WarmHeartedWoodsman_Will_Kill_Agent_If_Qliphoth_Counter_Is_Zero()
        {
            var creature = GetCreature(CreatureIds.WarmHeartedWoodsman);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeTrue();
        }

        [Fact]
        public void WarmHeartedWoodsman_Will_Not_Kill_Agent_If_Qliphoth_Counter_Is_Greater_Than_Zero()
        {
            const int QliphothCounterOne = 1;
            var creature = GetCreature(CreatureIds.WarmHeartedWoodsman, QliphothCounterOne);
            _ = InitializeCommandWindow(creature);
            var agent = TestExtensions.CreateAgentModel();
            var agentSlot = TestExtensions.CreateAgentSlot(currentAgent: agent);

            AgentSlotPatchSetFilter.Postfix(agentSlot, AgentState.IDLE);

            AgentWillDie(_mockImageAdapter.Object, _mockTextAdapter.Object).Should().BeFalse();
        }

        #endregion
    }
}
