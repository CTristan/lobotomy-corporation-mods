// SPDX-License-Identifier: MIT

using System;
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
            var fileManager = TestExtensions.CreateFileManager();
            Harmony_Patch.Instance.LoadData(fileManager);
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

        #region Helper Methods

        [CanBeNull]
        private static CreatureModel GetCreature(CreatureIds creatureId)
        {
            var creature = TestExtensions.CreateCreatureModel(TestData.DefaultAgentModel, TestData.DefaultCreatureTypeInfo, TestData.DefaultCreatureObserveInfoModel, TestData.DefaultSkillTypeInfo);
            creature.metadataId = (long)creatureId;

            return creature;
        }

        [NotNull]
        private static CommandWindow.CommandWindow InitializeCommandWindow(UnitModel currentTarget)
        {
            var commandWindow = TestExtensions.CreateCommandWindow(currentTarget, CommandType.Management, TestData.DefaultSkillTypeList);
            commandWindow.DeadColor = Color.red;

            return commandWindow;
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
    }
}
