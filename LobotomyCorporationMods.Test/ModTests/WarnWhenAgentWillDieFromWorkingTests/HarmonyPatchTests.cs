// SPDX-License-Identifier: MIT

#region

using System;
using CommandWindow;
using FluentAssertions;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Patches;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.WarnWhenAgentWillDieFromWorkingTests
{
    public sealed class HarmonyPatchTests : HarmonyPatchTestBase
    {
        public HarmonyPatchTests()
        {
            Harmony_Patch.Instance.AddLoggerTarget(MockLogger.Object);
        }

        /// <summary>Harmony requires the constructor to be public.</summary>
        [Fact]
        public void Constructor_is_public_and_externally_accessible()
        {
            Action action = () =>
            {
                _ = new Harmony_Patch();
            };

            action.Should().NotThrow();
        }

        [Fact]
        public void Class_AgentSlot_Method_SetFilter_is_patched_correctly()
        {
            var patch = typeof(AgentSlotPatchSetFilter);
            var originalClass = typeof(AgentSlot);
            const string MethodName = nameof(AgentSlot.SetFilter);

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_AgentSlot_Method_SetFilter_logs_exceptions()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            // Forcing null argument to test exception logging.
            VerifyArgumentNullExceptionLogging(() => AgentSlotPatchSetFilter.Postfix(null, (AgentState)1));
        }
    }
}
