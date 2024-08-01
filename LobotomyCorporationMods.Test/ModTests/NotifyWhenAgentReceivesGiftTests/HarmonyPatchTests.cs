// SPDX-License-Identifier: MIT

#region

using System;
using FluentAssertions;
using LobotomyCorporationMods.NotifyWhenAgentReceivesGift;
using LobotomyCorporationMods.NotifyWhenAgentReceivesGift.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.NotifyWhenAgentReceivesGiftTests
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
        public void Class_AgentSlot_Method_SetFilter_logs_exceptions()
        {
            var times = 1;
            VerifyArgumentNullExceptionLogging(() => UnitModelPatchAttachEgoGift.Prefix(null, UnityTestExtensions.CreateEgoGiftModel()), Times.Exactly(times++));
            VerifyArgumentNullExceptionLogging(() => UnitModelPatchAttachEgoGift.Prefix(UnityTestExtensions.CreateAgentModel(), null), Times.Exactly(times));
        }

        [Fact]
        public void Class_UnitModel_Method_AttachEgoGift_is_patched_correctly()
        {
            // ReSharper disable once StringLiteralTypo
            var patch = typeof(UnitModelPatchAttachEgoGift);
            var originalClass = typeof(UnitModel);
            const string MethodName = nameof(UnitModel.AttachEGOgift);

            ValidatePatch(patch, originalClass, MethodName);
        }
    }
}
