// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe;
using LobotomyCorporationMods.DontChatMe.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests
{
    public sealed class HarmonyPatchTests : DontChatMeModTests
    {
        [Fact]
        public void Harmony_Patch_constructor_is_public_and_accessible()
        {
            Action act = () =>
            {
                _ = new Harmony_Patch();
            };

            act.Should().NotThrow();
        }

        [Fact]
        public void Singleton_wires_all_runtime_components()
        {
            var instance = Harmony_Patch.Instance;

            instance.Config.Should().NotBeNull();
            instance.GameAdapter.Should().NotBeNull();
            instance.CooldownGate.Should().NotBeNull();
            instance.IdempotencyCache.Should().NotBeNull();
            instance.Transport.Should().NotBeNull();
            instance.Dispatcher.Should().NotBeNull();
            instance.Pump.Should().NotBeNull();
        }

        [Fact]
        public void Class_GameManager_Method_Update_is_patched_correctly()
        {
            var patch = typeof(GameManagerPatchUpdate);
            var originalClass = typeof(GameManager);
            const string MethodName = "Update";

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }
    }
}
