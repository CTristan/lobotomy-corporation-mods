// SPDX-License-Identifier: MIT

#region

using System;
using CommandWindow;
using FluentAssertions;
using LobotomyCorporationMods.GiftAlertIcon;
using LobotomyCorporationMods.GiftAlertIcon.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.GiftAlertIconTests
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
        public void Class_ManagementSlot_Method_SetUi_is_patched_correctly()
        {
            var patch = typeof(ManagementSlotPatchSetUi);
            var originalClass = typeof(ManagementSlot);
            const string MethodName = nameof(ManagementSlot.SetUI);

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_ManagementSlot_Method_SetUi_logs_exceptions()
        {
            // Forcing null arguments to test exception logging.
            // ReSharper disable AssignNullToNotNullAttribute
            var times = 1;
            VerifyArgumentNullExceptionLogging(() => ManagementSlotPatchSetUi.Postfix(null, UnityTestExtensions.CreateUnitModel()), Times.Exactly(times++));
            VerifyArgumentNullExceptionLogging(() => ManagementSlotPatchSetUi.Postfix(UnityTestExtensions.CreateManagementSlot(), null), Times.Exactly(times));
            // ReSharper enable AssignNullToNotNullAttribute
        }
    }
}
