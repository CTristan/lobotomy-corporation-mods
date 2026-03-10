// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;
using LobotomyCorporationMods.BugFixes;
using LobotomyCorporationMods.Common.Interfaces;
using Moq;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.BugFixesTests
{
    public class BugFixesModTests
    {
        protected const int FourTimes = 4;

        protected BugFixesModTests()
        {
            _ = new Harmony_Patch();
            Mock<ILogger> logger = new();
            Harmony_Patch.Instance.AddLoggerTarget(logger.Object);
        }

        [NotNull]
        protected static ArmorCreature ArmorCreature => new();
    }
}
