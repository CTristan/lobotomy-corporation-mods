// SPDX-License-Identifier: MIT

#region

using System;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.BugFixes;
using LobotomyCorporationMods.Test.Extensions;
using Moq;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.BugFixesTests
{
    public class BugFixesModTests : IDisposable
    {
        protected const int FourTimes = 4;

        protected BugFixesModTests()
        {
            _ = new Harmony_Patch();
            var logger = new Mock<ILogger>();
            Harmony_Patch.Instance.SetLogger(logger.Object);
        }

        [NotNull]
        protected static ArmorCreature ArmorCreature => new ArmorCreature();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnityTestExtensions.ResetStaticFields();
            }
        }
    }
}
