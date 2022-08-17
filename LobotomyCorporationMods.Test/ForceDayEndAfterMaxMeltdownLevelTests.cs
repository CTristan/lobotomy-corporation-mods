// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using CommandWindow;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.ForceDayEndAfterMaxMeltdownLevel;
using Xunit;
using Xunit.Extensions;

namespace LobotomyCorporationMods.Test
{
    public sealed class ForceDayEndAfterMaxMeltdownLevelTests
    {
        private const AgentState DefaultAgentState = AgentState.IDLE;
        private const CommandType DefaultCommandType = CommandType.Management;
        private const long DefaultCreatureId = 1L;
        private const int DefaultQliphothOverloadCounter = 1;
        private const RwbpType DefaultSelectedWork = RwbpType.R;
        private const int MaxMeltdownLevel = 10;

        public ForceDayEndAfterMaxMeltdownLevelTests()
        {
            _ = new Harmony_Patch();
            var fileManager = TestExtensions.CreateFileManager();
            Harmony_Patch.Instance.LoadData(fileManager);
        }

        /// <summary>
        ///     Harmony requires the constructor to be public.
        /// </summary>
        [Fact]
        public void Constructor_is_public_and_externally_accessible()
        {
            Action act = () => _ = new Harmony_Patch();
            act.ShouldNotThrow();
        }

        [Theory]
        [InlineData(AgentState.DEAD)]
        [InlineData(AgentState.PANIC)]
        public void Do_not_change_agent_work_slot_when_the_agent_cannot_be_controlled(AgentState agentState)
        {
            var agentSlot = TestExtensions.CreateAgentSlot(agentState);
            var commandWindow = GetDefaultCommandWindow();
            var creatureOverloadManager = GetDefaultCreatureOverloadManager();

            agentSlot.DisableIfMaxMeltdown(agentState, commandWindow, creatureOverloadManager);

            agentSlot.State.Should().Be(agentState);
        }

        [Fact]
        public void Do_not_change_agent_work_slot_if_no_work_has_been_selected_yet()
        {
            var agentSlot = GetDefaultAgentSlot();
            var commandWindow = TestExtensions.CreateCommandWindow(GetDefaultCreatureModel(), DefaultCommandType, null);
            var creatureOverloadManager = GetDefaultCreatureOverloadManager();

            agentSlot.DisableIfMaxMeltdown(DefaultAgentState, commandWindow, creatureOverloadManager);

            agentSlot.State.Should().Be(DefaultAgentState);
        }

        [Theory]
        [InlineData(CommandType.KitCreature)]
        [InlineData(CommandType.Suppress)]
        public void Do_not_change_agent_work_slot_if_window_type_is_not_management(CommandType windowType)
        {
            var agentSlot = GetDefaultAgentSlot();
            var commandWindow = TestExtensions.CreateCommandWindow(GetDefaultCreatureModel(), windowType, DefaultSelectedWork);
            var creatureOverloadManager = GetDefaultCreatureOverloadManager();

            agentSlot.DisableIfMaxMeltdown(DefaultAgentState, commandWindow, creatureOverloadManager);

            agentSlot.State.Should().Be(DefaultAgentState);
        }

        [Fact]
        public void Do_not_change_work_agent_slot_if_command_window_is_not_for_an_abnormality()
        {
            const long UnitId = 1L;
            var agentSlot = GetDefaultAgentSlot();
            var currentTarget = TestExtensions.CreateUnitModel(UnitId);
            var commandWindow = TestExtensions.CreateCommandWindow(currentTarget, DefaultCommandType, DefaultSelectedWork);
            var creatureOverloadManager = GetDefaultCreatureOverloadManager();

            agentSlot.DisableIfMaxMeltdown(DefaultAgentState, commandWindow, creatureOverloadManager);

            agentSlot.State.Should().Be(DefaultAgentState);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(9)]
        public void Do_not_disable_the_agent_work_slot_if_we_are_not_at_the_maximum_meltdown_level(int qliphothCounter)
        {
            var agentSlot = GetDefaultAgentSlot();
            var commandWindow = GetDefaultCommandWindow();
            var creatureOverloadManager = TestExtensions.CreateCreatureOverloadManager(qliphothCounter);

            agentSlot.DisableIfMaxMeltdown(DefaultAgentState, commandWindow, creatureOverloadManager);

            agentSlot.State.Should().Be(DefaultAgentState);
        }

        [Fact]
        public void Do_not_disable_the_agent_work_slot_if_the_room_is_in_meltdown_even_when_at_maximum_meltdown_level()
        {
            var agentSlot = GetDefaultAgentSlot();
            var isolateOverload = TestExtensions.CreateIsolateOverload(true);
            var isolateRoom = TestExtensions.CreateIsolateRoom(isolateOverload);
            var creatureUnit = TestExtensions.CreateCreatureUnit(isolateRoom);
            var creatureModel = TestExtensions.CreateCreatureModel(DefaultCreatureId, GetDefaultCreatureObserveInfoModel(), creatureUnit);
            var commandWindow = TestExtensions.CreateCommandWindow(creatureModel, DefaultCommandType, DefaultSelectedWork);
            var creatureOverloadManager = TestExtensions.CreateCreatureOverloadManager(MaxMeltdownLevel);

            agentSlot.DisableIfMaxMeltdown(DefaultAgentState, commandWindow, creatureOverloadManager);

            agentSlot.State.Should().Be(DefaultAgentState);
        }

        [Fact]
        public void Disable_the_agent_work_slot_when_we_are_at_the_maximum_meltdown_level()
        {
            var agentSlot = GetDefaultAgentSlot();
            var commandWindow = GetDefaultCommandWindow();
            var creatureOverloadManager = TestExtensions.CreateCreatureOverloadManager(MaxMeltdownLevel);

            agentSlot.DisableIfMaxMeltdown(DefaultAgentState, commandWindow, creatureOverloadManager);

            agentSlot.State.Should().Be(AgentState.UNCONTROLLABLE);
        }

        #region Helper Methods

        [NotNull]
        private static AgentSlot GetDefaultAgentSlot()
        {
            return TestExtensions.CreateAgentSlot(DefaultAgentState);
        }

        [NotNull]
        private static CommandWindow.CommandWindow GetDefaultCommandWindow()
        {
            return TestExtensions.CreateCommandWindow(GetDefaultCreatureModel(), DefaultCommandType, DefaultSelectedWork);
        }

        [NotNull]
        private static CreatureModel GetDefaultCreatureModel()
        {
            return TestExtensions.CreateCreatureModel(DefaultCreatureId, GetDefaultCreatureObserveInfoModel(), GetDefaultCreatureUnit());
        }

        [NotNull]
        private static CreatureObserveInfoModel GetDefaultCreatureObserveInfoModel()
        {
            return TestExtensions.CreateCreatureObserveInfoModel(GetDefaultCreatureTypeInfo(), GetDefaultObserveRegions());
        }

        [NotNull]
        private static CreatureOverloadManager GetDefaultCreatureOverloadManager()
        {
            return TestExtensions.CreateCreatureOverloadManager(DefaultQliphothOverloadCounter);
        }

        [NotNull]
        private static CreatureTypeInfo GetDefaultCreatureTypeInfo()
        {
            return TestExtensions.CreateCreatureTypeInfo();
        }

        [NotNull]
        private static CreatureUnit GetDefaultCreatureUnit()
        {
            return TestExtensions.CreateCreatureUnit(GetDefaultIsolateRoom());
        }

        [NotNull]
        private static IsolateRoom GetDefaultIsolateRoom()
        {
            return TestExtensions.CreateIsolateRoom(GetDefaultIsolateOverload());
        }

        [NotNull]
        private static IsolateOverload GetDefaultIsolateOverload()
        {
            return TestExtensions.CreateIsolateOverload(false);
        }

        [NotNull]
        private static ObserveRegion GetDefaultObserveRegion()
        {
            return new ObserveRegion { isObserved = true };
        }

        [NotNull]
        private static Dictionary<string, ObserveRegion> GetDefaultObserveRegions()
        {
            return new Dictionary<string, ObserveRegion>
            {
                { "stat", GetDefaultObserveRegion() },
                { "defense", GetDefaultObserveRegion() },
                { "work_r", GetDefaultObserveRegion() },
                { "work_w", GetDefaultObserveRegion() },
                { "work_b", GetDefaultObserveRegion() },
                { "work_p", GetDefaultObserveRegion() }
            };
        }

        #endregion
    }
}
