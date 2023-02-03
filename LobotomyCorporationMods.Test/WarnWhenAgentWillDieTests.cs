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

namespace LobotomyCorporationMods.Test
{
    public sealed class WarnWhenAgentWillDieTests
    {
        private const string DeadAgentString = "AgentState_Dead";

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

        #region Bloodbath Tests

        [Fact]
        public void Bloodbath_Will_Kill_Agent_With_Fortitude_Of_One()
        {
            var creature = GetCreature(CreatureIds.Bloodbath);
            var commandWindow = InitializeCommandWindow(creature);
            var agent = TestData.DefaultAgentModel;
            agent.primaryStat.hp = 1;

            var result = agent.CheckIfWorkWillKillAgent(commandWindow);

            result.Should().BeTrue();
        }

        #endregion

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
