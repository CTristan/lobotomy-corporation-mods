// SPDX-License-Identifier: MIT

#region

using System;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Test.Extensions;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.CommonTests.AdapterTests
{
    /// <summary>
    ///     Since adapter classes are wrappers for Unity system calls, testing the methods will always fail. These are mostly
    ///     for code coverage.
    /// </summary>
    public sealed class CustomizingWindowAdapterTests
    {
        [Fact]
        public void UpgradeAgentStat_throws_Unity_exception()
        {
            var customizingWindow = TestExtensions.CreateCustomizingWindow();
            var adapter = new CustomizingWindowAdapter(customizingWindow);

            Action action = () => adapter.UpgradeAgentStat(1, 1, 1);

            action.ShouldThrowUnityException();
        }
    }
}
