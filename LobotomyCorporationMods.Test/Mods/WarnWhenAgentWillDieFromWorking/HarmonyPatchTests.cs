// SPDX-License-Identifier: MIT

#region

using System;
using CommandWindow;
using FluentAssertions;
using LobotomyCorporationMods.Test.Extensions;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Patches;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.WarnWhenAgentWillDieFromWorking
{
    public sealed class HarmonyPatchTests
    {
        [Fact]
        public void Class_AgentSlot_Method_SetFilter_is_patched_correctly()
        {
            var patch = typeof(AgentSlotPatchSetFilter);
            var originalClass = typeof(AgentSlot);
            const string MethodName = "SetFilter";

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_AgentSlot_Method_SetFilter_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);

            static void Action() => AgentSlotPatchSetFilter.Postfix(null!, (AgentState)1);

            mockLogger.VerifyExceptionLogged<ArgumentNullException>(Action);
        }

        /// <summary>
        ///     Harmony requires the constructor to be public.
        /// </summary>
        [Fact]
        public void Constructor_is_public_and_externally_accessible()
        {
            Action action = () => _ = new Harmony_Patch();
            action.Should().NotThrow();
        }
    }
}
