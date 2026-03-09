// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using CommandWindow;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.GiftAlertIcon;
using LobotomyCorporationMods.GiftAlertIcon.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.GiftAlertIconTests
{
    public sealed class HarmonyPatchTests
    {
        [Fact]
        public void Class_ManagementSlot_Method_SetUi_is_patched_correctly()
        {
            Type patch = typeof(ManagementSlotPatchSetUi);
            Type originalClass = typeof(ManagementSlot);
            const string MethodName = nameof(ManagementSlot.SetUI);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_ManagementSlot_Method_SetUi_logs_exceptions()
        {
            Mock<ILogger> mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);

            // Forcing null arguments to test exception logging.
            // ReSharper disable AssignNullToNotNullAttribute
            int times = 1;
            Action action = () => ManagementSlotPatchSetUi.Postfix(null, UnityTestExtensions.CreateUnitModel());
            mockLogger.VerifyArgumentNullException(action, Times.Exactly(times++));
            action = () => ManagementSlotPatchSetUi.Postfix(UnityTestExtensions.CreateManagementSlot(), null);
            mockLogger.VerifyArgumentNullException(action, Times.Exactly(times));
            // ReSharper enable AssignNullToNotNullAttribute
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
