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

namespace LobotomyCorporationMods.Test.Mods.NotifyWhenAgentReceivesGift
{
    public sealed class HarmonyPatchTests
    {
        [Fact]
        public void Class_AgentSlot_Method_SetFilter_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);

            Action action = () => UnitModelPatchAttachEgoGift.Prefix(null, UnityTestExtensions.CreateEgoGiftModel());
            mockLogger.VerifyArgumentNullException(action);
            action = () => UnitModelPatchAttachEgoGift.Prefix(UnityTestExtensions.CreateAgentModel(), null);
            mockLogger.VerifyArgumentNullException(action, Times.Exactly(2));
        }

        [Fact]
        public void Class_UnitModel_Method_AttachEgoGift_is_patched_correctly()
        {
            // ReSharper disable once StringLiteralTypo
            var patch = typeof(UnitModelPatchAttachEgoGift);
            var originalClass = typeof(UnitModel);
            const string MethodName = nameof(UnitModel.AttachEGOgift);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
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
    }
}
