// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using CommandWindow;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Test.Extensions;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Patches;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.WarnWhenAgentWillDieFromWorkingTests
{
    public sealed class HarmonyPatchTests
    {
        [Fact]
        public void Class_AgentSlot_Method_SetFilter_is_patched_correctly()
        {
            Type patch = typeof(AgentSlotPatchSetFilter);
            Type originalClass = typeof(AgentSlot);
            const string MethodName = nameof(AgentSlot.SetFilter);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_AgentSlot_Method_SetFilter_logs_exceptions()
        {
            Mock<ILogger> mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);

            static void Action()
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                // Forcing null argument to test exception logging.
                AgentSlotPatchSetFilter.Postfix(null, (AgentState)1);
            }

            mockLogger.VerifyArgumentNullException(Action);
        }

        /// <summary>Harmony requires the constructor to be public.</summary>
        [Fact]
        public void Constructor_is_public_and_externally_accessible()
        {
            Action action = () =>
            {
                _ = new Harmony_Patch();
            };

            _ = action.Should().NotThrow();
        }
    }
}
