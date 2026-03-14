// SPDX-License-Identifier: MIT

#region

using System.Runtime.CompilerServices;
using AwesomeAssertions;
using LobotomyCorporationMods.DebugPanel.Patches;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests.PatchTests
{
    public sealed class GlobalGameManagerPatchAwakeTests : DebugPanelModTests
    {
        [Fact]
        public void PatchAfterAwake_returns_false_when_debugPanelAlreadyExists()
        {
            var instance = (GlobalGameManager)RuntimeHelpers.GetUninitializedObject(typeof(GlobalGameManager));

            var result = instance.PatchAfterAwake(true);

            _ = result.Should().BeFalse();
        }

        [Fact]
        public void PatchAfterAwake_returns_true_when_debugPanel_does_not_exist()
        {
            var instance = (GlobalGameManager)RuntimeHelpers.GetUninitializedObject(typeof(GlobalGameManager));

            var result = instance.PatchAfterAwake(false);

            _ = result.Should().BeTrue();
        }
    }
}
