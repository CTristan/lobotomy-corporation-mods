// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.BugFixes;
using LobotomyCorporationMods.Common.Interfaces;
using Moq;

#endregion

namespace LobotomyCorporationMods.Test.BugFixesTests
{
    public class BugFixesTests
    {
        protected BugFixesTests()
        {
            _ = new Harmony_Patch();
            var logger = new Mock<ILogger>();
            Harmony_Patch.Instance.LoadData(logger.Object);
        }
    }
}
