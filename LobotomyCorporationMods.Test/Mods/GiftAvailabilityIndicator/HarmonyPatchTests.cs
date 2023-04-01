// SPDX-License-Identifier: MIT

#region

using System;
using CommandWindow;
using FluentAssertions;
using LobotomyCorporationMods.GiftAvailabilityIndicator;
using LobotomyCorporationMods.GiftAvailabilityIndicator.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.GiftAvailabilityIndicator
{
    public sealed class HarmonyPatchTests
    {
        [Fact]
        public void Class_ManagementSlot_Method_SetUi_is_patched_correctly()
        {
            var patch = typeof(ManagementSlotPatchSetUi);
            var originalClass = typeof(ManagementSlot);
            const string MethodName = "SetUI";

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_ManagementSlot_Method_SetUi_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);

            var times = 1;
            Action action = static () => ManagementSlotPatchSetUi.Postfix(null!, TestUnityExtensions.CreateUnitModel());
            mockLogger.VerifyExceptionLogged<ArgumentNullException>(action, times++);
            action = static () => ManagementSlotPatchSetUi.Postfix(TestUnityExtensions.CreateManagementSlot(), null!);
            mockLogger.VerifyExceptionLogged<ArgumentNullException>(action, times);
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
