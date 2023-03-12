// SPDX-License-Identifier: MIT

#region

using System;
using FluentAssertions;
using LobotomyCorporationMods.NotifyWhenAgentReceivesGift;
using LobotomyCorporationMods.NotifyWhenAgentReceivesGift.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.NotifyWhenAgentReceivesGift
{
    public sealed class HarmonyPatchTests
    {
        [Fact]
        public void Class_AgentSlot_Method_SetFilter_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);

            Action action = static () => UnitModelPatchAttachEgoGift.Prefix(null!, TestUnityExtensions.CreateEgoGiftModel());
            mockLogger.VerifyExceptionLogged<ArgumentNullException>(action);
            action = static () => UnitModelPatchAttachEgoGift.Prefix(TestUnityExtensions.CreateAgentModel(), null!);
            mockLogger.VerifyExceptionLogged<ArgumentNullException>(action, 2);
        }

        [Fact]
        public void Class_UnitModel_Method_AttachEgoGift_is_patched_correctly()
        {
            // ReSharper disable once StringLiteralTypo
            const string MethodName = "AttachEGOgift";
            var patch = typeof(UnitModelPatchAttachEgoGift);
            var originalClass = typeof(UnitModel);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        /// <summary>
        ///     Harmony requires the constructor to be public.
        /// </summary>
        [Fact]
        public void Constructor_is_public_and_externally_accessible()
        {
            Action action = () => _ = new Harmony_Patch();
            action.ShouldNotThrow();
        }
    }
}
