// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using CommandWindow;
using LobotomyCorporationMods.Test.Extensions;
using Hemocode.WarnWhenAgentWillDieFromWorking;
using Hemocode.WarnWhenAgentWillDieFromWorking.Patches;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.WarnWhenAgentWillDieFromWorkingTests
{
    public sealed class HarmonyPatchTests
    {
        [Fact]
        public void Class_AgentSlot_Method_SetFilter_is_patched_correctly()
        {
            var patch = typeof(AgentSlotPatchSetFilter);
            var originalClass = typeof(AgentSlot);
            const string MethodName = nameof(AgentSlot.SetFilter);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_AgentSlot_Method_SetFilter_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);

            static void Action()
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                // Forcing null argument to test exception logging.
                AgentSlotPatchSetFilter.PostfixWithLogging(() => null, (AgentState)1, () => null);
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
